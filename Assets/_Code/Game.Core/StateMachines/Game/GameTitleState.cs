using System;
using System.Linq;
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
			_ = Globals.UI.FadeIn(Color.clear);

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

			await Globals.UI.ShowTitle();
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
			// Globals.State.MusicTitle.stop(STOP_MODE.ALLOWFADEOUT);

			await Globals.UI.FadeIn(Globals.Config.ColorBackgroundDark);
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
			await Globals.UI.FadeIn(Globals.Config.ColorBackgroundDark);

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
