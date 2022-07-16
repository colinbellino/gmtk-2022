using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core.StateMachines.Game
{
	public class GameTitleState : IState
	{
		public GameFSM FSM;

		public async UniTask Enter()
		{
			if (GameManager.Game.Config.DebugSkipTitle)
			{
				FSM.Fire(GameFSM.Triggers.StartGame);
				return;
			}

			GameManager.Game.UI.StartButton.onClick.AddListener(StartGame);
			GameManager.Game.UI.OptionsButton.onClick.AddListener(ToggleOptions);
			GameManager.Game.UI.CreditsButton.onClick.AddListener(StartCredits);
			GameManager.Game.UI.QuitButton.onClick.AddListener(QuitGame);
			GameManager.Game.OptionsUI.BackClicked += OnOptionsBackClicked;

			_ = GameManager.Game.UI.HideCredits(0);

			GameManager.Game.State.TitleMusic.getPlaybackState(out var state);
			if (state != PLAYBACK_STATE.PLAYING)
				GameManager.Game.State.TitleMusic.start();

			_ = GameManager.Game.UI.FadeIn(Color.clear);

			// UnityEngine.Debug.Log("FIXME:");
			// if (GameManager.Game.State.PlayerSaveData.ClearedLevels.Count > 0)
			// 	Localization.SetImageKey(GameManager.Game.UI.StartButton.gameObject, "UI/Continue");
			// else
			// 	Localization.SetImageKey(GameManager.Game.UI.StartButton.gameObject, "UI/Start");

			await GameManager.Game.UI.ShowTitle();

			if (Utils.IsDevBuild())
			{
				GameManager.Game.UI.SetDebugText("");

				// TODO: Remove this
				// UnityEngine.Debug.Log("Skipping player save");
				// GameManager.Game.State.PlayerSaveData.ClearedLevels = new System.Collections.Generic.HashSet<int>();
				// StartGame();
			}
		}

		public void Tick()
		{
			if (Utils.IsDevBuild())
			{
				if (Keyboard.current.tabKey.wasPressedThisFrame)
				{
					if (GameManager.Game.ControlsUI.IsOpened)
						_ = GameManager.Game.ControlsUI.Hide();
					else
						_ = GameManager.Game.ControlsUI.Show();
				}
			}

			if (GameManager.Game.Controls.Global.Cancel.WasReleasedThisFrame())
			{
				if (GameManager.Game.OptionsUI.IsOpened == false)
					QuitGame();
			}

			if (Utils.IsDevBuild())
			{
				// if (Keyboard.current.kKey.wasReleasedThisFrame)
				// {
				// 	if (Keyboard.current.leftShiftKey.isPressed)
				// 	{
				// 		GameManager.Game.State.TakeScreenshots = true;
				// 		UnityEngine.Debug.Log("Taking screenshots!");
				// 	}

				// 	UnityEngine.Debug.Log("Starting in replay mode.");
				// 	GameManager.Game.State.IsReplaying = true;
				// 	LoadLevel(0);
				// }
			}
		}

		public void FixedTick() { }

		public UniTask Exit()
		{
			GameManager.Game.UI.StartButton.onClick.RemoveListener(StartGame);
			GameManager.Game.UI.OptionsButton.onClick.RemoveListener(ToggleOptions);
			GameManager.Game.UI.CreditsButton.onClick.RemoveListener(StartCredits);
			GameManager.Game.UI.QuitButton.onClick.RemoveListener(QuitGame);
			GameManager.Game.OptionsUI.BackClicked -= OnOptionsBackClicked;

			return default;
		}

		private async void StartGame()
		{
			GameManager.Game.State.TitleMusic.stop(STOP_MODE.ALLOWFADEOUT);

			await GameManager.Game.UI.FadeIn(Color.black);
			await GameManager.Game.UI.HideTitle(0);
			await GameManager.Game.OptionsUI.Hide(0);

			FSM.Fire(GameFSM.Triggers.StartGame);
		}

		private void ToggleOptions()
		{
			_ = GameManager.Game.OptionsUI.Show();
		}

		private async void StartCredits()
		{
			await GameManager.Game.UI.HideTitle();
			await GameManager.Game.UI.FadeIn(Color.black);

			FSM.Fire(GameFSM.Triggers.CreditsRequested);
		}

		private void QuitGame()
		{
			FSM.Fire(GameFSM.Triggers.Quit);
		}

		private void OnOptionsBackClicked()
		{
			GameManager.Game.UI.SelectTitleOptionsGameObject();
		}
	}
}
