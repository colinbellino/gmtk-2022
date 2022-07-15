using System;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core
{
	public class PauseUI : MonoBehaviour
	{
		[SerializeField] private GameObject _pauseRoot;
		[SerializeField] private Button _levelSelectButton;
		[SerializeField] private Button _optionsButton;
		[SerializeField] private Button _backButton;
		[SerializeField] private Button _quitButton;

		public bool IsOpened => _pauseRoot.activeSelf;
		public Action BackClicked;

		public async UniTask Init()
		{
			_levelSelectButton.onClick.AddListener(OpenLevelSelect);
			_optionsButton.onClick.AddListener(OpenOptions);
			_backButton.onClick.AddListener(BackToGame);
			_quitButton.onClick.AddListener(QuitGame);
			GameManager.Game.OptionsUI.BackClicked += OnOptionsBackClicked;

			await Hide();
		}

		public async UniTask Show(float duration = 0.5f)
		{
			_pauseRoot.SetActive(true);

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(_levelSelectButton.gameObject);
		}

		public UniTask Hide(float duration = 0.5f)
		{
			_pauseRoot.SetActive(false);
			return default;
		}

		public void SelectOptionsGameObject()
		{
			EventSystem.current.SetSelectedGameObject(_optionsButton.gameObject);
		}

		private void OpenLevelSelect() => GameManager.Game.GameFSM.Fire(StateMachines.Game.GameFSM.Triggers.LevelSelectionRequested);

		private void OpenOptions() => _ = GameManager.Game.OptionsUI.Show();

		private void QuitGame()
		{
			GameManager.Game.State.LevelMusic.stop(STOP_MODE.ALLOWFADEOUT);
			GameManager.Game.GameFSM.Fire(StateMachines.Game.GameFSM.Triggers.Quit);
		}

		private void BackToGame()
		{
			BackClicked?.Invoke();
		}

		private void OnOptionsBackClicked()
		{
			EventSystem.current.SetSelectedGameObject(_optionsButton.gameObject);
		}
	}
}
