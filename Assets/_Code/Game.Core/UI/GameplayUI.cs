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
		[SerializeField] private Image _dropImage;
		private Dictionary<int, int> _reqIndexToTransformIndex;
		private float _progressWidth;
		private float _margin = 4;

		public bool IsOpened => _root.activeSelf;

		public async UniTask Init()
		{
			_reqIndexToTransformIndex = new Dictionary<int, int>();
			var progressImage = _requests[0].Find("Progress").GetComponent<Image>();
			_progressWidth = progressImage.rectTransform.rect.width;
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
				progressImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _progressWidth - progress * _progressWidth);
				ColorUtility.TryParseHtmlString("#d77643", out var color);
				if (req.FromDM)
					ColorUtility.TryParseHtmlString("#8e4c90", out color);

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

		public async UniTask AddRequest(int reqIndex, DiceRequest req)
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
			var posX = 0;
			var posY = -(_reqIndexToTransformIndex.Count - 1) * (_requests[transformIndex].rect.height + _margin);
			await _requests[transformIndex].DOLocalMove(new Vector2(-_requests[transformIndex].rect.width, posY), 0.02f);
			_requests[transformIndex].gameObject.SetActive(true);
			await _requests[transformIndex].DOLocalMove(new Vector2(posX, posY), 0.3f);
		}

		public async UniTask RemoveRequest(int reqIndex, DiceRequest req)
		{
			var transformIndex = _reqIndexToTransformIndex[reqIndex];

			await _requests[transformIndex].DOLocalMoveX(-_requests[transformIndex].rect.width, 0.2f);
			_requests[transformIndex].gameObject.SetActive(false);

			_reqIndexToTransformIndex.Remove(reqIndex);

			var i = 0;
			foreach (var r in _requests)
			{
				if (r.gameObject.activeSelf)
				{
					await r.DOLocalMoveY(-i * (r.rect.height + _margin), 0.2f);
					i += 1;
				}
			}
		}

		public UniTask Hide(float duration = 0.5f)
		{
			_root.SetActive(false);

			return default;
		}
	}
}
