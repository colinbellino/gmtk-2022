using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
	public class GameplayUI : MonoBehaviour
	{
		[SerializeField] private GameObject _root;
		[SerializeField] private RectTransform[] _requests;
		[SerializeField] private TMP_Text _timerText;
		[SerializeField] private TMP_Text _scoreText;
		private Dictionary<int, int> _reqIndexToTransformIndex;

		public bool IsOpened => _root.activeSelf;

		public async UniTask Init()
		{
			_reqIndexToTransformIndex = new Dictionary<int, int>();
			await Hide();
		}

		public UniTask Show(float duration = 0.5f)
		{
			_root.SetActive(true);

			foreach (var r in _requests)
				r.gameObject.SetActive(false);

			return default;
		}

		public void UpdateRequest(int reqIndex, DiceRequest req)
		{
			var progress = (Time.time - req.Timestamp) / Utils.GetDuration(req);

			if (Utils.IsDevBuild())
			{
				var color = Color.white;
				if (req.FromDM)
					color = Color.red;
				var text = Utils.DiceRequestToString(req) + " (" + (int)(progress * 100) + "%)";
				Globals.UI.AddDebugLine($"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>");
			}

			var transformIndex = _reqIndexToTransformIndex[reqIndex];

			var r = _requests[transformIndex];
			r.GetComponentInChildren<TMP_Text>().text = Utils.DiceRequestToString(req);
			{
				var progressImage = r.Find("Progress").GetComponent<Image>();
				progressImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, r.rect.width - progress * r.rect.width);
				var color = Color.blue;
				if (req.FromDM)
					color = Color.red;
				progressImage.color = color;
			}
		}

		public void Tick()
		{
			Globals.UI.SetDebugText("");
			Globals.UI.AddDebugLine("Level: " + Globals.State.CurrentLevelIndex);
			Globals.UI.AddDebugLine("Score: " + Globals.State.Score);
			Globals.UI.AddDebugLine("Timer: " + Utils.FormatTimer(Globals.State.Timer - Time.time));
			Globals.UI.AddDebugLine("");
			Globals.UI.AddDebugLine("Requests: " + Globals.State.CompletedRequests.Count + "/" + Globals.State.Requests.Count);
			foreach (var reqIndex in Globals.State.ActiveRequests)
			{
				var req = Globals.State.Requests[reqIndex];
				UpdateRequest(reqIndex, req);
			}

			SetTimer(Utils.FormatTimer(Globals.State.Timer - Time.time));
			SetScore(Globals.State.Score.ToString());
		}

		private void SetTimer(string value)
		{
			_timerText.text = value;
		}

		private void SetScore(string value)
		{
			_scoreText.text = value;
		}

		public async void AddRequest(int reqIndex, DiceRequest req)
		{
			var transformIndex = 0;
			for (int i = 0; i < _requests.Length; i++)
			{
				if (_requests[i].gameObject.activeSelf == false)
				{
					transformIndex = i;
					break;
				}
			}

			_reqIndexToTransformIndex.Add(reqIndex, transformIndex);
			var posY = _requests[transformIndex].rect.position.y;
			var posX = (_reqIndexToTransformIndex.Count - 1) * (_requests[transformIndex].rect.width + 10);
			await _requests[transformIndex].DOLocalMove(new Vector2(posX, 10), 0.002f);
			_requests[transformIndex].gameObject.SetActive(true);
			_ = _requests[transformIndex].DOLocalMove(new Vector2(posX, posY), 0.3f);
		}

		public async void RemoveRequest(int reqIndex, DiceRequest req)
		{
			var transformIndex = _reqIndexToTransformIndex[reqIndex];

			await _requests[transformIndex].DOLocalMoveY(40, 0.2f);
			_requests[transformIndex].gameObject.SetActive(false);

			var i = 0;
			foreach (var r in _requests)
			{
				if (r.gameObject.activeSelf)
				{
					_ = r.DOLocalMoveX(i * (r.rect.width + 10), 0.2f);
					i += 1;
				}
			}

			_reqIndexToTransformIndex.Remove(reqIndex);
		}

		public UniTask Hide(float duration = 0.5f)
		{
			_root.SetActive(false);

			return default;
		}
	}
}
