using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Core.StateMachines.Game
{
	public class GameLoadLevelState : IState
	{
		public GameFSM FSM;

		public async UniTask Enter()
		{
			// var levelName = Globals.State.AllLevels[Globals.State.CurrentLevelIndex];
			// var levelData = await Utils.ReadStreamingAsset($"/Levels~/{levelName}.txt");
			// var level = LevelHelpers.InstantiateLevel(levelData);
			// level.StartRoom = LevelHelpers.GetStartRoom(level);
			// level.CurrentRoom = level.StartRoom;

			// Globals.CameraRig.transform.position = LevelHelpers.GetRoomOrigin(level.CurrentRoom);

			// Globals.State.Level = level;

			// await UniTask.NextFrame();

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
