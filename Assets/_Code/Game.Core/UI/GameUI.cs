using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core
{
	public class GameUI : MonoBehaviour
	{
		[Header("Debug")]
		[SerializeField] private GameObject _debugRoot;
		[SerializeField] private TMP_Text _debugText;
		[SerializeField] private TMP_Text _versionText;
		[Header("Title")]
		[SerializeField] private GameObject _titleRoot;
		[SerializeField] private RectTransform _titleWrapper;
		[SerializeField] public Button StartButton;
		[SerializeField] public Button OptionsButton;
		[SerializeField] public Button CreditsButton;
		[SerializeField] public Button QuitButton;
		[Header("Level Selection")]
		[SerializeField] public GameObject _levelSelectionRoot;
		[SerializeField] public LevelButton[] LevelButtons;
		[Header("Credits")]
		[SerializeField] public GameObject _creditsRoot;
		[Header("Intro")]
		[SerializeField] public GameObject _introRoot;
		[Header("Ending")]
		[SerializeField] public GameObject _endingRoot;
		[Header("Transitions")]
		[SerializeField] private GameObject _fadeRoot;
		[SerializeField] private Image _fadeToBlackImage;

		private TweenerCore<Color, Color, ColorOptions> _fadeTweener;

		public async UniTask Init()
		{
			HideDebug();
			await HideCredits(0);
			await HideTitle(0);
			await HideLevelSelection(0);
		}

		public void ShowDebug() { _debugRoot.SetActive(true); }
		public void HideDebug() { _debugRoot.SetActive(false); }
		public void SetDebugText(string value)
		{
			_debugText.text = value;
		}
		public void AddDebugLine(string value)
		{
			_debugText.text += value + "\n";
		}
		public void SetVersion(string value)
		{
			_versionText.text = value;
		}

		public async UniTask ShowTitle(float duration = 0.5f)
		{
			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(StartButton.gameObject);

			_titleRoot.SetActive(true);
			// await _titleWrapper.DOLocalMoveY(0, duration / GameManager.Game.State.TimeScaleCurrent);
		}
		public async UniTask HideTitle(float duration = 0.5f)
		{
			// await _titleWrapper.DOLocalMoveY(156, duration / GameManager.Game.State.TimeScaleCurrent);
			_titleRoot.SetActive(false);
		}
		public void SelectTitleOptionsGameObject()
		{
			EventSystem.current.SetSelectedGameObject(OptionsButton.gameObject);
		}

		public async UniTask ShowLevelName(string title, float duration = 0.5f)
		{
			await UniTask.NextFrame();
			// _levelNameRoot.SetActive(true);
			// _levelNameText.text = title;
			// await _levelNameText.rectTransform.DOLocalMoveY(-80, duration / GameManager.Game.State.TimeScaleCurrent);
		}
		public async UniTask HideLevelName(float duration = 0.25f)
		{
			await UniTask.NextFrame();
			// await _levelNameText.rectTransform.DOLocalMoveY(-130, duration / GameManager.Game.State.TimeScaleCurrent);
			// _levelNameRoot.SetActive(false);
		}

		public async UniTask ShowLevelSelection(float duration = 0.5f)
		{
			_levelSelectionRoot.SetActive(true);

			for (int levelIndex = 0; levelIndex < LevelButtons.Length; levelIndex++)
			{
				var button = LevelButtons[levelIndex];

				if (levelIndex < GameManager.Game.State.AllLevels.Length)
				{
					if (GameManager.Game.Config.DebugLevels || levelIndex == 0 || GameManager.Game.State.PlayerSaveData.ClearedLevels.Contains(levelIndex - 1))
					{
						var level = GameManager.Game.State.AllLevels[levelIndex];
						button.Button.interactable = true;
						button.Text.text = "Level " + levelIndex;
						// button.Text.text = string.IsNullOrEmpty(level.Title) ? level.name : level.Title;
						// button.Thumbnail.texture = level.Screenshot;
						button.Thumbnail.gameObject.SetActive(true);
					}
					else
					{
						button.Button.interactable = false;
						button.Text.text = "???";
						button.Thumbnail.gameObject.SetActive(false);
					}
				}
				else
				{
					button.Button.interactable = false;
					button.gameObject.SetActive(false);
				}
			}

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			var nextLevelIndex = 0;
			if (GameManager.Game.State.PlayerSaveData.ClearedLevels.Count > 0)
				nextLevelIndex = Math.Min(GameManager.Game.State.PlayerSaveData.ClearedLevels.LastOrDefault() + 1, GameManager.Game.State.AllLevels.Length - 1);
			EventSystem.current.SetSelectedGameObject(LevelButtons[nextLevelIndex].gameObject);
		}
		public UniTask HideLevelSelection(float duration = 0.5f)
		{
			_levelSelectionRoot.SetActive(false);
			return default;
		}
		public UniTask ToggleLevelSelection(float duration = 0.5f)
		{
			if (_levelSelectionRoot.activeSelf)
			{
				return HideLevelSelection(duration);
			}
			return ShowLevelSelection(duration);
		}

		public UniTask ShowCredits(float duration = 0.5f)
		{
			_creditsRoot.SetActive(true);
			return default;
		}
		public UniTask HideCredits(float duration = 0.5f)
		{
			_creditsRoot.SetActive(false);
			return default;
		}

		public UniTask ShowIntro(float duration = 0.5f)
		{
			_introRoot.SetActive(true);
			return default;
		}
		public UniTask HideIntro(float duration = 0.5f)
		{
			_introRoot.SetActive(false);
			return default;
		}

		public UniTask ShowEnding(float duration = 0.5f)
		{
			_endingRoot.SetActive(true);
			return default;
		}
		public UniTask HideEnding(float duration = 0.5f)
		{
			_endingRoot.SetActive(false);
			return default;
		}

		public async UniTask FadeIn(Color color, float duration = 1f)
		{
			_fadeRoot.SetActive(true);
			if (_fadeTweener != null)
			{
				_fadeToBlackImage.color = _fadeTweener.endValue;
				_fadeTweener.Kill(true);
			}
			_fadeTweener = _fadeToBlackImage.DOColor(color, duration / GameManager.Game.State.TimeScaleCurrent);
			await _fadeTweener;
		}

		private async UniTask FadeInPanel(Image panel, TMP_Text text, float duration)
		{
			panel.gameObject.SetActive(true);

			foreach (var t in panel.GetComponentsInChildren<TMP_Text>())
			{
				_ = t.DOFade(1f, 0f);
			}

			_ = panel.DOFade(1f, duration);

			text.maxVisibleCharacters = 0;

			await UniTask.Delay(TimeSpan.FromSeconds(duration / GameManager.Game.State.TimeScaleCurrent));

			var totalInvisibleCharacters = text.textInfo.characterCount;
			var counter = 0;
			while (true)
			{
				var visibleCount = counter % (totalInvisibleCharacters + 1);
				text.maxVisibleCharacters = visibleCount;

				if (visibleCount >= totalInvisibleCharacters)
				{
					break;
				}

				counter += 1;

				await UniTask.Delay(TimeSpan.FromMilliseconds(10 / GameManager.Game.State.TimeScaleCurrent));
			}

			var buttons = panel.GetComponentsInChildren<Button>();
			for (int i = 0; i < buttons.Length; i++)
			{
				_ = buttons[i].image.DOFade(1f, duration / GameManager.Game.State.TimeScaleCurrent);
			}
		}

		private async UniTask FadeOutPanel(Image panel, float duration)
		{
			_ = panel.DOFade(0f, duration / GameManager.Game.State.TimeScaleCurrent);

			foreach (var graphic in panel.GetComponentsInChildren<Graphic>())
			{
				_ = graphic.DOFade(0f, duration / GameManager.Game.State.TimeScaleCurrent);
			}

			await UniTask.Delay(TimeSpan.FromSeconds(duration / GameManager.Game.State.TimeScaleCurrent));
			panel.gameObject.SetActive(false);
		}
	}
}
