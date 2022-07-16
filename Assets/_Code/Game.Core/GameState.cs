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

		public EventInstance TitleMusic;
		public EventInstance LevelMusic;
		public EventInstance EndMusic;
		public EventInstance PauseSnapshot;

		public PlayerSettings PlayerSettings;
		public PlayerSaveData PlayerSaveData;

		public int CurrentLevelIndex;
		public List<DiceRequest> Requests;
		public List<int> QueuedRequests;
		public List<int> ActiveRequests;
		public List<int> CompletedRequests;
		public List<int> FailedRequests;
		public int Score;
		public float Timer;
	}

	[Serializable]
	public class DiceRequest
	{
		public DiceRoll Roll;
		public bool FromDM;
		public float Offset;
		[NonSerialized] public float Timestamp;
	}

	public enum DieTypes
	{
		D4 = 4,
		D6 = 6,
		D8 = 8,
		D10 = 10,
		D12 = 12,
		D20 = 20,
		D100 = 100,
	}

	public class DiceBag
	{
		public DieTypes Die;
		public int Quantity;
		public int Modifier;
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
