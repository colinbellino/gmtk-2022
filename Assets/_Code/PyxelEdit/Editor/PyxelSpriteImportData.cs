// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor.AssetImporters;

// namespace PyxelEdit
// {
// 	// Source: https://github.com/martinhodler/unity-aseprite-importer/blob/master/Editor/Data/AseFileSpriteImportData.cs
// 	// TODO: Check license for using this file or build our own
// 	[Serializable]
// 	public class PyxelSpriteImportData
// 	{
// 		public string name;

// 		//     Position and size of the Sprite in a given texture.
// 		public Rect rect;

// 		//     Pivot value represented by SpriteAlignment.
// 		public SpriteAlignment alignment;

// 		//     Pivot value represented in Vector2.
// 		public Vector2 pivot;

// 		//     Border value for the generated Sprite.
// 		public Vector4 border;

// 		//     Sprite Asset creation uses this outline when it generates the Mesh for the Sprite.
// 		//     If this is not given, SpriteImportData.tesselationDetail will be used to determine
// 		//     the mesh detail.
// 		public List<Vector2[]> outline;

// 		//     Controls mesh generation detail. This value will be ignored if SpriteImportData.ouline
// 		//     is provided.
// 		public float tessellationDetail;

// 		//     An identifier given to a Sprite. Use this to identify which data was used to
// 		//     generate that Sprite.
// 		public string spriteID;


// 		public SpriteImportData ToSpriteImportData()
// 		{
// 			return new SpriteImportData()
// 			{
// 				alignment = alignment,
// 				border = border,
// 				name = name,
// 				outline = outline,
// 				pivot = pivot,
// 				rect = rect,
// 				spriteID = spriteID,
// 				tessellationDetail = tessellationDetail
// 			};
// 		}
// 	}
// }
