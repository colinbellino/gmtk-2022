using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Game.Core
{
	// FIXME: use a custom binary formatter for our needs, not BinaryFormatter...
	public static class BinaryFileSerializer
	{
		public static bool Serialize<T>(T data, string path)
		{
			try
			{
				var formatter = new BinaryFormatter();
				var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
				formatter.Serialize(stream, data);
				stream.Close();
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				return false;
			}
		}

		public static bool Deserialize<T>(string path, ref T data)
		{
			try
			{
				var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
				var formatter = new BinaryFormatter();
				data = (T)formatter.Deserialize(stream);
				stream.Close();
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				return false;
			}
		}
	}
}
