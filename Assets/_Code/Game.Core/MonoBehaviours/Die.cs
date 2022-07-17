using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Core
{
	public class Die : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] private SpriteRenderer _spriteRenderer;

		[HideInInspector] public DieTypes DieType;

		private Bag _bag;
		private bool _hitBag;

		private void Awake()
		{
			_bag = GameObject.FindObjectOfType<Bag>();
		}

		public void Init(DieTypes type)
		{
			DieType = type;
			_spriteRenderer.sprite = Globals.Config.DieSprites[DieType];
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnPointerDown " + eventData);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnBeginDrag " + eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			var worldPosition = Globals.CameraRig.Camera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, Globals.CameraRig.Camera.nearClipPlane));
			transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.localPosition.z);
			// UnityEngine.Debug.Log(name + " OnDrag " + eventData);

			// UnityEngine.Debug.Log(RaycastUtilities.PointerIsOverUI(eventData.position));
			var pointerData = RaycastUtilities.ScreenPosToPointerData(eventData.position);
			var results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerData, results);
			_hitBag = results.Count > 0 &&
				results[0].gameObject.layer == LayerMask.NameToLayer("Bag") &&
				results[0].gameObject.GetComponentInParent<Bag>() == _bag;

			if (_hitBag)
				_bag.Highlight();
			else
				_bag.Dehighlight();
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnEndDrag " + eventData);

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

	public static class RaycastUtilities
	{
		public static bool PointerIsOverUI(Vector2 screenPos)
		{
			var hitObject = UIRaycast(ScreenPosToPointerData(screenPos));
			return hitObject != null && hitObject.layer == LayerMask.NameToLayer("UI");
		}

		public static GameObject UIRaycast(PointerEventData pointerData)
		{
			var results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerData, results);

			return results.Count < 1 ? null : results[0].gameObject;
		}

		public static PointerEventData ScreenPosToPointerData(Vector2 screenPos)
		   => new PointerEventData(EventSystem.current) { position = screenPos };
	}
}
