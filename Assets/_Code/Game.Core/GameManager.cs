using Cysharp.Threading.Tasks;
using Game.Core.StateMachines.Game;
using Game.Inputs;
using UnityEngine;

namespace Game.Core
{
	public static class Globals
	{
		public static GameConfig Config;
		public static GameUI UI;
		public static PauseUI PauseUI;
		public static OptionsUI OptionsUI;
		public static ControlsUI ControlsUI;
		public static GameplayUI GameplayUI;
		public static CameraRig CameraRig;
		public static GameControls Controls;
		public static GameState State;
		public static GameFSM GameFSM;
	}

	public class GameManager : MonoBehaviour
	{
		[SerializeField] private GameConfig _config;
		[SerializeField] private CameraRig _cameraRig;
		[SerializeField] private GameUI _gameUI;
		[SerializeField] private PauseUI _pauseUI;
		[SerializeField] private OptionsUI _optionsUI;
		[SerializeField] private ControlsUI _controlsUI;
		[SerializeField] private GameplayUI _gameplayUI;

		private async UniTask Start()
		{
			try
			{
				Globals.Config = _config;
				Globals.Controls = new GameControls();
				Globals.CameraRig = _cameraRig;
				Globals.UI = _gameUI;
				Globals.PauseUI = _pauseUI;
				Globals.OptionsUI = _optionsUI;
				Globals.ControlsUI = _controlsUI;
				Globals.GameplayUI = _gameplayUI;
				Globals.State = new GameState();
				Globals.GameFSM = new GameFSM(_config.DebugStateMachine);

				await Globals.GameFSM.Start();
			}
			catch (System.Exception exc)
			{
				UnityEngine.Debug.LogError(exc);
			}
		}

		private void Update()
		{
			Time.timeScale = Globals.State.TimeScaleCurrent;
			Globals.GameFSM.Tick();
		}

		private void LateUpdate()
		{
			// Stick the UI to the camera
			Globals.UI.transform.position = Globals.CameraRig.transform.position + Globals.CameraRig.Camera.transform.localPosition;
		}

		private void OnDisable()
		{
			DG.Tweening.DOTween.KillAll();
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (Utils.IsWebGL())
				Silence(!hasFocus);
		}

		private void OnApplicationPause(bool isPaused)
		{
			if (Utils.IsWebGL())
				Silence(isPaused);
		}

		private void Silence(bool silence)
		{
			if (silence)
				AudioHelpers.SetVolume(Globals.Config.GameBus, 0);
			else
				AudioHelpers.SetVolume(Globals.Config.GameBus, Globals.State.Settings.VolumeGame);
		}
	}
}
