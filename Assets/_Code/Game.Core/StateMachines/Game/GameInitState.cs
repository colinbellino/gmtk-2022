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
			GameManager.Game.UI.SetDebugText("");

			_ = GameManager.Game.UI.FadeIn(Color.black, 0);

			FMODUnity.RuntimeManager.LoadBank("SFX", loadSamples: true);

			GameManager.Game.State.Version = Application.version;
			GameManager.Game.State.Commit = await ReadStreamingAsset("/commit.txt");
			GameManager.Game.State.TitleMusic = FMODUnity.RuntimeManager.CreateInstance(GameManager.Game.Config.MusicTitle);
			GameManager.Game.State.LevelMusic = FMODUnity.RuntimeManager.CreateInstance(GameManager.Game.Config.MusicMain);
			GameManager.Game.State.EndMusic = FMODUnity.RuntimeManager.CreateInstance(GameManager.Game.Config.MusicEnd);
			GameManager.Game.State.PauseSnapshot = FMODUnity.RuntimeManager.CreateInstance(GameManager.Game.Config.SnapshotPause);
			GameManager.Game.State.TimeScaleCurrent = GameManager.Game.State.TimeScaleDefault = 1f;
			GameManager.Game.State.Random = new Unity.Mathematics.Random();
			GameManager.Game.State.Random.InitState((uint)Random.Range(0, int.MaxValue));
			// GameManager.Game.State.Bag = new DiceBag();

			while (LocalizationSettings.InitializationOperation.IsDone == false)
				await UniTask.NextFrame();

			GameManager.Game.State.PlayerSettings = Save.LoadPlayerSettings();
			GameManager.Game.State.PlayerSaveData = Save.LoadPlayerSaveData();
			SetPlayerSettings(GameManager.Game.State.PlayerSettings);

			GameManager.Game.Controls.Global.Enable();

			GameManager.Game.UI.SetVersion($"{GameManager.Game.State.Version} - {GameManager.Game.State.Commit}");
			await GameManager.Game.UI.Init();
			await GameManager.Game.PauseUI.Init();
			await GameManager.Game.OptionsUI.Init();
			// await GameManager.Game.ControlsUI.Init();
			await GameManager.Game.GameplayUI.Init();

			GameManager.Game.UI.ShowDebug();

			if (IsDevBuild())
			{
				// if (GameManager.Game.Config.DebugLevels)
				// {
				// 	GameManager.Game.State.DebugLevels = Resources.LoadAll<Level>("Levels/Debug");
				// 	GameManager.Game.State.AllLevels = new Level[GameManager.Game.Config.Levels.Length + GameManager.Game.State.DebugLevels.Length];
				// 	GameManager.Game.Config.Levels.CopyTo(GameManager.Game.State.AllLevels, 0);
				// 	GameManager.Game.State.DebugLevels.CopyTo(GameManager.Game.State.AllLevels, GameManager.Game.Config.Levels.Length);
				// }

				if (GameManager.Game.Config.LockFPS > 0)
				{
					Debug.Log($"Locking FPS to {GameManager.Game.Config.LockFPS}");
					Application.targetFrameRate = GameManager.Game.Config.LockFPS;
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
			AudioHelpers.SetVolume(GameManager.Game.Config.GameBus, playerSettings.GameVolume);
			AudioHelpers.SetVolume(GameManager.Game.Config.MusicBus, playerSettings.MusicVolume);
			AudioHelpers.SetVolume(GameManager.Game.Config.SoundBus, playerSettings.SoundVolume);
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(playerSettings.LocaleCode);
			// Ignore resolution for WebGL since we always start in windowed mode with a fixed size.
			if (IsWebGL() == false)
				Screen.SetResolution(playerSettings.ResolutionWidth, playerSettings.ResolutionHeight, playerSettings.FullScreen, playerSettings.ResolutionRefreshRate);
		}
	}
}
