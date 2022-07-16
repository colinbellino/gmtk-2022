using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Core
{
	public class DieGenerator : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] private DieTypes DieType;

		private Die _dragDie;

		public void OnPointerDown(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnPointerDown " + eventData);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnBeginDrag " + eventData);
			_dragDie = GameObject.Instantiate(Resources.Load<Die>("Die"), transform.position, Quaternion.identity);
			_dragDie.Init(DieType);
			_dragDie.OnBeginDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnDrag " + eventData);
			if (_dragDie != null)
				_dragDie.OnDrag(eventData);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnEndDrag " + eventData);
			if (_dragDie != null)
			{
				_dragDie.OnEndDrag(eventData);
				_dragDie = null;
			}
		}
	}
}
