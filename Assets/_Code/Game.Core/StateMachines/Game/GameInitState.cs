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
			// Globals.State.Bag = new DiceBag();

			while (LocalizationSettings.InitializationOperation.IsDone == false)
				await UniTask.NextFrame();

			Globals.State.PlayerSettings = Save.LoadPlayerSettings();
			Globals.State.PlayerSaveData = Save.LoadPlayerSaveData();
			SetPlayerSettings(Globals.State.PlayerSettings);

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

		private void SetPlayerSettings(PlayerSettings playerSettings)
		{
			AudioHelpers.SetVolume(Globals.Config.GameBus, playerSettings.GameVolume);
			AudioHelpers.SetVolume(Globals.Config.MusicBus, playerSettings.MusicVolume);
			AudioHelpers.SetVolume(Globals.Config.SoundBus, playerSettings.SoundVolume);
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(playerSettings.LocaleCode);
			// Ignore resolution for WebGL since we always start in windowed mode with a fixed size.
			if (IsWebGL() == false)
				Screen.SetResolution(playerSettings.ResolutionWidth, playerSettings.ResolutionHeight, playerSettings.FullScreen, playerSettings.ResolutionRefreshRate);
		}
	}
}
