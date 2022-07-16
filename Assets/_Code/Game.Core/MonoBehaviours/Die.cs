using Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class Die : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField] private DieTypes DieType;
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

			if (GameManager.Game.State.Bag == null)
			{
				GameManager.Game.State.Bag = new DiceBag();
			}

			if (GameManager.Game.State.Bag.Die == DieType)
			{
				GameManager.Game.State.Bag.Quantity += 1;
			}
			else
			{
				GameManager.Game.State.Bag.Die = DieType;
				GameManager.Game.State.Bag.Quantity = 1;
			}

			SubmitBag();
		}

		// RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, LayerMask.GetMask("Bag"));

		// // UnityEngine.Debug.Log(name + " OnEndDrag " + eventData);
		// if (hit)
		// {
		// 	UnityEngine.Debug.Log(name + " => " + hit.transform.name);
		// }
	}

	private void SubmitBag()
	{
		var bag = GameManager.Game.State.Bag;
		var activeIds = GameManager.Game.State.ActiveRequests;
		UnityEngine.Debug.Log("Bag: " + Utils.DiceRequestToString(bag));

		var matchIndex = activeIds.FindIndex(reqIndex =>
		{
			var req = GameManager.Game.State.Requests[reqIndex];
			return req.Die == bag.Die && req.Quantity == bag.Quantity && req.Bonus == bag.Bonus;
		});
		if (matchIndex == -1)
		{
			UnityEngine.Debug.Log("no requests matched");
		}
		else
		{
			var req = GameManager.Game.State.Requests[GameManager.Game.State.ActiveRequests[matchIndex]];
			UnityEngine.Debug.Log("matched request: " + Utils.DiceRequestToString(req) + " (" + req.Id + ")");
			GameManager.Game.State.ActiveRequests.RemoveAt(matchIndex);
			GameManager.Game.State.CompletedRequests.Add(matchIndex);
		}
	}
}
