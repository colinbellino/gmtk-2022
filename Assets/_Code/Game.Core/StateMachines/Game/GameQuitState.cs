using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Game.Core.StateMachines.Game
{
	public class GameQuitState : IState
	{
		public GameFSM FSM;

		public async UniTask Enter()
		{
			Globals.Controls.Disable();
			AudioHelpers.StopEverything();

			if (Utils.IsWebGL())
			{
				await SceneManager.LoadSceneAsync("WebGLQuit", LoadSceneMode.Single);
				return;
			}

#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#else
			UnityEngine.Application.Quit();
#endif
		}

		public void Tick() { }

		public void FixedTick() { }

		public UniTask Exit()
		{
			return default;
		}
	}
}
