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

#if UNITY_EDITOR
		[ContextMenu("Rename")]
		private void Rename()
		{
			var newName = $"{Type} x{Quantity} +{Modifier} - {Name}";
			string assetPath = UnityEditor.AssetDatabase.GetAssetPath(GetInstanceID());
			UnityEditor.AssetDatabase.RenameAsset(assetPath, newName);
			UnityEditor.AssetDatabase.SaveAssets();
		}
#endif
	}
}
