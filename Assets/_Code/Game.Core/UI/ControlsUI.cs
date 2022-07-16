using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core
{
	public class ControlsUI : MonoBehaviour
	{
		[SerializeField] private GameObject _root;
		[SerializeField] private Button _closeButton;
		[SerializeField] private GameObject _keyboardRoot;
		[SerializeField] private GameObject _xboxRoot;
		[SerializeField] private GameObject _ps5Root;

		public bool IsOpened => _root.activeSelf;

		public async UniTask Init()
		{
			await Hide(0);
		}

		public async UniTask Show(float duration = 0.5f)
		{
			Globals.Controls.Global.Cancel.performed += CancelInputPerformed;
			_closeButton.onClick.AddListener(CloseButtonClick);

			SetInputType(0);
			_root.SetActive(true);

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(_closeButton.gameObject);
		}

		public UniTask Hide(float duration = 0.5f)
		{
			Globals.Controls.Global.Cancel.performed -= CancelInputPerformed;
			_closeButton.onClick.RemoveListener(CloseButtonClick);

			_root.SetActive(false);
			return default;
		}

		public void SetInputType(InputTypes inputType)
		{
			switch (inputType)
			{
				case InputTypes.Keyboard:
					{
						_keyboardRoot.SetActive(true);
						_xboxRoot.SetActive(false);
						_ps5Root.SetActive(false);
					}
					break;
				case InputTypes.XInputController:
					{
						_keyboardRoot.SetActive(false);
						_xboxRoot.SetActive(true);
						_ps5Root.SetActive(false);
					}
					break;
				case InputTypes.DualShockGamepad:
					{
						_keyboardRoot.SetActive(false);
						_xboxRoot.SetActive(false);
						_ps5Root.SetActive(true);
					}
					break;
			}
		}

		private async void CancelInputPerformed(InputAction.CallbackContext obj)
		{
			UnityEngine.Debug.Log("CancelInputPerformed");
		}

		private void CloseButtonClick()
		{
			UnityEngine.Debug.Log("CloseButtonClick");
		}
	}
}
