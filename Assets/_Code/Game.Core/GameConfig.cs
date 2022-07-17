using System;
using FMODUnity;
using UnityEngine;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game/Game Config")]
	public class GameConfig : ScriptableObject
	{
		[Header("DEBUG")]
		public bool DebugStateMachine;
		public bool DebugSkipTitle;
		public bool DebugGottaGoFast;
		public int LockFPS = 60;

		[Header("Colors")]
		public Color ColorBackgroundDark;
		public Color ColorLight;

		[Header("CONTENT")]
		public int MaxFails = 5;
		public int ScoreFail = 2;
		public int ScoreMultiplier = 100;
		public DieTypeToSprite DieSprites;
		public Level[] Levels;

		[Header("AUDIO")]
		public string GameBus = "bus:/Game";
		public string MusicBus = "bus:/Game/Music";
		public string SoundBus = "bus:/Game/SFX";
		public EventReference SnapshotPause;
		public EventReference SoundMenuConfirm;
		public EventReference MusicMain;

		public static Vector3 ROOM_SIZE = new Vector2(15, 9);
	}

	[Serializable]
	public class DieTypeToSprite : SerializableDictionary<DieTypes, Sprite> { }
}
