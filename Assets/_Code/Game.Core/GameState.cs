using System;
using System.Collections.Generic;
using FMOD.Studio;

namespace Game.Core
{
	public class GameState
	{
		public string Version;
		public string Commit;

		public Unity.Mathematics.Random Random;
		public bool Running;
		public bool Paused;
		public float TimeScaleCurrent;
		public float TimeScaleDefault;

		public InputTypes CurrentInputType;

		public EventInstance TitleMusic;
		public EventInstance LevelMusic;
		public EventInstance EndMusic;
		public EventInstance PauseSnapshot;

		public int CurrentLevelIndex;
		public Level Level;
		public PlayerController Player;
		public string[] DebugLevels;
		public string[] AllLevels;

		public PlayerSettings PlayerSettings;
		public PlayerSaveData PlayerSaveData;
	}

	public enum InputTypes { Keyboard, XInputController, DualShockGamepad }

	[Serializable]
	public struct PlayerSettings
	{
		public string LocaleCode;

		public float GameVolume;
		public float SoundVolume;
		public float MusicVolume;

		public bool FullScreen;
		public int ResolutionWidth;
		public int ResolutionHeight;
		public int ResolutionRefreshRate;

		public bool Screenshake;
		public bool AssistMode;
	}

	[Serializable]
	public struct PlayerSaveData
	{
		public HashSet<int> ClearedLevels;
	}
}
