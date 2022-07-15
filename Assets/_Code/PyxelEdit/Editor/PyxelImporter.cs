using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEditor.U2D.Sprites;
using System;

namespace PyxelEdit
{
	[ScriptedImporter(1, "pyxel")]
	public class PyxelImporter : ScriptedImporter, ISpriteEditorDataProvider
	{
		[HideInInspector] public Texture2D Texture;
		[HideInInspector] public SpriteRect[] SpriteRects = new SpriteRect[0];
		[HideInInspector] public PyxelSpriteImportData[] SpriteImportData;

		[SerializeField] public PyxelTextureImportSettings ImportSettings;
		[SerializeField] [ReadOnly] public PyxelDocData _data;
		[SerializeField] [HideInInspector] private string _serializedSpriteImportData;

		public override void OnImportAsset(AssetImportContext ctx)
		{
			var entries = new Dictionary<string, byte[]>();
			var layers = new List<string>();
			var tiles = new List<string>();
			var archiveName = Path.GetFileNameWithoutExtension(ctx.assetPath);

			using (var archive = ZipFile.Open(ctx.assetPath, ZipArchiveMode.Read))
			{
				foreach (var entry in archive.Entries)
				{
					if (entry.FullName.StartsWith("layer"))
						layers.Add(entry.FullName);

					if (entry.FullName.StartsWith("tile"))
						tiles.Add(entry.FullName);

					var stream = entry.Open();
					var memoryStream = new MemoryStream();
					stream.CopyTo(memoryStream);
					stream.Close();

					entries.Add(entry.FullName, memoryStream.GetBuffer());
				}
			}

			{
				var filename = "docData.json";
				var dataString = Encoding.UTF8.GetString(entries[filename]);
				_data = JsonConvert.DeserializeObject<PyxelDocData>(dataString);

				var text = new TextAsset(dataString);
				text.name = filename;

				ctx.AddObjectToAsset(filename, text);
			}

			// Generate preview
			{
				var previewPixels = new Color32[0];

				for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
				{
					var filename = layers[layerIndex];

					var texture = new Texture2D(_data.tileset.tileWidth, _data.tileset.tileHeight);
					texture.LoadImage(entries[filename]);
					texture.name = archiveName + "_layer_" + layerIndex + "_texture_preview";
					texture.hideFlags = HideFlags.HideInHierarchy;
					texture.filterMode = ImportSettings.filterMode;

					ctx.AddObjectToAsset(texture.name, texture, texture);

					var sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2), texture.width);
					sprite.name = archiveName + "_layer_" + layerIndex;

					ctx.AddObjectToAsset(sprite.name, sprite, sprite.texture);

					var pixels = texture.GetPixels32();
					if (previewPixels.Length == 0)
					{
						previewPixels = pixels;
						continue;
					}

					for (int i = 0; i < pixels.Length; i++)
						previewPixels[i] = Color.Lerp(pixels[i], previewPixels[i], previewPixels[i].a / 1.0f);
				}

				{
					var filename = "Preview";
					var texture = new Texture2D(_data.canvas.width, _data.canvas.height);
					texture.name = filename;
					texture.filterMode = ImportSettings.filterMode;
					texture.SetPixels32(previewPixels);

					ctx.AddObjectToAsset(filename, texture, texture);
					ctx.SetMainObject(texture);
				}
			}

			// Generate tiles per layer
			{
				for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
				{
					var filename = layers[layerIndex];
					var tileIndex = 0;

					var texture = new Texture2D(_data.tileset.tileWidth, _data.tileset.tileHeight);
					texture.LoadImage(entries[filename]);
					texture.name = archiveName + "_layer_" + layerIndex + "_texture";
					texture.hideFlags = HideFlags.HideInHierarchy;
					texture.filterMode = ImportSettings.filterMode;
					ctx.AddObjectToAsset(texture.name, texture, texture);

					for (int x = 0; x < _data.canvas.width; x += _data.canvas.tileWidth)
					{
						for (int y = 0; y < _data.canvas.height; y += _data.canvas.tileHeight)
						{
							var sprite = Sprite.Create(texture, new Rect(x, y, _data.canvas.tileWidth, _data.canvas.tileHeight), new Vector2(_data.canvas.tileWidth / 2, _data.canvas.tileHeight / 2), _data.canvas.tileWidth);
							sprite.name = archiveName + "_layer_" + layerIndex + "_" + tileIndex;

							ctx.AddObjectToAsset(sprite.name, sprite, sprite.texture);

							tileIndex += 1;
						}
					}
				}
			}

