using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.Mathematics;

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

			var levelIndex = Globals.State.CurrentLevelIndex;
			if (Globals.State.CurrentSave.ClearedLevels != null && Globals.State.CurrentSave.ClearedLevels.Count > 0)
				levelIndex = math.min(Globals.State.CurrentSave.ClearedLevels.LastOrDefault() + 1, Globals.Config.Levels.Length - 1);

			// UnityEngine.Debug.Log("levelIndex: " + levelIndex);

			var level = Globals.Config.Levels[levelIndex];
			Globals.State.Score = 0;
			Globals.State.Requests = new List<DiceRequest>(level.Requests);
			var t = Time.time;
			foreach (var req in Globals.State.Requests)
			{
				req.Timestamp = t + req.Offset;
				t = req.Timestamp + Utils.GetDuration(req);
			}
			Globals.State.Timer = Time.time + level.Timer;
			Globals.State.QueuedRequests = Globals.State.Requests.Select((r, i) => i).ToList();
			Globals.State.ActiveRequests = new List<int> { };
			Globals.State.CompletedRequests = new List<int> { };
			Globals.State.FailedRequests = new List<int> { };

			Save.SaveGame(Globals.State.CurrentSave);

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
			// Globals.State.StartedAt = Time.time;
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
						Save.SaveSettings(Globals.State.Settings);
					}
				}

				if (Globals.Controls.Gameplay.Confirm.WasPerformedThisFrame())
				{
					GameObject.FindObjectOfType<Bag>().SubmitBag();
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

				for (int i = Globals.State.ActiveRequests.Count - 1; i >= 0; i--)
				{
					var reqIndex = Globals.State.ActiveRequests[i];
					var req = Globals.State.Requests[reqIndex];
					if (Time.time >= req.Timestamp + Utils.GetDuration(req))
					{
						Globals.State.FailedRequests.Add(reqIndex);
						Globals.State.ActiveRequests.Remove(reqIndex);
					}
				}

				Globals.GameplayUI.Tick();

				if (Time.time >= Globals.State.Timer)
				{
					NextLevel();
					return;
				}
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
			if (Globals.State.CurrentSave.ClearedLevels == null)
				Globals.State.CurrentSave.ClearedLevels = new HashSet<int>();
			Globals.State.CurrentSave.ClearedLevels.Add(Globals.State.CurrentLevelIndex);
			Save.SaveGame(Globals.State.CurrentSave);
			Globals.State.CurrentLevelIndex += 1;

			AudioHelpers.PlayOneShot(Globals.Config.StageClear);

			await UniTask.Delay(1000);

			if (Globals.State.CurrentLevelIndex >= Globals.Config.Levels.Length)
			{
				Victory();
				return;
			}

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
