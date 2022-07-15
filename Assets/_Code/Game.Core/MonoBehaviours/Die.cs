using Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class Die : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	private Bag _bag;

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
		var worldPosition = GameManager.Game.CameraRig.Camera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, GameManager.Game.CameraRig.Camera.nearClipPlane));
		transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.localPosition.z);
		// UnityEngine.Debug.Log(name + " OnDrag " + eventData);

		var hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, LayerMask.GetMask("Bag"));
		if (hit)
		{
			_bag = hit.transform.GetComponent<Bag>();
			_bag.Highlight();
		}
		else
		{
			if (_bag != null)
			{
				_bag.Dehighlight();
			}
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (_bag != null)
		{
			_bag.Dehighlight();
			UnityEngine.Debug.Log(name + " => " + _bag.name);
		}

		// RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, LayerMask.GetMask("Bag"));

		// // UnityEngine.Debug.Log(name + " OnEndDrag " + eventData);
		// if (hit)
		// {
		// 	UnityEngine.Debug.Log(name + " => " + hit.transform.name);
		// }
	}
}
