using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Core.StateMachines.Game;
using Game.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace Game.Core
{
	public class GameManager : MonoBehaviour
	{
		public static GameSingleton Game { get; private set; }

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
				Game = new GameSingleton();
				Game.Config = _config;
				Game.Controls = new GameControls();
				Game.CameraRig = _cameraRig;
				Game.UI = _gameUI;
				Game.PauseUI = _pauseUI;
				Game.OptionsUI = _optionsUI;
				Game.ControlsUI = _controlsUI;
				Game.GameplayUI = _gameplayUI;
				Game.State = new GameState();
				Game.GameFSM = new GameFSM(_config.DebugStateMachine, Game);

				await Game.GameFSM.Start();
			}
			catch (System.Exception exc)
			{
				UnityEngine.Debug.LogError(exc);
			}
		}

		private void Update()
		{
			Time.timeScale = Game.State.TimeScaleCurrent;
			Game?.GameFSM.Tick();
		}

		private void LateUpdate()
		{
			// Stick the UI to the camera
			Game.UI.transform.position = Game.CameraRig.transform.position + Game.CameraRig.Camera.transform.localPosition;
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
				AudioHelpers.SetVolume(Game.Config.GameBus, 0);
			else
				AudioHelpers.SetVolume(Game.Config.GameBus, Game.State.PlayerSettings.GameVolume);
		}
	}
}
