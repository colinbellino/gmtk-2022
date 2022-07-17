using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using Unity.Mathematics;

namespace Game.Core.StateMachines.Game
{
	public class GameGameplayState : IState
	{
		public GameFSM FSM;

		public async UniTask Enter()
		{
			Globals.Controls.Gameplay.Enable();
			Globals.Controls.Gameplay.Move.performed += OnMovePerformed;

			Globals.State.Requests = new List<DiceRequest> { };
			Globals.State.ActiveRequests = new List<int> { };
			Globals.State.CompletedRequests = new List<int> { };
			Globals.State.FailedRequests = new List<int> { };
			Globals.State.Score = 0;

			Globals.State.MusicMain.setPitch(Globals.State.TimeScaleCurrent);
			Globals.State.MusicMain.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
				Globals.State.MusicMain.start();

			_ = Globals.GameplayUI.Show();
			Globals.GameplayUI.Tick();
			await Globals.UI.FadeIn(Color.clear);

			if (Globals.State.CurrentLevelIndex >= Globals.Config.Levels.Count())
				Globals.State.CurrentLevelIndex = 0;
			var level = Globals.Config.Levels[Globals.State.CurrentLevelIndex];

			if (Globals.State.CurrentSave.FinishedTutorial)
				Globals.State.Requests = new List<DiceRequest>();
			else
				Globals.State.Requests = new List<DiceRequest>(level.Requests);

			{
				var rolls = Resources.LoadAll<DiceRoll>("Dice Rolls");
				var randomRequestsCount = 10000;
				var randomRequests = new List<DiceRequest>(randomRequestsCount);
				for (int i = 0; i < randomRequestsCount; i++)
				{
					var req = new DiceRequest();
					req.Roll = rolls[Globals.State.Random.NextInt(0, rolls.Length)];
					if (Globals.State.Random.NextInt(0, 11) > 9)
						req.FromDM = true;
					// if (Globals.State.Random.NextInt(0, 11) > 5)
					req.Offset = Globals.State.Random.NextFloat(-3, 0.3f);
					// else
					// 	req.Offset = 0.3f;
					randomRequests.Add(req);

					// UnityEngine.Debug.Log(Utils.DiceRequestToString(req) + " | offset: " + req.Offset + " | DM: " + req.FromDM);
				}

				Globals.State.Requests.AddRange(randomRequests);
			}

			var t = Time.time;
			foreach (var req in Globals.State.Requests)
			{
				req.Timestamp = t + req.Offset;
				t = req.Timestamp + Utils.GetDuration(req);
			}
			Globals.State.Timer = Time.time + level.Timer;
			Globals.State.QueuedRequests = Globals.State.Requests.Select((r, i) => i).ToList();

			Save.SaveGame(Globals.State.CurrentSave);

			Globals.PauseUI.BackClicked += ResumeGame;

			Globals.State.Running = true;
		}

		public async void Tick()
		{
			if (Globals.State.Running)
			{
				if (Globals.State.Paused == false)
				{
					if (Utils.IsDevBuild() && Globals.Config.DebugGottaGoFast)
						Globals.State.TimeScaleCurrent = 5f;
					else if (Globals.State.Settings.AssistMode)
						Globals.State.TimeScaleCurrent = 0.5f;
					else
						Globals.State.TimeScaleCurrent = Globals.State.TimeScaleDefault;
				}

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
						_ = Globals.OptionsUI.Hide();
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

				Globals.GameplayUI.Tick();

				if (Globals.State.CurrentSave.FinishedTutorial == false && Globals.State.CompletedRequests.Count >= 5)
				{
					Globals.State.CurrentSave.FinishedTutorial = true;
					Save.SaveGame(Globals.State.CurrentSave);
				}

				{
					var toAdd = new List<int>();
					for (int i = Globals.State.QueuedRequests.Count - 1; i >= 0; i--)
					{
						var reqIndex = Globals.State.QueuedRequests[i];
						var req = Globals.State.Requests[reqIndex];
						if (Time.time >= req.Timestamp)
						{
							Globals.State.QueuedRequests.Remove(reqIndex);
							Globals.State.ActiveRequests.Add(reqIndex);
							toAdd.Add(reqIndex);
						}
					}

					if (toAdd.Count > 0)
						await Globals.GameplayUI.AddRequests(toAdd);
				}

				{
					var toRemove = new List<int>();
					for (int i = Globals.State.ActiveRequests.Count - 1; i >= 0; i--)
					{
						var reqIndex = Globals.State.ActiveRequests[i];
						var req = Globals.State.Requests[reqIndex];
						if (Time.time >= req.Timestamp + Utils.GetDuration(req))
						{
							Globals.State.ActiveRequests.Remove(reqIndex);
							Globals.State.FailedRequests.Add(reqIndex);

							var score = Globals.Config.ScoreFail * Globals.Config.ScoreMultiplier;
							Globals.State.Score = math.max(0, Globals.State.Score - score);

							AudioHelpers.PlayOneShot(Globals.Config.SoundRequestFail);

							toRemove.Add(reqIndex);
						}
					}

					if (toRemove.Count > 0)
						await Globals.GameplayUI.RemoveRequests(toRemove);
				}

				if (Globals.State.FailedRequests.Count > Globals.Config.MaxFails)
				{
					if (Utils.IsDevBuild() && Globals.Config.DebugGottaGoFast == true)
					{

					}
					else
					{
						Victory();
					}
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
			await Globals.UI.FadeIn(Globals.Config.ColorBackgroundDark);
			await Globals.UI.HideLevelName(0);
			await Globals.GameplayUI.Hide(0);
			await Globals.PauseUI.Hide(0);
			await Globals.OptionsUI.Hide(0);

			await Globals.GameplayUI.RemoveRequests(Globals.State.ActiveRequests);
		}

		private void OnMovePerformed(InputAction.CallbackContext context)
		{
			if (Globals.State.Running == false || Globals.State.Paused)
				return;
		}

		private void Victory()
		{
			UnityEngine.Debug.Log("End of the game reached.");
			Globals.State.MusicMain.stop(STOP_MODE.ALLOWFADEOUT);
			FSM.Fire(GameFSM.Triggers.Won);
		}

		private async void NextLevel()
		{
			// if (Globals.State.CurrentSave.ClearedLevels == null)
			// 	Globals.State.CurrentSave.ClearedLevels = new HashSet<int>();
			// Globals.State.CurrentSave.ClearedLevels.Add(Globals.State.CurrentLevelIndex);
			Save.SaveGame(Globals.State.CurrentSave);
			Globals.State.CurrentLevelIndex += 1;

			Globals.State.Running = false;

			if (Globals.State.CurrentLevelIndex >= Globals.Config.Levels.Length)
			{
				Victory();
				return;
			}

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
