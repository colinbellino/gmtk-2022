using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PyxelEdit
{
	[Serializable]
	public class PyxelDocData
	{
		public string version;
		public string name;
		public Canvas canvas;
		public Tileset tileset;

		[Serializable]
		public struct Canvas
		{
			public int width;
			public int height;
			public int tileHeight;
			public int tileWidth;
			public int currentLayerIndex;
			public int numLayers;
			[JsonConverter(typeof(ArrayMapConverter<Layer>))]
			public Layer[] layers;
		}

		[Serializable]
		public struct Layer
		{
			public string name;
			public string blendMode;
			public bool muted;
			public bool hidden;
			public bool collapsed;
			public bool soloed;
			public int alpha;
			[JsonConverter(typeof(ArrayMapConverter<TileRef>))]
			public TileRef[] tileRefs;
			public string type;
			public int parentIndex;
		}

		[Serializable]
		public struct TileRef
		{
			public int index;
			public bool flipX;
			public int rot;
		}

		[Serializable]
		public struct Tileset
		{
			public int tileHeight;
			public bool fixedWidth;
			public int tilesWide;
			public bool fixedHeight;
			public int numTiles;
			public int tileWidth;
		}
	}

	public class ArrayMapConverter<T> : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var t = JToken.FromObject(value);

			if (t.Type != JTokenType.Array)
			{
				t.WriteTo(writer);
			}
			else
			{
				var array = (JArray)t;
				var keys = array.Select((p, i) => i).ToList();

				var o = new JObject();
				foreach (var key in keys)
				{
					o.Add(new JProperty(key.ToString(), array[key]));
				}

				o.WriteTo(writer);
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return string.Empty;
			}
			else if (reader.TokenType == JsonToken.String)
			{
				return serializer.Deserialize(reader, objectType);
			}
			else
			{
				var obj = JObject.Load(reader);
				var array = new T[obj.Count];
				foreach (var key in obj.Properties())
				{
					array[int.Parse(key.Name)] = obj[key.Name].ToObject<T>(serializer);
				}
				return array;
			}
		}

		public override bool CanConvert(Type objectType)
		{
			return true;
		}
	}
}
