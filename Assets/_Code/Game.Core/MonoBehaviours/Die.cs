using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core
{
	public class Die : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] private Image _spriteRenderer;
		[SerializeField] private RectTransform _rectTransform;

		[HideInInspector] public DieTypes DieType;

		private Bag _bag;
		private Canvas _canvas;
		private bool _hitBag;

		private void Awake()
		{
			_bag = GameObject.FindObjectOfType<Bag>();
			_canvas = _rectTransform.GetComponentInParent<Canvas>();
		}

		public void Init(DieTypes type)
		{
			DieType = type;
			_spriteRenderer.sprite = Globals.Config.DieSprites[DieType];
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnBeginDrag " + eventData);

			AudioHelpers.PlayOneShotRandom(Globals.Config.SoundDiceDrag, transform.position);
		}

		public void OnDrag(PointerEventData eventData)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, Input.mousePosition, _canvas.worldCamera, out var pos);
			transform.position = _canvas.transform.TransformPoint(pos);

			var pointerData = new PointerEventData(EventSystem.current) { position = eventData.position };
			var results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerData, results);
			_hitBag = false;
			foreach (var r in results)
			{
				if (r.gameObject.layer == LayerMask.NameToLayer("Bag") && r.gameObject.GetComponentInParent<Bag>() == _bag)
				{
					_hitBag = true;
					break;
				}
			}

			if (_hitBag)
				_bag.Highlight();
			else
				_bag.Dehighlight();
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnEndDrag " + eventData);

			AudioHelpers.PlayOneShotRandom(Globals.Config.SoundDiceDrop, transform.position);

			if (_hitBag)
			{
				_bag.Dehighlight();
				_bag.AddDie(this);
			}
			else
			{
				_bag.RemoveDie(this);
				GameObject.Destroy(gameObject);
			}
		}
	}
}
