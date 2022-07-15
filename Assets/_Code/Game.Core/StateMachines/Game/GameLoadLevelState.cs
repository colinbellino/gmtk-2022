using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Core.StateMachines.Game
{
	public class GameLoadLevelState : IState
	{
		public GameFSM FSM;

		public async UniTask Enter()
		{
			var levelName = GameManager.Game.State.AllLevels[GameManager.Game.State.CurrentLevelIndex];
			var levelData = await Utils.ReadStreamingAsset($"/Levels~/{levelName}.txt");
			var level = LevelHelpers.InstantiateLevel(levelData);
			level.StartRoom = LevelHelpers.GetStartRoom(level);
			level.CurrentRoom = level.StartRoom;

			GameManager.Game.CameraRig.transform.position = LevelHelpers.GetRoomOrigin(level.CurrentRoom);

			GameManager.Game.State.Level = level;

			await UniTask.NextFrame();

			FSM.Fire(GameFSM.Triggers.Done);
		}

		public UniTask Exit()
		{
			return default;
		}

		public void FixedTick() { }

		public void Tick() { }
	}
}
