using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;

namespace PyxelEdit
{
	// Source: https://github.com/martinhodler/unity-aseprite-importer/blob/master/Editor/SpriteAtlasBuilder.cs
	// TODO: Check license for using this file or build our own
	public class SpriteAtlasBuilder
	{
		private readonly Vector2Int spriteSize;
		private int padding = 1;


		public SpriteAtlasBuilder()
		{
			spriteSize = new Vector2Int(16, 16);
		}

		public SpriteAtlasBuilder(Vector2Int spriteSize, int padding = 1)
		{
			this.spriteSize = spriteSize;
			this.padding = padding;
		}

		public SpriteAtlasBuilder(int width, int height, int padding = 1)
		{
			spriteSize = new Vector2Int(width, height);
			this.padding = padding;
		}

		public Texture2D GenerateAtlas(Texture2D[] sprites, out SpriteImportData[] spriteImportData, bool baseTwo = true)
		{
			int cols, rows;

			CalculateColsRows(sprites.Length, spriteSize, out cols, out rows);

			return GenerateAtlas(sprites, cols, rows, out spriteImportData, baseTwo);
		}

		public Texture2D GenerateAtlas(Texture2D[] sprites, int cols, int rows, out SpriteImportData[] spriteImportData, bool baseTwo = false)
		{
			// UnityEngine.Debug.Log("GenerateAtlas: " + cols + "x" + rows);
			spriteImportData = new SpriteImportData[sprites.Length];

			var width = cols * spriteSize.x;
			var height = rows * spriteSize.y;

			if (baseTwo)
			{
				var baseTwoValue = CalculateNextBaseTwoValue(Math.Max(width, height));
				width = baseTwoValue;
				height = baseTwoValue;
			}

			var atlas = CreateTransparentTexture(width, height);
			var index = 0;

			for (var row = 0; row < rows; ++row)
			{
				for (var col = 0; col < cols; ++col)
				{
					Rect spriteRect = new Rect(
						col * spriteSize.x,
						atlas.height - ((row + 1) * spriteSize.y),
						spriteSize.x,
						spriteSize.y
					);
					Color[] colors = sprites[index].GetPixels();
					atlas.SetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height, sprites[index].GetPixels());
					atlas.Apply();

					List<Vector2[]> outline = GenerateRectOutline(spriteRect);
					spriteImportData[index] = CreateSpriteImportData(index.ToString(), spriteRect, outline);

					index++;
					if (index >= sprites.Length)
						break;
				}
				if (index >= sprites.Length)
					break;
			}

			return atlas;
		}

		public static List<Vector2Int[]> GenerateRectOutline(RectInt rect)
		{
			List<Vector2Int[]> outline = new List<Vector2Int[]>();

			outline.Add(new Vector2Int[] { new Vector2Int(rect.x, rect.y), new Vector2Int(rect.x, rect.yMax) });
			outline.Add(new Vector2Int[] { new Vector2Int(rect.x, rect.yMax), new Vector2Int(rect.xMax, rect.yMax) });
			outline.Add(new Vector2Int[] { new Vector2Int(rect.xMax, rect.yMax), new Vector2Int(rect.xMax, rect.y) });
			outline.Add(new Vector2Int[] { new Vector2Int(rect.xMax, rect.y), new Vector2Int(rect.x, rect.yMax) });

			return outline;
		}

		public static List<Vector2[]> GenerateRectOutline(Rect rect)
		{
			List<Vector2[]> outline = new List<Vector2[]>();

			outline.Add(new Vector2[] { new Vector2(rect.x, rect.y), new Vector2(rect.x, rect.yMax) });
			outline.Add(new Vector2[] { new Vector2(rect.x, rect.yMax), new Vector2(rect.xMax, rect.yMax) });
			outline.Add(new Vector2[] { new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.y) });
			outline.Add(new Vector2[] { new Vector2(rect.xMax, rect.y), new Vector2(rect.x, rect.yMax) });

			return outline;
		}

		private SpriteImportData CreateSpriteImportData(string name, Rect rect, List<Vector2[]> outline)
		{
			return new SpriteImportData()
			{
				alignment = SpriteAlignment.Center,
				border = Vector4.zero,
				name = name,
				outline = outline,
				pivot = new Vector2(0.5f, 0.5f),
				rect = rect,
				spriteID = GUID.Generate().ToString(),
				tessellationDetail = 0
			};
		}

		private static void CalculateColsRows(int spritesCount, Vector2 spriteSize, out int cols, out int rows)
		{
			float minDifference = float.MaxValue;
			cols = spritesCount;
			rows = 1;

			float width = spriteSize.x * cols;
			float height = spriteSize.y * rows;



			for (rows = 1; rows < spritesCount; ++rows)
			{
				cols = Mathf.CeilToInt((float)spritesCount / rows);

				width = spriteSize.x * cols;
				height = spriteSize.y * rows;

				float difference = Mathf.Abs(width - height);
				if (difference < minDifference)
				{
					minDifference = difference;
				}
				else
				{
					rows -= 1;
					cols = Mathf.CeilToInt((float)spritesCount / rows);
					break;
				}
			}
		}

		private static int CalculateNextBaseTwoValue(int value)
		{
			var exponent = 0;
			var baseTwo = 0;

			while (baseTwo < value)
			{
				baseTwo = (int)Math.Pow(2, exponent);
				exponent++;
			}

			return baseTwo;
		}

		public static Texture2D CreateTransparentTexture(int width, int height)
		{
			Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
			Color[] pixels = new UnityEngine.Color[width * height];

			for (int i = 0; i < pixels.Length; i++) pixels[i] = UnityEngine.Color.clear;

			texture.SetPixels(pixels);
			texture.Apply();

			return texture;
		}
	}
}
