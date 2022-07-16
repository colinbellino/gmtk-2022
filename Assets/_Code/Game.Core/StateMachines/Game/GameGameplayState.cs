using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.Core.StateMachines.Game
{
	public class GameGameplayState : IState
	{
		public GameFSM FSM;

		public async UniTask Enter()
		{
			await SceneManager.LoadSceneAsync("Gameplay", LoadSceneMode.Additive);

			Globals.Controls.Gameplay.Enable();
			Globals.Controls.Gameplay.Move.performed += OnMovePerformed;

			Globals.State.LevelMusic.setPitch(Globals.State.TimeScaleCurrent);
			Globals.State.LevelMusic.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
				Globals.State.LevelMusic.start();

			Globals.State.Requests = new List<DiceRequest> {
				new DiceRequest { Id = 0, Quantity = 1, Die = DieTypes.D6,   Bonus = +0,  Timestamp = Time.time + 0f },
				new DiceRequest { Id = 1, Quantity = 2, Die = DieTypes.D4,   Bonus = +0,  Timestamp = Time.time + 1.6f },
				new DiceRequest { Id = 2, Quantity = 1, Die = DieTypes.D10,  Bonus = +2,  Timestamp = Time.time + 2f },
				new DiceRequest { Id = 3, Quantity = 3, Die = DieTypes.D6,   Bonus = +0,  Timestamp = Time.time + 3f },
				new DiceRequest { Id = 0, Quantity = 1, Die = DieTypes.D100, Bonus = +0,  Timestamp = Time.time + 3 + 0f },
				new DiceRequest { Id = 1, Quantity = 2, Die = DieTypes.D12,  Bonus = +0,  Timestamp = Time.time + 3 + 1.6f },
				new DiceRequest { Id = 2, Quantity = 2, Die = DieTypes.D10,  Bonus = +2,  Timestamp = Time.time + 3 + 2f },
				new DiceRequest { Id = 3, Quantity = 5, Die = DieTypes.D4,   Bonus = +0,  Timestamp = Time.time + 3 + 3f },
			};

			Globals.State.Score = 0;
			Globals.State.QueuedRequests = Globals.State.Requests.Select((r, i) => i).ToList();
			Globals.State.ActiveRequests = new List<int> { };
			Globals.State.CompletedRequests = new List<int> { };

			if (Utils.IsDevBuild())
			{
				Globals.UI.SetDebugText("");
				// 	Globals.UI.AddDebugLine("F1:  Load next level");
				// 	Globals.UI.AddDebugLine("F2:  Kill all enemies");
				// 	Globals.UI.AddDebugLine("F4:  Damage player");
				// 	Globals.UI.AddDebugLine("F5:  Heal player");
				// 	Globals.UI.AddDebugLine("R:   Restart level");
			}

			Globals.State.Running = true;
		}

		public void Tick()
		{
			if (Globals.State.Running)
			{
				if (Globals.Controls.Global.Pause.WasPerformedThisFrame())
				{
					if (Globals.State.Paused)
					{
						ResumeGame();
					}
					else
					{
						Globals.State.TimeScaleCurrent = 0f;
						Globals.State.Paused = true;
						_ = Globals.PauseUI.Show();
						Globals.State.PauseSnapshot.start();
					}
				}

				if (Globals.Controls.Global.Cancel.WasPerformedThisFrame())
				{
					if (Globals.OptionsUI.IsOpened)
					{
						Globals.OptionsUI.Hide();
						Globals.PauseUI.SelectOptionsGameObject();
						Save.SavePlayerSettings(Globals.State.PlayerSettings);
					}
				}

				if (Globals.Controls.Gameplay.Confirm.WasPerformedThisFrame())
				{
					GameObject.FindObjectOfType<Bag>().SubmitBag();
					return;
				}

				if (Globals.Controls.Gameplay.Reset.WasPerformedThisFrame())
				{
					FSM.Fire(GameFSM.Triggers.Retry);
					return;
				}

				if (Utils.IsDevBuild())
				{
					if (Keyboard.current.f1Key.wasReleasedThisFrame)
					{
						NextLevel();
						return;
					}
				}

				var s = Globals.State;
				for (int i = Globals.State.QueuedRequests.Count - 1; i >= 0; i--)
				{
					var reqIndex = Globals.State.QueuedRequests[i];
					var req = Globals.State.Requests[reqIndex];
					if (Time.time >= req.Timestamp)
					{
						Globals.State.ActiveRequests.Add(reqIndex);
						Globals.State.QueuedRequests.Remove(reqIndex);
					}
				}

				Globals.GameplayUI.Tick();
			}
		}

		public void FixedTick() { }

		public async UniTask Exit()
		{
			if (Utils.IsDevBuild())
				Globals.UI.SetDebugText("");

			Globals.Controls.Gameplay.Disable();
			Globals.Controls.Gameplay.Move.performed -= OnMovePerformed;

			Globals.State.TimeScaleCurrent = Globals.State.TimeScaleDefault;
			Globals.State.Running = false;
			Globals.State.Paused = false;
			Globals.State.PauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT);

			Globals.PauseUI.BackClicked -= ResumeGame;
			await Globals.UI.FadeIn(Color.black);
			await Globals.UI.HideLevelName(0);
			await Globals.GameplayUI.Hide(0);
			await Globals.PauseUI.Hide(0);
			await Globals.OptionsUI.Hide(0);

			await SceneManager.UnloadSceneAsync("Gameplay");
		}

		private void OnMovePerformed(InputAction.CallbackContext context)
		{
			if (Globals.State.Running == false || Globals.State.Paused)
				return;
		}

		private void Victory()
		{
			UnityEngine.Debug.Log("End of the game reached.");
			Globals.State.LevelMusic.stop(STOP_MODE.ALLOWFADEOUT);
			FSM.Fire(GameFSM.Triggers.Won);
		}

		private async void NextLevel()
		{
			Globals.State.Running = false;
			Save.SavePlayerSaveData(Globals.State.PlayerSaveData);

			AudioHelpers.PlayOneShot(Globals.Config.StageClear);

			await UniTask.Delay(1000);

			Globals.State.Running = false;
			FSM.Fire(GameFSM.Triggers.NextLevel);
		}

		private void ResumeGame()
		{
			Globals.State.TimeScaleCurrent = Globals.State.TimeScaleDefault;
			Globals.State.Paused = false;
			Globals.PauseUI.Hide();
			Globals.State.PauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT);
		}
	}
}
