using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Core
{
	public class DieBonus : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] private SpriteRenderer _spriteRenderer;

		private Bag _bag;
		private bool _hitBag;

		private void Awake()
		{
			_bag = GameObject.FindObjectOfType<Bag>();
		}

		public void Init()
		{

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

			var hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, LayerMask.GetMask("Bag"));
			_hitBag = hit.transform != null && hit.transform.root == _bag.transform;

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
				_bag.AddBonus(this);
			}
			else
			{
				_bag.RemoveBonus(this);
			}
		}
	}
}
