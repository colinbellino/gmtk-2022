using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game/Game Config")]
	public class GameConfig : ScriptableObject
	{
		[Header("DEBUG")]
		public bool DebugStateMachine;
		public bool DebugLevels;
		public int LockFPS = 60;

		[Header("CONTENT")]
		public PlayerController Player;
		public string[] Levels = new string[0];
		public TileToEntity TileToEntity;
		public Tile[] WallTiles;

		[Header("AUDIO")]
		public string GameBus = "bus:/Game";
		public string MusicBus = "bus:/Game/Music";
		public string SoundBus = "bus:/Game/SFX";
		public EventReference SnapshotPause;
		public EventReference SoundMenuConfirm;
		public EventReference MusicTitle;
		public EventReference MusicMain;
		public EventReference MusicEnd;
		public EventReference PlayerFootsteps;
		public EventReference HeartBeat;
		public EventReference PlayerDash;
		public EventReference PlayerDeath;
		public EventReference PlayerYawn;
		public EventReference PlayerAttack;
		public EventReference PlayerDamage;
		public EventReference EnemyStun;
		public EventReference EnemyDamage;
		public EventReference VillagerEnemyMovement;
		public EventReference PriestEnemyAttack;
		public EventReference StageClear;
		public EventReference PlayerHeal;
		public EventReference BatProjectileExplode;

		public static Vector3 ROOM_SIZE = new Vector2(15, 9);
	}

	[Serializable]
	public class TileToEntity : SerializableDictionary<TileBase, Entity> { }

	[Serializable]
	public class TileToInfo : SerializableDictionary<TileBase, TileInfo> { }

	[Serializable]
	public class TileInfo
	{
		public bool Walkable;
	}
}
