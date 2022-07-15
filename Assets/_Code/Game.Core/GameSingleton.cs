using System.Collections.Generic;
using Game.Core.StateMachines.Game;
using Game.Inputs;

namespace Game.Core
{
	public class GameSingleton
	{
		public GameConfig Config;
		public GameUI UI;
		public PauseUI PauseUI;
		public OptionsUI OptionsUI;
		public ControlsUI ControlsUI;
		public GameplayUI GameplayUI;
		public CameraRig CameraRig;
		public GameControls Controls;
		public GameState State;
		public GameFSM GameFSM;
	}
}
