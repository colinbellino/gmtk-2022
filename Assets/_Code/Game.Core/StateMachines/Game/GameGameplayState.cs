using Cysharp.Threading.Tasks;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core.StateMachines.Game
{
	public class GameGameplayState : IState
	{
		public GameFSM FSM;

		public async UniTask Enter()
		{
			GameManager.Game.State.Running = true;

			var player = GameObject.Instantiate(GameManager.Game.Config.Player);
			player.transform.position = LevelHelpers.GetRoomCenter(GameManager.Game.State.Level.CurrentRoom);
			GameManager.Game.State.Player = player;
			GameManager.Game.State.Level.CurrentRoom.Explored = true;

			GameManager.Game.CameraRig.transform.position = LevelHelpers.GetRoomCenter(GameManager.Game.State.Level.CurrentRoom);

			GameManager.Game.GameplayUI.SetHealth(GameManager.Game.State.Player.Health.currentHP, GameManager.Game.State.Player.Health.getMaxHP());
			GameManager.Game.GameplayUI.SetMiniMap(GameManager.Game.State.Level);
			_ = GameManager.Game.GameplayUI.Show();
			GameManager.Game.State.Player.Health.CurrentHPChanged += GameManager.Game.GameplayUI.SetHealth;

			GameManager.Game.PauseUI.BackClicked += ResumeGame;
			_ = GameManager.Game.UI.FadeIn(Color.clear);

			LevelHelpers.ActivateRoom(GameManager.Game.State.Level.CurrentRoom);

			GameManager.Game.Controls.Gameplay.Enable();
			GameManager.Game.Controls.Gameplay.Move.performed += OnMovePerformed;

			GameManager.Game.State.LevelMusic.setPitch(GameManager.Game.State.TimeScaleCurrent);
			GameManager.Game.State.LevelMusic.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
				GameManager.Game.State.LevelMusic.start();

			if (Utils.IsDevBuild())
			{
				GameManager.Game.UI.SetDebugText("");
				GameManager.Game.UI.AddDebugLine("F1:  Load next level");
				GameManager.Game.UI.AddDebugLine("F2:  Kill all enemies");
				GameManager.Game.UI.AddDebugLine("F4:  Damage player");
				GameManager.Game.UI.AddDebugLine("F5:  Heal player");
				GameManager.Game.UI.AddDebugLine("R:   Restart level");
			}
		}

		public void Tick()
		{
			var player = GameManager.Game.State.Player;
			var level = GameManager.Game.State.Level;

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

					if (Keyboard.current.f2Key.wasReleasedThisFrame)
					{
						// Kill all enemies! Mohahaha
						UnityEngine.Debug.Log("Killing all enemies.");
						foreach (var room in level.Rooms)
						{
							foreach (var entity in room.Entities)
							{
								var health = entity.GetComponent<Health>();
								if (health != null)
									health.DealDamage(health.getMaxHP(), new Vector3(0, 0, 0));
							}
						}
						return;
					}

					if (Keyboard.current.f4Key.wasReleasedThisFrame)
					{
						UnityEngine.Debug.Log("Stop hitting yourself.");
						player.Health.DealDamage(200, Vector3.zero);
					}
					if (Keyboard.current.f5Key.wasReleasedThisFrame)
					{
						UnityEngine.Debug.Log("Thanks for the fish.");
						player.Health.Heal(player.Health.getMaxHP());
					}
				}

				var allEnemiesAreDead = true;
				foreach (var room in level.Rooms)
				{
					foreach (var entity in room.Entities)
					{
						var health = entity.GetComponent<Health>();
						if (health != null && health.currentHP > 0)
						{
							allEnemiesAreDead = false;
							break;
						}
					}
				}
				if (allEnemiesAreDead)
				{
					NextLevel();
					return;
				}

				// Room transitions
				{
					var roomCenter = LevelHelpers.GetRoomCenter(level.CurrentRoom);
					var roomBounds = new Bounds(roomCenter, GameConfig.ROOM_SIZE);
					if (roomBounds.Contains(player.transform.position) == false)
					{
						var direction = Utils.SnapTo(player.transform.position - roomCenter, 90f);
						direction.y = -direction.y; // Reverse the Y axis because unity's is bottom > top and ours is top > bottom

						var nextRoom = LevelHelpers.GetRoomInDirection(direction, level);
						if (nextRoom != null)
						{
							LevelHelpers.TransitionToRoom(level, nextRoom);
							GameManager.Game.GameplayUI.SetMiniMap(level, allEnemiesAreDead);
							var destination = LevelHelpers.GetRoomCenter(level.CurrentRoom);
							GameManager.Game.CameraRig.transform.DOMove(destination, 0.3f);
						}
					}
				}

				GameManager.Game.GameplayUI.SetDash(player.DashProgress);

				if (player.IsFullyDead)
				{
					UnityEngine.Debug.Log("The player died, restarting the level.");
					FSM.Fire(GameFSM.Triggers.Retry);
					return;
				}
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

			GameObject.Destroy(GameManager.Game.State.Player.gameObject);
			GameManager.Game.State.Player = null;

			foreach (var room in GameManager.Game.State.Level.Rooms)
				if (room.Instance != null)
					GameObject.Destroy(room.Instance.gameObject);
			GameManager.Game.State.Level = null;
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
			GameManager.Game.State.PlayerSaveData.ClearedLevels.Add(GameManager.Game.State.CurrentLevelIndex);
			Save.SavePlayerSaveData(GameManager.Game.State.PlayerSaveData);
			GameManager.Game.State.CurrentLevelIndex += 1;

			AudioHelpers.PlayOneShot(GameManager.Game.Config.StageClear);

			await UniTask.Delay(1000);

			if (GameManager.Game.State.CurrentLevelIndex >= GameManager.Game.Config.Levels.Length)
			{
				Victory();
				return;
			}

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
