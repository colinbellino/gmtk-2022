using Cysharp.Threading.Tasks;
using UnityEngine;
using FMOD.Studio;

namespace Game.Core.StateMachines.Game
{
	public class GameEndingState : IState
	{
		public GameFSM FSM;
		private float _readyTimestamp;

		public async UniTask Enter()
		{
			_readyTimestamp = Time.time + 1f;

			await GameManager.Game.UI.ShowEnding(0);
			await GameManager.Game.UI.FadeIn(Color.clear);

			GameManager.Game.State.EndMusic.getPlaybackState(out var state);
			if (state != PLAYBACK_STATE.PLAYING)
				GameManager.Game.State.EndMusic.start();
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
			await GameManager.Game.UI.FadeIn(Color.black);
			await GameManager.Game.UI.HideEnding(0);
		}
	}
}
