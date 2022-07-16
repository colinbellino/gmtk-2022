using UnityEngine;

namespace Game.Core
{
	public static class Save
	{
		private static readonly string SETTINGS_KEY = "Settings";
		private static readonly string SAVE_KEY = "Save0";

		private static readonly string SETTINGS_PATH = Application.persistentDataPath + "/Settings.bin";
		private static readonly string SAVE_PATH = Application.persistentDataPath + "/Save0.bin";

		public static bool LoadSettings(ref GameSettings settings)
		{
			if (Utils.IsWebGL())
			{
				Debug.Log("[Save] Loading player settings (WebGL): " + SETTINGS_KEY);
				return PlayerPrefsSerializer.Deserialize(SETTINGS_KEY, ref settings);
			}

			Debug.Log("[Save] Loading player settings (Binary): " + SETTINGS_PATH);
			return BinaryFileSerializer.Deserialize(SETTINGS_PATH, ref settings);
		}

		public static bool SaveSettings(GameSettings settings)
		{
			if (Utils.IsWebGL())
			{
				Debug.Log("[Save] Saving player settings (WebGL): " + SETTINGS_KEY);
				return PlayerPrefsSerializer.Serialize(settings, SETTINGS_KEY);
			}

			Debug.Log("[Save] Saving player settings (Binary): " + SETTINGS_PATH);
			return BinaryFileSerializer.Serialize(settings, SETTINGS_PATH);
		}

		public static bool LoadGame(ref GameSave save)
		{
			if (Utils.IsWebGL())
			{
				Debug.Log("[Save] Loading player data (WebGL): " + SAVE_KEY);
				return PlayerPrefsSerializer.Deserialize(SAVE_KEY, ref save);
			}

			Debug.Log("[Save] Loading player data (Binary): " + SAVE_PATH);
			return BinaryFileSerializer.Deserialize(SAVE_PATH, ref save);
		}

		public static bool SaveGame(GameSave save)
		{
			if (Utils.IsWebGL())
			{
				Debug.Log("[Save] Saving player data (WebGL): " + SAVE_KEY);
				return PlayerPrefsSerializer.Serialize(save, SAVE_KEY);
			}

			Debug.Log("[Save] Saving player data (Binary): " + SAVE_PATH);
			return BinaryFileSerializer.Serialize(save, SAVE_PATH);
		}
	}
}
