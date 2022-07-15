using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;

namespace Game.Core.StateMachines.Game
{
	public class GameSelectLevelState : IState
	{
		public GameFSM FSM;
		private bool _loading;

		public async UniTask Enter()
		{
			_loading = false;
			await GameManager.Game.UI.ShowLevelSelection(0);
			for (int i = 0; i < GameManager.Game.UI.LevelButtons.Length; i++)
			{
				var button = GameManager.Game.UI.LevelButtons[i];
				int levelIndex = i;
				button.Button.onClick.AddListener(() => LoadLevel(levelIndex));
			}

			await GameManager.Game.UI.FadeIn(Color.clear);
		}

		public void Tick()
		{
			if (GameManager.Game.Controls.Global.Cancel.WasPerformedThisFrame())
			{
				GameManager.Game.State.LevelMusic.stop(STOP_MODE.ALLOWFADEOUT);
				FSM.Fire(GameFSM.Triggers.Quit);
			}
		}

		public void FixedTick() { }

		public async UniTask Exit()
		{
			for (int i = 0; i < GameManager.Game.UI.LevelButtons.Length; i++)
			{
				var button = GameManager.Game.UI.LevelButtons[i];
				int levelIndex = i;
				button.Button.onClick.RemoveListener(() => LoadLevel(levelIndex));
			}

			await GameManager.Game.UI.FadeIn(Color.black);
			await GameManager.Game.UI.HideLevelSelection(0);
		}

		private async void LoadLevel(int levelIndex)
		{
			if (_loading)
				return;

			_loading = true;
			Debug.Log($"Loading level {levelIndex}.");
			GameManager.Game.State.CurrentLevelIndex = levelIndex;
			GameManager.Game.State.TitleMusic.stop(STOP_MODE.ALLOWFADEOUT);
			await GameManager.Game.UI.FadeIn(Color.black);
			FSM.Fire(GameFSM.Triggers.LevelSelected);
		}
	}
}
