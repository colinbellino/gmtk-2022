using UnityEditor;
using UnityEngine;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game/Dice Roll")]
	public class DiceRoll : ScriptableObject
	{
		public string Name;
		public int Quantity = 1;
		public DieTypes Type;
		public int Modifier;

		[ContextMenu("Rename")]
		private void Rename()
		{
			var newName = $"{Type} x{Quantity} +{Modifier} - {Name}";
			string assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
			AssetDatabase.RenameAsset(assetPath, newName);
			AssetDatabase.SaveAssets();
		}
	}
}
