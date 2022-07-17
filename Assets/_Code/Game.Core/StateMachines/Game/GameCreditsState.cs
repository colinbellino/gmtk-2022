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

			Globals.UI.SetDebugText("");

			await Globals.UI.ShowCredits();
			await Globals.UI.FadeIn(Color.clear);
		}

		public void Tick()
		{
			if (Time.time >= _readyTimestamp && Globals.Controls.Global.Cancel.WasPerformedThisFrame() || Globals.Controls.Global.Confirm.WasPerformedThisFrame())
			{
				FSM.Fire(GameFSM.Triggers.Done);
			}
		}

		public void FixedTick() { }

		public async UniTask Exit()
		{
			Globals.State.EndMusic.stop(STOP_MODE.ALLOWFADEOUT);
			await Globals.UI.FadeIn(Globals.Config.ColorBackgroundDark);
			await Globals.UI.HideCredits();
		}
	}
}
