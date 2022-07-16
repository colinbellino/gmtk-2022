using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Core
{
	public class BonusGenerator : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		private DieBonus _drag;

		public void OnPointerDown(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnPointerDown " + eventData);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			UnityEngine.Debug.Log(name + " OnBeginDrag " + eventData);
			_drag = GameObject.Instantiate(Resources.Load<DieBonus>("DieBonus"), transform.position, Quaternion.identity);
			_drag.Init();
			_drag.OnBeginDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnDrag " + eventData);
			if (_drag != null)
				_drag.OnDrag(eventData);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnEndDrag " + eventData);
			if (_drag != null)
			{
				_drag.OnEndDrag(eventData);
				_drag = null;
			}
		}
	}
}