			// Generate tileset
			{
				var rows = Mathf.Max(_data.tileset.numTiles / _data.tileset.tilesWide, 1);
				var cols = Mathf.Min(_data.tileset.tilesWide, _data.tileset.numTiles);

				var textureInformation = new SourceTextureInformation()
				{
					containsAlpha = true,
					hdr = false,
					width = cols * _data.tileset.tileWidth,
					height = rows * _data.tileset.tileHeight,
				};
				var platformSettings = new TextureImporterPlatformSettings()
				{
					overridden = false,
				};

				var textures = new Texture2D[_data.tileset.numTiles];

				var textureImporterSettings = new TextureImporterSettings()
				{
					seamlessCubemap = ImportSettings.seamlessCubemap,
					mipmapBias = ImportSettings.mipmapBias,
					wrapMode = ImportSettings.wrapMode,
					wrapModeU = ImportSettings.wrapModeU,
					wrapModeV = ImportSettings.wrapModeV,
					wrapModeW = ImportSettings.wrapModeW,
					alphaIsTransparency = ImportSettings.alphaIsTransparency,
					spriteMode = ImportSettings.spriteMode,
					spritePixelsPerUnit = ImportSettings.spritePixelsPerUnit,
					spriteTessellationDetail = ImportSettings.spriteTessellationDetail,
					spriteExtrude = ImportSettings.spriteExtrude,
					spriteMeshType = ImportSettings.spriteMeshType,
					spriteAlignment = ImportSettings.spriteAlignment,
					spritePivot = ImportSettings.spritePivot,
					spriteBorder = ImportSettings.spriteBorder,
					spriteGenerateFallbackPhysicsShape = ImportSettings.spriteGenerateFallbackPhysicsShape,
					aniso = ImportSettings.aniso,
					filterMode = ImportSettings.filterMode,
					cubemapConvolution = ImportSettings.cubemapConvolution,
					textureType = ImportSettings.textureType,
					textureShape = ImportSettings.textureShape,
					mipmapFilter = ImportSettings.mipmapFilter,
					mipmapEnabled = ImportSettings.mipmapEnabled,
					sRGBTexture = ImportSettings.sRGBTexture,
					fadeOut = ImportSettings.fadeOut,
					borderMipmap = ImportSettings.borderMipmap,
					mipMapsPreserveCoverage = ImportSettings.mipMapsPreserveCoverage,
					mipmapFadeDistanceStart = ImportSettings.mipmapFadeDistanceStart,
					alphaTestReferenceValue = ImportSettings.alphaTestReferenceValue,
					convertToNormalMap = ImportSettings.convertToNormalMap,
					heightmapScale = ImportSettings.heightmapScale,
					normalMapFilter = ImportSettings.normalMapFilter,
					alphaSource = ImportSettings.alphaSource,
					singleChannelComponent = ImportSettings.singleChannelComponent,
					readable = ImportSettings.readable,
					streamingMipmaps = ImportSettings.streamingMipmaps,
					streamingMipmapsPriority = ImportSettings.streamingMipmapsPriority,
					npotScale = ImportSettings.npotScale,
					generateCubemap = ImportSettings.generateCubemap,
					mipmapFadeDistanceEnd = ImportSettings.mipmapFadeDistanceEnd,
				};
				for (int i = 0; i < _data.tileset.numTiles; i++)
				{
					var filename = tiles[i];

					var texture = new Texture2D(_data.tileset.tileWidth, _data.tileset.tileHeight);
					texture.LoadImage(entries[filename]);

					textures[i] = texture;
				}

				var atlasBuilder = new SpriteAtlasBuilder(_data.tileset.tileWidth, _data.tileset.tileHeight, 0);
				var atlas = atlasBuilder.GenerateAtlas(textures, cols, rows, out var importData, false);

				SpriteImportData = new PyxelSpriteImportData[importData.Length];
				for (int i = 0; i < SpriteImportData.Length; i++)
				{
					SpriteImportData[i] = new PyxelSpriteImportData
					{
						alignment = importData[i].alignment,
						border = importData[i].border,
						name = string.Format("{0}_{1}", archiveName, importData[i].name),
						pivot = importData[i].pivot,
						rect = importData[i].rect,
						spriteID = importData[i].spriteID,
					};
				}
				var settings = new TextureGenerationSettings
				{
					assetPath = archiveName + "_texture_atlas",
					spriteImportData = ConvertAseFileSpriteImportDataToUnity(SpriteImportData),
					textureImporterSettings = textureImporterSettings,
					enablePostProcessor = false,
					sourceTextureInformation = textureInformation,
					qualifyForSpritePacking = true,
					platformSettings = platformSettings,
					spritePackingTag = "pyxel",
					secondarySpriteTextures = new SecondarySpriteTexture[0],
				};
				var output = TextureGenerator.GenerateTexture(settings, new NativeArray<Color32>(atlas.GetPixels32(), Allocator.Temp));

				Texture = output.texture;
				Texture.hideFlags = HideFlags.HideInHierarchy;
				ctx.AddObjectToAsset(Texture.name, Texture, Texture);

				foreach (var sprite in output.sprites)
					ctx.AddObjectToAsset(sprite.name, sprite, sprite.texture);

				foreach (var texture in textures)
					DestroyImmediate(texture);
			}

