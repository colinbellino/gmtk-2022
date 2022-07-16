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

			GameManager.Game.Controls.Gameplay.Enable();
			GameManager.Game.Controls.Gameplay.Move.performed += OnMovePerformed;

			GameManager.Game.State.LevelMusic.setPitch(GameManager.Game.State.TimeScaleCurrent);
			GameManager.Game.State.LevelMusic.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
				GameManager.Game.State.LevelMusic.start();

			GameManager.Game.State.Requests = new List<DiceRequest> {
				new DiceRequest { Id = 0, Quantity = 1, Die = DieTypes.D6,   Bonus = +0,  Timestamp = Time.time + 0f },
				new DiceRequest { Id = 1, Quantity = 2, Die = DieTypes.D4,   Bonus = +0,  Timestamp = Time.time + 1.6f },
				new DiceRequest { Id = 2, Quantity = 1, Die = DieTypes.D10,  Bonus = +2,  Timestamp = Time.time + 2f },
				new DiceRequest { Id = 3, Quantity = 3, Die = DieTypes.D6,   Bonus = +0,  Timestamp = Time.time + 3f },
				new DiceRequest { Id = 0, Quantity = 1, Die = DieTypes.D100, Bonus = +0,  Timestamp = Time.time + 3 + 0f },
				new DiceRequest { Id = 1, Quantity = 2, Die = DieTypes.D12,  Bonus = +0,  Timestamp = Time.time + 3 + 1.6f },
				new DiceRequest { Id = 2, Quantity = 1, Die = DieTypes.D10,  Bonus = +2,  Timestamp = Time.time + 3 + 2f },
				new DiceRequest { Id = 3, Quantity = 5, Die = DieTypes.D4,   Bonus = +0,  Timestamp = Time.time + 3 + 3f },
			};

			GameManager.Game.State.QueuedRequests = GameManager.Game.State.Requests.Select((r, i) => i).ToList();
			GameManager.Game.State.ActiveRequests = new List<int> { };
			GameManager.Game.State.CompletedRequests = new List<int> { };

			if (Utils.IsDevBuild())
			{
				GameManager.Game.UI.SetDebugText("");
				// 	GameManager.Game.UI.AddDebugLine("F1:  Load next level");
				// 	GameManager.Game.UI.AddDebugLine("F2:  Kill all enemies");
				// 	GameManager.Game.UI.AddDebugLine("F4:  Damage player");
				// 	GameManager.Game.UI.AddDebugLine("F5:  Heal player");
				// 	GameManager.Game.UI.AddDebugLine("R:   Restart level");
			}

			GameManager.Game.State.Running = true;
		}

		public void Tick()
		{
			if (GameManager.Game.State.Running)
			{
				if (GameManager.Game.Controls.Global.Pause.WasPerformedThisFrame())
				{
					if (GameManager.Game.State.Paused)
					{
						ResumeGame();
					}
					else
					{
						GameManager.Game.State.TimeScaleCurrent = 0f;
						GameManager.Game.State.Paused = true;
						_ = GameManager.Game.PauseUI.Show();
						GameManager.Game.State.PauseSnapshot.start();
					}
				}

				if (GameManager.Game.Controls.Global.Cancel.WasPerformedThisFrame())
				{
					if (GameManager.Game.OptionsUI.IsOpened)
					{
						GameManager.Game.OptionsUI.Hide();
						GameManager.Game.PauseUI.SelectOptionsGameObject();
						Save.SavePlayerSettings(GameManager.Game.State.PlayerSettings);
					}
				}

				if (GameManager.Game.Controls.Gameplay.Confirm.WasPerformedThisFrame())
				{
					GameObject.FindObjectOfType<Bag>().SubmitBag();
					return;
				}

				if (GameManager.Game.Controls.Gameplay.Reset.WasPerformedThisFrame())
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

				var s = GameManager.Game.State;
				for (int i = GameManager.Game.State.QueuedRequests.Count - 1; i >= 0; i--)
				{
					var reqIndex = GameManager.Game.State.QueuedRequests[i];
					var req = GameManager.Game.State.Requests[reqIndex];
					if (Time.time >= req.Timestamp)
					{
						GameManager.Game.State.ActiveRequests.Add(reqIndex);
						GameManager.Game.State.QueuedRequests.Remove(reqIndex);
					}
				}

				GameManager.Game.GameplayUI.Tick();
			}
		}

		public void FixedTick() { }

		public async UniTask Exit()
		{
			if (Utils.IsDevBuild())
				GameManager.Game.UI.SetDebugText("");

			GameManager.Game.Controls.Gameplay.Disable();
			GameManager.Game.Controls.Gameplay.Move.performed -= OnMovePerformed;

			GameManager.Game.State.TimeScaleCurrent = GameManager.Game.State.TimeScaleDefault;
			GameManager.Game.State.Running = false;
			GameManager.Game.State.Paused = false;
			GameManager.Game.State.PauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT);

			GameManager.Game.PauseUI.BackClicked -= ResumeGame;
			await GameManager.Game.UI.FadeIn(Color.black);
			await GameManager.Game.UI.HideLevelName(0);
			await GameManager.Game.GameplayUI.Hide(0);
			await GameManager.Game.PauseUI.Hide(0);
			await GameManager.Game.OptionsUI.Hide(0);

			await SceneManager.UnloadSceneAsync("Gameplay");
		}

		private void OnMovePerformed(InputAction.CallbackContext context)
		{
			if (GameManager.Game.State.Running == false || GameManager.Game.State.Paused)
				return;
		}

		private void Victory()
		{
			UnityEngine.Debug.Log("End of the game reached.");
			GameManager.Game.State.LevelMusic.stop(STOP_MODE.ALLOWFADEOUT);
			FSM.Fire(GameFSM.Triggers.Won);
		}

		private async void NextLevel()
		{
			GameManager.Game.State.Running = false;
			Save.SavePlayerSaveData(GameManager.Game.State.PlayerSaveData);

			AudioHelpers.PlayOneShot(GameManager.Game.Config.StageClear);

			await UniTask.Delay(1000);

			GameManager.Game.State.Running = false;
			FSM.Fire(GameFSM.Triggers.NextLevel);
		}

		private void ResumeGame()
		{
			GameManager.Game.State.TimeScaleCurrent = GameManager.Game.State.TimeScaleDefault;
			GameManager.Game.State.Paused = false;
			GameManager.Game.PauseUI.Hide();
			GameManager.Game.State.PauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT);
		}
	}
}
