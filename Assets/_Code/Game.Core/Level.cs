using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using System.IO;

namespace Game.Core
{
	public static class LevelHelpers
	{
		private static Vector3Int[] DIRECTIONS = new Vector3Int[] {
			Vector3Int.up,
			Vector3Int.right,
			Vector3Int.down,
			Vector3Int.left,
		};

		private static char RoomTypeEmpty = '.';
		private static char RoomTypeSpawn = 'S';

		public static Room GetStartRoom(Level level)
		{
			foreach (var room in level.Rooms)
			{
				if (room.Type == RoomTypeSpawn)
					return room;
			}

			return null;
		}

		public static Room GetRoomInDirection(Vector2 direction, Level level)
		{
			var x = 0;
			var y = 0;
			if (math.round(direction.x) > 0)
				x = 1;
			else if (math.round(direction.x) < 0)
				x = -1;
			if (math.round(direction.y) > 0)
				y = 1;
			else if (math.round(direction.y) < 0)
				y = -1;

			var positionX = level.CurrentRoom.X + x;
			var positionY = level.CurrentRoom.Y + y;
			var room = GetRoomAtPosition(positionX, positionY, level);

			return room;
		}

		public static Vector3 GetRoomCenter(Room room)
		{
			return new Vector3(room.X * GameConfig.ROOM_SIZE.x + GameConfig.ROOM_SIZE.x / 2, -room.Y * GameConfig.ROOM_SIZE.y + GameConfig.ROOM_SIZE.y / 2);
		}

		public static Vector3 GetRoomOrigin(Room room)
		{
			return new Vector3(room.X * GameConfig.ROOM_SIZE.x, -room.Y * GameConfig.ROOM_SIZE.y);
		}

		public static Room GetRoomAtPosition(int x, int y, Level level)
		{
			foreach (var room in level.Rooms)
			{
				if (room.X == x && room.Y == y && room.Instance != null)
					return room;
			}

			return null;
		}

		public static void TransitionToRoom(Level level, Room roomToTransitionTo)
		{
			level.CurrentRoom.Explored = true;
			DeactivateRoom(level.CurrentRoom);
			ActivateRoom(roomToTransitionTo);

			level.CurrentRoom = roomToTransitionTo;
		}

		public static void ActivateRoom(Room roomToActivate)
		{
			// TODO: close door, spawn entities, etc
			// UnityEngine.Debug.Log("Activating room: " + roomToActivate);
			foreach (var entity in roomToActivate.Entities)
			{
				entity.gameObject.SetActive(true);
			}

#if UNITY_EDITOR
			roomToActivate.Instance.name += " CURRENT";
#endif
		}

		public static void DeactivateRoom(Room roomToDeactivate)
		{
			// UnityEngine.Debug.Log("Deactivating room: " + roomToDeactivate);
			foreach (var entity in roomToDeactivate.Entities)
			{
				entity.gameObject.SetActive(false);
			}

#if UNITY_EDITOR
			roomToDeactivate.Instance.name = roomToDeactivate.Instance.name.Replace(" CURRENT", "");
#endif
		}

		public static Level InstantiateLevel(string levelData)
		{
			levelData = levelData.Trim();

			var level = new Level
			{
				Rooms = new List<Room>(),
			};

			var x = 0;
			var y = 0;
			var i = 0;
			var width = 0;
			foreach (var roomType in levelData)
			{
				if (roomType == '\n')
				{
					x = 0;
					y += 1;
					continue;
				}

				RoomBehaviour roomInstance = null;
				var entities = new List<Entity>();

				if (roomType != RoomTypeEmpty)
				{
					var roomPrefab = Resources.Load<RoomBehaviour>("Rooms/Room " + roomType);
					roomInstance = GameObject.Instantiate(roomPrefab);
					roomInstance.transform.position = new Vector3(x * GameConfig.ROOM_SIZE.x, -y * GameConfig.ROOM_SIZE.y);
#if UNITY_EDITOR
					roomInstance.name = $"[{x},{y}] {roomType}";
#endif

					foreach (Transform child in roomInstance.transform)
					{
						var entity = child.gameObject.GetComponent<Entity>();
						if (entity == null)
							continue;

						entity.SpawnPosition = entity.transform.localPosition;
						entity.Ready = true;
						entity.gameObject.SetActive(false);
						entities.Add(entity);
					}
				}

				var room = new Room
				{
					X = x,
					Y = y,
					Index = i,
					Type = roomType,
					Instance = roomInstance,
					Entities = entities,
				};
				level.Rooms.Add(room);

				x += 1;
				i += 1;
				width = x;
			}

			level.Width = width;
			level.Height = y + 1;

			// Do a second loop once we have all the rooms to check for connections for doors.
			foreach (var room in level.Rooms)
			{
				if (room.Instance == null)
					continue;

				for (int directionIndex = 0; directionIndex < DIRECTIONS.Length; directionIndex++)
				{
					var direction = DIRECTIONS[directionIndex];
					direction.y = -direction.y;
					var nextRoom = GetRoomAtPosition(room.X + direction.x, room.Y + direction.y, level);
					if (nextRoom == null)
					{
						room.Instance.WallsTilemap.SetTile(room.Instance.DoorPositions[directionIndex], GameManager.Game.Config.WallTiles[directionIndex]);
					}
				}
			}

			return level;
		}
	}

	public class Level
	{
		public int Width;
		public int Height;
		public List<Room> Rooms;
		public Room StartRoom;
		public Room CurrentRoom;
	}

	public class Room
	{
		public char Type;
		public int Index;
		public int X;
		public int Y;
		public bool Explored;
		public RoomBehaviour Instance;
		public List<Entity> Entities;

		public override string ToString()
		{
			return $"Room [{X},{Y}]: {Type}";
		}
	}
}
