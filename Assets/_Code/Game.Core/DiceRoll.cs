using UnityEngine;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game/Dice Roll")]
	public class DiceRoll : ScriptableObject
	{
		public int Quantity = 1;
		public DieTypes Type;
		public int Modifier;
	}
}
