using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
	public class GameplayUI : MonoBehaviour
	{
		[SerializeField] private GameObject _root;
		[SerializeField] private TMPro.TMP_Text _healthText;
		[SerializeField] private RectTransform _healthCurrentFill;
		[SerializeField] private RectTransform _healthTempFill;
		[SerializeField] private RectTransform _dashCurrentFill;
		[SerializeField] private RectTransform[] _mapRooms;
		[SerializeField] private GridLayoutGroup _mapGridLayoutGroup;
		[SerializeField] private Color _mapColorDefault = Color.white;
		[SerializeField] private Color _mapColorExplored = Color.white;
		[SerializeField] private Color _mapColorStart = Color.white;
		[SerializeField] private Color _mapColorCurrent = Color.white;

		private float _currentHealthDefaultWidth;
		private float _tempHealthDefaultWidth;
		private float _previousCurrentHealth;
		private float _tempHealth;
		private float _currentDashDefaultWidth;
		private TweenerCore<float, float, FloatOptions> _tempHealthTween;

		public bool IsOpened => _root.activeSelf;

		public async UniTask Init()
		{
			_currentHealthDefaultWidth = _healthCurrentFill.sizeDelta.x;
			_tempHealthDefaultWidth = _healthTempFill.sizeDelta.x;
			_currentDashDefaultWidth = _dashCurrentFill.sizeDelta.x;
			await Hide();
		}

		public UniTask Show(float duration = 0.5f)
		{
			_root.SetActive(true);

			// EventSystem.current.SetSelectedGameObject(null);
			// await UniTask.NextFrame();
			// EventSystem.current.SetSelectedGameObject(_levelSelectButton.gameObject);

			return default;
		}

		public void UpdateRequest(DiceRequest req)
		{
			if (Time.time >= req.Timestamp)
				GameManager.Game.UI.AddDebugLine(Utils.DiceRequestToString(req));
		}

		public void Tick()
		{
			GameManager.Game.UI.SetDebugText("");
			foreach (var req in GameManager.Game.State.Requests)
			{
				UpdateRequest(req);
			}
		}

		public UniTask Hide(float duration = 0.5f)
		{
			_root.SetActive(false);

			return default;
		}

		public void SetHealth(int current, int max)
		{
			// _healthText.text = $"Health: {current}/{max}";

			var currentPercentage = (float)current / max;

			if (current < _previousCurrentHealth)
			{
				var loss = _previousCurrentHealth - current;
				var tempPercentage = (current + loss) / max;
				_healthTempFill.sizeDelta = new Vector2(tempPercentage * _tempHealthDefaultWidth, _healthTempFill.sizeDelta.y);

				_tempHealth = current + loss;

				_tempHealthTween = DOTween.To(() => _tempHealth, x => _tempHealth = x, current, 1f)
					.OnUpdate(() =>
					{
						_healthTempFill.sizeDelta = new Vector2(_tempHealth / max * _tempHealthDefaultWidth, _healthTempFill.sizeDelta.y);
					});
			}

			_healthCurrentFill.sizeDelta = new Vector2(currentPercentage * _currentHealthDefaultWidth, _healthCurrentFill.sizeDelta.y);

			_previousCurrentHealth = current;
		}

		public void SetDash(float value)
		{
			_dashCurrentFill.sizeDelta = new Vector2(value * _currentDashDefaultWidth, _dashCurrentFill.sizeDelta.y);
		}

		public void SetMiniMap(Level level, bool mustReturnToStart = false)
		{
			_mapGridLayoutGroup.constraintCount = level.Width;

			for (int roomIndex = 0; roomIndex < _mapRooms.Length; roomIndex++)
			{
				var roomRect = _mapRooms[roomIndex];

				roomRect.Find("Background").GetComponent<Image>().color = Color.black;
				roomRect.Find("Icon").GetComponent<Image>().color = Color.clear;

				if (roomIndex >= level.Rooms.Count)
				{
					roomRect.gameObject.SetActive(false);
					continue;
				}

				roomRect.gameObject.SetActive(true);
				var room = level.Rooms[roomIndex];
				if (room.Instance == null)
				{
					roomRect.Find("Background").GetComponent<Image>().color = Color.clear;
					roomRect.Find("Color").GetComponent<Image>().color = Color.clear;
				}
				else
				{
					roomRect.Find("Color").GetComponent<Image>().color = _mapColorDefault;

					if (room.Explored)
					{
						roomRect.Find("Color").GetComponent<Image>().color = _mapColorExplored;
					}
					if (room == level.StartRoom)
					{
						roomRect.Find("Icon").GetComponent<Image>().color = _mapColorStart;
						if (mustReturnToStart)
							roomRect.Find("Color").GetComponent<Image>().DOColor(Color.white, 1f).SetLoops(-1, LoopType.Yoyo);
					}
					if (room == level.CurrentRoom)
					{
						roomRect.Find("Color").GetComponent<Image>().color = _mapColorCurrent;
					}
				}
			}
		}
	}
}
