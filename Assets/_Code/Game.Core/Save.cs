using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Game.Core
{
	public static class Save
	{
		private static string _playerSettingsPath = Application.persistentDataPath + "/Settings.bin";
		private static string _playerSettingsKey = "PlayerSettings";
		private static string _playerSaveDataPath = Application.persistentDataPath + "/Save0.bin";
		private static string _playerDataKey = "PlayerSave0";

		public static PlayerSettings LoadPlayerSettings()
		{
			var data = new PlayerSettings
			{
				GameVolume = 1,
				SoundVolume = 1,
				MusicVolume = 1,
				FullScreen = Screen.fullScreen,
				ResolutionWidth = Screen.currentResolution.width,
				ResolutionHeight = Screen.currentResolution.height,
				ResolutionRefreshRate = Screen.currentResolution.refreshRate,
				LocaleCode = LocalizationSettings.SelectedLocale.Identifier.Code,
				Screenshake = true,
			};

			if (Utils.IsWebGL())
			{
				UnityEngine.Debug.Log("Loading player settings: " + _playerSettingsKey);
				if (PlayerPrefsSerializer.Deserialize(_playerSettingsKey, ref data) == false)
					UnityEngine.Debug.LogWarning("Couldn't load player settings.");
			}
			else
			{
				UnityEngine.Debug.Log("Loading player settings: " + _playerSettingsPath);
				if (BinaryFileSerializer.Deserialize(_playerSettingsPath, ref data) == false)
					UnityEngine.Debug.LogWarning("Couldn't load player settings.");
			}

			return data;
		}

		public static void SavePlayerSettings(PlayerSettings data)
		{
			if (Utils.IsWebGL())
			{
				PlayerPrefsSerializer.Serialize(data, _playerSettingsKey);
				UnityEngine.Debug.Log("Saving player settings: " + _playerSettingsKey);
			}
			else
			{
				BinaryFileSerializer.Serialize(data, _playerSettingsPath);
				UnityEngine.Debug.Log("Saving player settings: " + _playerSettingsPath);
			}
		}

		public static PlayerSaveData LoadPlayerSaveData()
		{
			var data = new PlayerSaveData
			{
				ClearedLevels = new HashSet<int>(),
			};

			if (Utils.IsWebGL())
			{
				UnityEngine.Debug.Log("Loading player data: " + _playerDataKey);
				if (PlayerPrefsSerializer.Deserialize(_playerDataKey, ref data) == false)
					UnityEngine.Debug.LogWarning("Couldn't load player data.");
			}
			else
			{
				if (BinaryFileSerializer.Deserialize(_playerSaveDataPath, ref data) == false)
					UnityEngine.Debug.LogWarning("Couldn't load player data.");
			}

			return data;
		}

		public static void SavePlayerSaveData(PlayerSaveData data)
		{
			if (Utils.IsWebGL())
			{
				PlayerPrefsSerializer.Serialize(data, _playerDataKey);
				UnityEngine.Debug.Log("Saving player data: " + _playerDataKey);
			}
			else
			{
				BinaryFileSerializer.Serialize(data, _playerSaveDataPath);
				UnityEngine.Debug.Log("Saving player data: " + _playerSaveDataPath);
			}
		}
	}
}
