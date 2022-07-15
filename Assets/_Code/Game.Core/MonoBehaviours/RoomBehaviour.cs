using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	public class RoomBehaviour : MonoBehaviour
	{
		[SerializeField] public Tilemap WallsTilemap;

		[SerializeField]
		[Tooltip("Order: North, East, South, West")]
		public Vector3Int[] DoorPositions = new Vector3Int[] {
			new Vector3Int(7, 8, 0),
			new Vector3Int(14, 4, 0),
			new Vector3Int(7, 0, 0),
			new Vector3Int(0, 4, 0),
		};

		[SerializeField]
		[Tooltip("Order: North, East, South, West")]
		public bool[] DoorPossibilities = new bool[] {
			true,
			true,
			true,
			true,
		};

		private void OnDrawGizmos()
		{
			if (Application.isPlaying == false)
			{
				Gizmos.color = Color.red;
				foreach (var doorPosition in DoorPositions)
				{
					Gizmos.DrawWireCube(doorPosition + new Vector3(0.5f, 0.5f), new Vector3(0.5f, 0.5f));
				}
			}
		}
	}
}
