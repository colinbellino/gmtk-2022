using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;

namespace Game.Core.StateMachines.Game
{
	public class GameIntroState : IState
	{
		public GameFSM FSM;

		public async UniTask Enter()
		{
			await Globals.UI.ShowIntro(0);
			await Globals.UI.FadeIn(Color.clear);
		}

		public void Tick()
		{
			if (Globals.Controls.Global.Cancel.WasPerformedThisFrame() || Globals.Controls.Global.Confirm.WasPerformedThisFrame())
			{
				FSM.Fire(GameFSM.Triggers.Done);
			}
		}

		public void FixedTick() { }

		public async UniTask Exit()
		{
			await Globals.UI.FadeIn(Globals.Config.ColorBackgroundDark);
			await Globals.UI.HideIntro(0);
			// Globals.State.MusicTitle.stop(STOP_MODE.ALLOWFADEOUT);
		}
	}
}
