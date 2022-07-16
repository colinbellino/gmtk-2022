using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
	public class Bag : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer _spriteRenderer;

		private Color _defaultColor;
		private List<Die> _dice = new List<Die>();

		private void Awake()
		{
			_defaultColor = _spriteRenderer.color;
		}

		public void Dehighlight()
		{
			_spriteRenderer.color = _defaultColor;
		}

		public void Highlight()
		{
			var color = _defaultColor;
			color.r = 1;
			_spriteRenderer.color = color;
		}

		public void AddDie(Die die)
		{
			if (_dice.Contains(die))
				return;

			// UnityEngine.Debug.Log(die + " => " + name);
			_dice.Add(die);
			// SubmitBag();
		}

		public void RemoveDie(Die die)
		{
			if (_dice.Contains(die) == false)
				return;

			// UnityEngine.Debug.Log(die + " <= " + name);
			_dice.Remove(die);
			// SubmitBag();
		}

		public void SubmitBag()
		{
			UnityEngine.Debug.Log("SubmitBag");
			var bag = new DiceBag();

			foreach (var die in _dice)
			{
				if (bag.Die > 0 && bag.Die != die.DieType)
				{
					DestroyDice();
					return;
				}

				bag.Die = die.DieType;
				bag.Quantity += 1;
			}

			var activeIds = GameManager.Game.State.ActiveRequests;
			UnityEngine.Debug.Log("Bag: " + Utils.DiceRequestToString(bag));

			var matchIndex = activeIds.FindIndex(reqIndex =>
			{
				var req = GameManager.Game.State.Requests[reqIndex];
				return req.Die == bag.Die && req.Quantity == bag.Quantity && req.Bonus == bag.Bonus;
			});
			if (matchIndex == -1)
			{
				// UnityEngine.Debug.Log("no requests matched");
			}
			else
			{
				var req = GameManager.Game.State.Requests[GameManager.Game.State.ActiveRequests[matchIndex]];
				GameManager.Game.State.ActiveRequests.RemoveAt(matchIndex);
				GameManager.Game.State.CompletedRequests.Add(matchIndex);
				UnityEngine.Debug.Log("matched request: " + Utils.DiceRequestToString(req) + " (" + req.Id + ")");

				DestroyDice();
			}
		}

		private void DestroyDice()
		{
			foreach (var die in _dice)
				GameObject.Destroy(die.gameObject);

			_dice.Clear();
		}
	}
}
