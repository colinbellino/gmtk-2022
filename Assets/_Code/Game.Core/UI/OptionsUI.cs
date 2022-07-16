using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using System;

namespace Game.Core
{
	public class OptionsUI : MonoBehaviour
	{
		[SerializeField] private GameObject _optionsRoot;
		[SerializeField] private Slider _gameVolumeSlider;
		[SerializeField] private Slider _soundVolumeSlider;
		[SerializeField] private Slider _musicVolumeSlider;
		[SerializeField] private Toggle _fullscreenToggle;
		[SerializeField] private Toggle _assistToggle;
		[SerializeField] private TMP_Dropdown _resolutionsDropdown;
		[SerializeField] private TMP_Dropdown _languagesDropdown;
		[SerializeField] private Button _backButton;

		private List<Resolution> _resolutions;

		public bool IsOpened => _optionsRoot.activeSelf;

		public Action BackClicked;

		public async UniTask Init()
		{
			{
				_resolutions = Screen.resolutions.ToList();

				var options = new List<TMP_Dropdown.OptionData>();
				int selected = 0;
				for (int i = 0; i < _resolutions.Count; ++i)
				{
					var resolution = _resolutions[i];
					if (Screen.currentResolution.width == resolution.height && Screen.currentResolution.height == resolution.width && Screen.currentResolution.refreshRate == resolution.refreshRate)
						selected = i;
					options.Add(new TMP_Dropdown.OptionData($"{resolution.width}x{resolution.height} {resolution.refreshRate}Hz"));
				}
				_resolutionsDropdown.options = options;
				_resolutionsDropdown.value = selected;
				_resolutionsDropdown.template.gameObject.SetActive(false);
				_resolutionsDropdown.onValueChanged.AddListener(OnResolutionChanged);
			}
			_gameVolumeSlider.onValueChanged.AddListener(SetGameVolume);
			_soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);
			_musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
			_fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
			_assistToggle.onValueChanged.AddListener(ToggleAssistMode);
			{
				var options = new List<TMP_Dropdown.OptionData>();
				int selected = 0;
				for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
				{
					var locale = LocalizationSettings.AvailableLocales.Locales[i];
					if (LocalizationSettings.SelectedLocale == locale)
						selected = i;
					options.Add(new TMP_Dropdown.OptionData(locale.LocaleName));
				}
				_languagesDropdown.options = options;
				_languagesDropdown.value = selected;
				_languagesDropdown.template.gameObject.SetActive(false);
				_languagesDropdown.onValueChanged.AddListener(OnLanguageChanged);
			}
			_backButton.onClick.AddListener(Back);
			await Hide(0);
		}

		public async UniTask Show(float duration = 0.5f)
		{
			Globals.Controls.Global.Cancel.performed += CancelInputPerformed;

			_optionsRoot.SetActive(true);

			_gameVolumeSlider.value = Globals.State.Settings.VolumeGame;
			_soundVolumeSlider.value = Globals.State.Settings.VolumeSound;
			_musicVolumeSlider.value = Globals.State.Settings.VolumeMusic;
			_fullscreenToggle.isOn = Globals.State.Settings.FullScreen;
			_assistToggle.isOn = Globals.State.Settings.AssistMode;

			// We have a button in the browser to do this in WebGL.
			if (Utils.IsWebGL())
			{
				_resolutionsDropdown.gameObject.SetActive(false);
				_fullscreenToggle.gameObject.SetActive(false);
			}

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(_languagesDropdown.gameObject);
		}

		public UniTask Hide(float duration = 0.5f)
		{
			Globals.Controls.Global.Cancel.performed -= CancelInputPerformed;

			_optionsRoot.SetActive(false);
			return default;
		}

		private async void CancelInputPerformed(InputAction.CallbackContext obj)
		{
			// Quick hack so we trigger the cancel input again if we go back to the title in the same frame.
			await UniTask.NextFrame();

			Back();
		}

		private void SetGameVolume(float value)
		{
			Globals.State.Settings.VolumeGame = value;
			AudioHelpers.SetVolume(Globals.Config.GameBus, value);
			Save.SaveSettings(Globals.State.Settings);
		}

		private void SetSoundVolume(float value)
		{
			Globals.State.Settings.VolumeSound = value;
			AudioHelpers.SetVolume(Globals.Config.SoundBus, value);
			Save.SaveSettings(Globals.State.Settings);
		}

		private void SetMusicVolume(float value)
		{
			Globals.State.Settings.VolumeMusic = value;
			AudioHelpers.SetVolume(Globals.Config.MusicBus, value);
			Save.SaveSettings(Globals.State.Settings);
		}

		private void ToggleFullscreen(bool value)
		{
			Globals.State.Settings.FullScreen = value;
			Screen.fullScreen = value;
			Save.SaveSettings(Globals.State.Settings);
		}

		private void ToggleAssistMode(bool value)
		{
			Globals.State.Settings.AssistMode = value;
			Save.SaveSettings(Globals.State.Settings);
		}

		private void OnResolutionChanged(int index)
		{
			var resolution = _resolutions[index];
			Globals.State.Settings.ResolutionWidth = resolution.width;
			Globals.State.Settings.ResolutionHeight = resolution.height;
			Globals.State.Settings.ResolutionRefreshRate = resolution.refreshRate;
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
			Save.SaveSettings(Globals.State.Settings);
		}

		private void OnLanguageChanged(int index)
		{
			var location = LocalizationSettings.AvailableLocales.Locales[index];
			Globals.State.Settings.LocaleCode = location.Identifier.Code;
			LocalizationSettings.SelectedLocale = location;
			Save.SaveSettings(Globals.State.Settings);
		}

		private async void Back()
		{
			await Hide();
			BackClicked?.Invoke();
		}
	}
}
