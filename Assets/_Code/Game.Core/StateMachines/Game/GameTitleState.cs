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
			if (Globals.Config.DebugSkipTitle)
			{
				FSM.Fire(GameFSM.Triggers.StartGame);
				return;
			}

			Globals.UI.StartButton.onClick.AddListener(StartGame);
			Globals.UI.OptionsButton.onClick.AddListener(ToggleOptions);
			Globals.UI.CreditsButton.onClick.AddListener(StartCredits);
			Globals.UI.QuitButton.onClick.AddListener(QuitGame);
			Globals.OptionsUI.BackClicked += OnOptionsBackClicked;

			_ = Globals.UI.HideCredits(0);

			Globals.State.TitleMusic.getPlaybackState(out var state);
			if (state != PLAYBACK_STATE.PLAYING)
				Globals.State.TitleMusic.start();

			_ = Globals.UI.FadeIn(Color.clear);

			// UnityEngine.Debug.Log("FIXME:");
			// if (Globals.State.PlayerSaveData.ClearedLevels.Count > 0)
			// 	Localization.SetImageKey(Globals.UI.StartButton.gameObject, "UI/Continue");
			// else
			// 	Localization.SetImageKey(Globals.UI.StartButton.gameObject, "UI/Start");

			await Globals.UI.ShowTitle();

			if (Utils.IsDevBuild())
			{
				Globals.UI.SetDebugText("");

				// TODO: Remove this
				// UnityEngine.Debug.Log("Skipping player save");
				// Globals.State.PlayerSaveData.ClearedLevels = new System.Collections.Generic.HashSet<int>();
				// StartGame();
			}
		}

		public void Tick()
		{
			if (Utils.IsDevBuild())
			{
				if (Keyboard.current.tabKey.wasPressedThisFrame)
				{
					if (Globals.ControlsUI.IsOpened)
						_ = Globals.ControlsUI.Hide();
					else
						_ = Globals.ControlsUI.Show();
				}
			}

			if (Globals.Controls.Global.Cancel.WasReleasedThisFrame())
			{
				if (Globals.OptionsUI.IsOpened == false)
					QuitGame();
			}

			if (Utils.IsDevBuild())
			{
				// if (Keyboard.current.kKey.wasReleasedThisFrame)
				// {
				// 	if (Keyboard.current.leftShiftKey.isPressed)
				// 	{
				// 		Globals.State.TakeScreenshots = true;
				// 		UnityEngine.Debug.Log("Taking screenshots!");
				// 	}

				// 	UnityEngine.Debug.Log("Starting in replay mode.");
				// 	Globals.State.IsReplaying = true;
				// 	LoadLevel(0);
				// }
			}
		}

		public void FixedTick() { }

		public UniTask Exit()
		{
			Globals.UI.StartButton.onClick.RemoveListener(StartGame);
			Globals.UI.OptionsButton.onClick.RemoveListener(ToggleOptions);
			Globals.UI.CreditsButton.onClick.RemoveListener(StartCredits);
			Globals.UI.QuitButton.onClick.RemoveListener(QuitGame);
			Globals.OptionsUI.BackClicked -= OnOptionsBackClicked;

			return default;
		}

		private async void StartGame()
		{
			Globals.State.TitleMusic.stop(STOP_MODE.ALLOWFADEOUT);

			await Globals.UI.FadeIn(Color.black);
			await Globals.UI.HideTitle(0);
			await Globals.OptionsUI.Hide(0);

			FSM.Fire(GameFSM.Triggers.StartGame);
		}

		private void ToggleOptions()
		{
			_ = Globals.OptionsUI.Show();
		}

		private async void StartCredits()
		{
			await Globals.UI.HideTitle();
			await Globals.UI.FadeIn(Color.black);

			FSM.Fire(GameFSM.Triggers.CreditsRequested);
		}

		private void QuitGame()
		{
			FSM.Fire(GameFSM.Triggers.Quit);
		}

		private void OnOptionsBackClicked()
		{
			Globals.UI.SelectTitleOptionsGameObject();
		}
	}
}
