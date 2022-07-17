using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Core
{
	public class DieModifierGenerator : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		private DieModifier _drag;

		public void OnPointerDown(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnPointerDown " + eventData);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			// UnityEngine.Debug.Log(name + " OnBeginDrag " + eventData);
			_drag = GameObject.Instantiate(Resources.Load<DieModifier>("Die Modifier"), GameObject.Find("UI/Gameplay Canvas/Root/Dice").transform);
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
