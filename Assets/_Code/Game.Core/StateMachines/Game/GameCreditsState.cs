using Cysharp.Threading.Tasks;
using UnityEngine;
using FMOD.Studio;

namespace Game.Core.StateMachines.Game
{
	public class GameCreditsState : IState
	{
		public GameFSM FSM;
		private float _readyTimestamp;

		public async UniTask Enter()
		{
			_readyTimestamp = Time.time + 1f;

			GameManager.Game.UI.SetDebugText("");

			await GameManager.Game.UI.ShowCredits();
			await GameManager.Game.UI.FadeIn(Color.clear);
		}

		public void Tick()
		{
			if (Time.time >= _readyTimestamp && GameManager.Game.Controls.Global.Cancel.WasPerformedThisFrame() || GameManager.Game.Controls.Global.Confirm.WasPerformedThisFrame())
			{
				FSM.Fire(GameFSM.Triggers.Done);
			}
		}

		public void FixedTick() { }

		public async UniTask Exit()
		{
			GameManager.Game.State.EndMusic.stop(STOP_MODE.ALLOWFADEOUT);
			await GameManager.Game.UI.FadeIn(Color.black);
			await GameManager.Game.UI.HideCredits();
		}
	}
}