			AssetDatabase.WriteImportSettingsIfDirty(assetPath);

			AssetDatabase.Refresh();
			AssetDatabase.LoadAllAssetsAtPath(assetPath);
			EditorApplication.RepaintProjectWindow();
		}

		private static SpriteImportData[] ConvertAseFileSpriteImportDataToUnity(PyxelSpriteImportData[] spriteImportData)
		{
			SpriteImportData[] importData = new SpriteImportData[spriteImportData.Length];

			for (int i = 0; i < spriteImportData.Length; i++)
			{
				importData[i] = spriteImportData[i].ToSpriteImportData();
			}

			return importData;
		}

		#region ISpriteEditorDataProvider implementation

		private PyxelTextureDataProvider textureDataProvider;
		private PyxelOutlineDataProvider outlineDataProvider;

		public SpriteImportMode spriteImportMode => (SpriteImportMode)ImportSettings.spriteMode;
		public float pixelsPerUnit => ImportSettings.spritePixelsPerUnit;
		public UnityEngine.Object targetObject => this;

		public SpriteRect[] GetSpriteRects()
		{
			var spriteRects = new List<SpriteRect>();

			foreach (PyxelSpriteImportData importData in SpriteImportData)
			{
				spriteRects.Add(new SpriteRect()
				{
					spriteID = ConvertStringToGUID(importData.spriteID),
					alignment = importData.alignment,
					border = importData.border,
					name = importData.name,
					pivot = importData.pivot,
					rect = importData.rect
				});
			}

			SpriteRects = spriteRects.ToArray();
			return SpriteRects;
		}

		public void SetSpriteRects(SpriteRect[] spriteRects)
		{
			SpriteRects = spriteRects;
		}

		public void Apply()
		{
			UnityEngine.Debug.Log("aPlly");
			if (SpriteRects != null && SpriteRects.Length > 0)
			{
				List<PyxelSpriteImportData> newImportData = new List<PyxelSpriteImportData>();

				foreach (SpriteRect spriteRect in SpriteRects)
				{
					var data = new PyxelSpriteImportData()
					{
						alignment = spriteRect.alignment,
						border = spriteRect.border,
						name = spriteRect.name,
						pivot = spriteRect.pivot,
						rect = spriteRect.rect,
						spriteID = spriteRect.spriteID.ToString()
					};

					var current = Array.Find(SpriteImportData, d => d.spriteID == spriteRect.spriteID.ToString());
					var currentIndex = Array.FindIndex(SpriteImportData, d => d.spriteID == spriteRect.spriteID.ToString());

					if (currentIndex > -1)
					{
						data.outline = current.outline;
						data.tessellationDetail = current.tessellationDetail;
					}
					else
					{
						data.outline = SpriteAtlasBuilder.GenerateRectOutline(data.rect);
						data.tessellationDetail = 0;
					}

					newImportData.Add(data);
				}

				SpriteRects = new SpriteRect[0];

				SpriteImportData = newImportData.ToArray();
				EditorUtility.SetDirty(this);
			}

			AssetDatabase.WriteImportSettingsIfDirty(assetPath);
			//SaveAndReimport();

			AssetDatabase.Refresh();
			AssetDatabase.LoadAllAssetsAtPath(assetPath);
			EditorApplication.RepaintProjectWindow();
		}

		public void InitSpriteEditorDataProvider()
		{
			textureDataProvider = new PyxelTextureDataProvider(this);
			outlineDataProvider = new PyxelOutlineDataProvider(this);
		}

		public T GetDataProvider<T>() where T : class
		{
			if (typeof(T) == typeof(ITextureDataProvider))
				return textureDataProvider as T;

			if (typeof(T) == typeof(ISpriteOutlineDataProvider))
				return outlineDataProvider as T;

			if (typeof(T) == typeof(ISpriteEditorDataProvider))
				return this as T;

			Debug.Log(typeof(T).Name + " not found");
			return null;
		}

		public bool HasDataProvider(Type type)
		{
			if (type == typeof(ITextureDataProvider))
				return true;

			if (type == typeof(ISpriteOutlineDataProvider))
				return true;

			// Debug.Log("Does not support" + type.Name);
			return false;
		}

		#endregion

		private GUID ConvertStringToGUID(string guidString)
		{
			if (!GUID.TryParse(guidString, out GUID guid))
			{
				guid = GUID.Generate();
			}

			return guid;
		}
	}

	public class ReadOnlyAttribute : PropertyAttribute { }

	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}

	public class PyxelTextureDataProvider : ITextureDataProvider
	{
		private readonly PyxelImporter importer;

		public PyxelTextureDataProvider(PyxelImporter importer)
		{
			this.importer = importer;
		}

		public Texture2D texture => importer.Texture;

		public Texture2D previewTexture => importer.Texture;

		public Texture2D GetReadableTexture2D()
		{
			if (importer.ImportSettings.spriteMode == (int)SpriteImportMode.Multiple)
			{
				return importer.Texture;
			}
			return null;
		}

		public void GetTextureActualWidthAndHeight(out int width, out int height)
		{
			width = importer.Texture.width;
			height = importer.Texture.height;
		}
	}

	public class PyxelOutlineDataProvider : ISpriteOutlineDataProvider
	{
		private readonly PyxelImporter importer;

		public PyxelOutlineDataProvider(PyxelImporter importer)
		{
			this.importer = importer;
		}
		public List<Vector2[]> GetOutlines(GUID guid)
		{
			foreach (var data in importer.SpriteImportData)
			{
				if (data.spriteID == guid.ToString())
				{
					return data.outline;
				}
			}

			return new List<Vector2[]>();
		}

		public float GetTessellationDetail(GUID guid)
		{
			for (int i = 0; i < importer.SpriteImportData.Length; i++)
			{
				if (importer.SpriteImportData[i].spriteID == guid.ToString())
				{
					return importer.SpriteImportData[i].tessellationDetail;
				}
			}

			return 0f;
		}

		public void SetOutlines(GUID guid, List<Vector2[]> data)
		{
			for (int i = 0; i < importer.SpriteImportData.Length; i++)
			{
				if (importer.SpriteImportData[i].spriteID == guid.ToString())
				{
					importer.SpriteImportData[i].outline = data;
				}
			}
		}

		public void SetTessellationDetail(GUID guid, float value)
		{
			for (int i = 0; i < importer.SpriteImportData.Length; i++)
			{
				if (importer.SpriteImportData[i].spriteID == guid.ToString())
				{
					importer.SpriteImportData[i].tessellationDetail = value;
				}
			}
		}
	}

	[Serializable]
	public class PyxelSpriteImportData
	{
		public string name;

		//     Position and size of the Sprite in a given texture.
		public Rect rect;

		//     Pivot value represented by SpriteAlignment.
		public SpriteAlignment alignment;

		//     Pivot value represented in Vector2.
		public Vector2 pivot;

		//     Border value for the generated Sprite.
		public Vector4 border;

		//     Sprite Asset creation uses this outline when it generates the Mesh for the Sprite.
		//     If this is not given, SpriteImportData.tesselationDetail will be used to determine
		//     the mesh detail.
		public List<Vector2[]> outline;

		//     Controls mesh generation detail. This value will be ignored if SpriteImportData.ouline
		//     is provided.
		public float tessellationDetail;

		//     An identifier given to a Sprite. Use this to identify which data was used to
		//     generate that Sprite.
		public string spriteID;


		public SpriteImportData ToSpriteImportData()
		{
			return new SpriteImportData()
			{
				alignment = alignment,
				border = border,
				name = name,
				outline = outline,
				pivot = pivot,
				rect = rect,
				spriteID = spriteID,
				tessellationDetail = tessellationDetail
			};
		}
	}
}
