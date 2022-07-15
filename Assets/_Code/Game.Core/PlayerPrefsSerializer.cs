using UnityEngine;

namespace Game.Core
{
	public static class PlayerPrefsSerializer
	{
		public static void Serialize<T>(T data, string key)
		{
			var dataString = JsonUtility.ToJson(data);
			UnityEngine.Debug.Log(key + ": " + dataString);
			PlayerPrefs.SetString(key, dataString);
			PlayerPrefs.Save();
		}

		public static bool Deserialize<T>(string key, ref T data)
		{
			try
			{
				var dataString = PlayerPrefs.GetString(key);
				if (string.IsNullOrEmpty(dataString))
					return false;

				UnityEngine.Debug.Log(key + ": " + dataString);
				data = JsonUtility.FromJson<T>(dataString);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
