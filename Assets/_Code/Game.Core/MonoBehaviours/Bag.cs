using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
	public class Bag : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer _spriteRenderer;

		private Color _defaultColor;
		private List<Die> _dice = new List<Die>();
		private List<DieBonus> _bonuses = new List<DieBonus>();

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
		}

		public void RemoveDie(Die die)
		{
			if (_dice.Contains(die) == false)
				return;

			// UnityEngine.Debug.Log(die + " <= " + name);
			_dice.Remove(die);
		}

		public void AddBonus(DieBonus bonus)
		{
			if (_bonuses.Contains(bonus))
				return;

			// UnityEngine.Debug.Log(bonus + " => " + name);
			_bonuses.Add(bonus);
		}

		public void RemoveBonus(DieBonus bonus)
		{
			if (_bonuses.Contains(bonus) == false)
				return;

			// UnityEngine.Debug.Log(bonus + " <= " + name);
			_bonuses.Remove(bonus);
		}

		public void SubmitBag()
		{
			var bag = new DiceBag();

			foreach (var die in _dice)
			{
				if (bag.Die > 0 && bag.Die != die.DieType)
				{
					Empty();
					return;
				}

				bag.Die = die.DieType;
				bag.Quantity += 1;
			}

			bag.Bonus = _bonuses.Count;

			var activeIds = GameManager.Game.State.ActiveRequests;

			var matchIndex = activeIds.FindIndex(reqIndex =>
			{
				var req = GameManager.Game.State.Requests[reqIndex];
				return req.Die == bag.Die && req.Quantity == bag.Quantity && req.Bonus == bag.Bonus;
			});
			if (matchIndex == -1)
			{
				UnityEngine.Debug.Log("Bag: " + Utils.DiceRequestToString(bag));
				// UnityEngine.Debug.Log("no requests matched");
			}
			else
			{
				var req = GameManager.Game.State.Requests[GameManager.Game.State.ActiveRequests[matchIndex]];
				GameManager.Game.State.ActiveRequests.RemoveAt(matchIndex);
				GameManager.Game.State.CompletedRequests.Add(matchIndex);
				UnityEngine.Debug.Log("matched request: " + Utils.DiceRequestToString(req) + " (" + req.Id + ")");
			}

			Empty();
		}

		private void Empty()
		{
			foreach (var die in _dice)
				GameObject.Destroy(die.gameObject);
			_dice.Clear();

			foreach (var bonus in _bonuses)
				GameObject.Destroy(bonus.gameObject);
			_bonuses.Clear();
		}
	}
}
