using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using static Game.Core.Utils;

namespace Game.Core.StateMachines.Game
{
	public class GameInitState : IState
	{
		public GameFSM FSM;

		public async UniTask Enter()
		{
			Globals.UI.SetDebugText("");

			_ = Globals.UI.FadeIn(Color.black, 0);

			FMODUnity.RuntimeManager.LoadBank("SFX", loadSamples: true);

			Globals.State.Version = Application.version;
			Globals.State.Commit = await ReadStreamingAsset("/commit.txt");
			Globals.State.TitleMusic = FMODUnity.RuntimeManager.CreateInstance(Globals.Config.MusicTitle);
			Globals.State.LevelMusic = FMODUnity.RuntimeManager.CreateInstance(Globals.Config.MusicMain);
			Globals.State.EndMusic = FMODUnity.RuntimeManager.CreateInstance(Globals.Config.MusicEnd);
			Globals.State.PauseSnapshot = FMODUnity.RuntimeManager.CreateInstance(Globals.Config.SnapshotPause);
			Globals.State.TimeScaleCurrent = Globals.State.TimeScaleDefault = 1f;
			Globals.State.Random = new Unity.Mathematics.Random();
			Globals.State.Random.InitState((uint)Random.Range(0, int.MaxValue));
			Globals.State.CurrentLevelIndex = 0;

			while (LocalizationSettings.InitializationOperation.IsDone == false)
				await UniTask.NextFrame();

			{
				if (Save.LoadGame(ref Globals.State.CurrentSave) == false)
					Debug.LogWarning("[Game] Couldn't load player save.");

				if (Save.LoadSettings(ref Globals.State.Settings) == false)
				{
					Globals.State.Settings = new GameSettings
					{
						VolumeGame = 1,
						VolumeSound = 1,
						VolumeMusic = 1,
						FullScreen = Screen.fullScreen,
						ResolutionWidth = Screen.currentResolution.width,
						ResolutionHeight = Screen.currentResolution.height,
						ResolutionRefreshRate = Screen.currentResolution.refreshRate,
						LocaleCode = LocalizationSettings.SelectedLocale.Identifier.Code,
						Screenshake = true,
					};
					Debug.LogWarning("[Game] Couldn't load player settings, saving default one.");
					Save.SaveSettings(Globals.State.Settings);
				}
			}

			ApplySettings(Globals.State.Settings);

			Globals.Controls.Global.Enable();

			Globals.UI.SetVersion($"{Globals.State.Version} - {Globals.State.Commit}");
			await Globals.UI.Init();
			await Globals.PauseUI.Init();
			await Globals.OptionsUI.Init();
			// await Globals.ControlsUI.Init();
			await Globals.GameplayUI.Init();

			Globals.UI.ShowDebug();

			if (IsDevBuild())
			{
				// if (Globals.Config.DebugLevels)
				// {
				// 	Globals.State.DebugLevels = Resources.LoadAll<Level>("Levels/Debug");
				// 	Globals.State.AllLevels = new Level[Globals.Config.Levels.Length + Globals.State.DebugLevels.Length];
				// 	Globals.Config.Levels.CopyTo(Globals.State.AllLevels, 0);
				// 	Globals.State.DebugLevels.CopyTo(Globals.State.AllLevels, Globals.Config.Levels.Length);
				// }

				if (Globals.Config.LockFPS > 0)
				{
					Debug.Log($"Locking FPS to {Globals.Config.LockFPS}");
					Application.targetFrameRate = Globals.Config.LockFPS;
					QualitySettings.vSyncCount = 1;
				}
				else
				{
					Application.targetFrameRate = 999;
					QualitySettings.vSyncCount = 0;
				}
			}

			FSM.Fire(GameFSM.Triggers.Done);
		}

		public void Tick() { }

		public void FixedTick() { }

		public UniTask Exit() { return default; }

		private void ApplySettings(GameSettings settings)
		{
			AudioHelpers.SetVolume(Globals.Config.GameBus, settings.VolumeGame);
			AudioHelpers.SetVolume(Globals.Config.MusicBus, settings.VolumeMusic);
			AudioHelpers.SetVolume(Globals.Config.SoundBus, settings.VolumeSound);
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(settings.LocaleCode);
			// Ignore resolution for WebGL since we always start in windowed mode with a fixed size.
			if (IsWebGL() == false)
				Screen.SetResolution(settings.ResolutionWidth, settings.ResolutionHeight, settings.FullScreen, settings.ResolutionRefreshRate);
		}
	}
}
