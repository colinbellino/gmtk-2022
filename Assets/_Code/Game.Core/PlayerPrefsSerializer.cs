using System;
using UnityEngine;

namespace Game.Core
{
	public static class PlayerPrefsSerializer
	{
		public static bool Serialize<T>(T data, string key)
		{
			try
			{
				var dataString = JsonUtility.ToJson(data);
				PlayerPrefs.SetString(key, dataString);
				PlayerPrefs.Save();
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				return false;
			}
		}

		public static bool Deserialize<T>(string key, ref T data)
		{
			try
			{
				var dataString = PlayerPrefs.GetString(key);
				if (string.IsNullOrEmpty(dataString))
				{
					Debug.LogError($"No PlayerPrefs data for key: {key}");
					return false;
				}

				data = JsonUtility.FromJson<T>(dataString);
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
