using Cysharp.Threading.Tasks;
using UnityEngine;
using FMOD.Studio;
using UnityEngine.InputSystem;

namespace Game.Core.StateMachines.Game
{
	public class GameEndingState : IState
	{
		public GameFSM FSM;
		private float _readyTimestamp;

		public async UniTask Enter()
		{
			_readyTimestamp = Time.time + 0.5f;

			await Globals.UI.ShowEnding(0);
			await Globals.UI.FadeIn(Color.clear);

			// Globals.State.EndMusic.getPlaybackState(out var state);
			// if (state != PLAYBACK_STATE.PLAYING)
			// 	Globals.State.EndMusic.start();
		}

		public void Tick()
		{
			if (Time.time >= _readyTimestamp && (Keyboard.current.anyKey.wasReleasedThisFrame || Globals.Controls.Global.Cancel.WasPerformedThisFrame() || Globals.Controls.Global.Confirm.WasPerformedThisFrame()))
			{
				FSM.Fire(GameFSM.Triggers.Done);
			}
		}

		public void FixedTick() { }

		public async UniTask Exit()
		{
			await Globals.UI.FadeIn(Globals.Config.ColorBackgroundDark);
			await Globals.UI.HideEnding(0);
		}
	}
}
