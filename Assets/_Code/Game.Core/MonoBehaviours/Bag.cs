using System.Collections.Generic;
using Unity.Mathematics;
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

			bag.Modifier = _bonuses.Count;

			var activeIds = Globals.State.ActiveRequests;

			var matchIndex = activeIds.FindIndex(reqIndex =>
			{
				var req = Globals.State.Requests[reqIndex];
				return req.Roll.Type == bag.Die && req.Roll.Quantity == bag.Quantity && req.Roll.Modifier == bag.Modifier;
			});
			if (matchIndex == -1)
			{
				UnityEngine.Debug.Log("Invalid bag: " + Utils.DiceRequestToString(bag));
				// UnityEngine.Debug.Log("no requests matched");

				var score = 200;
				Globals.State.Score = math.max(0, Globals.State.Score - score);
			}
			else
			{
				var reqIndex = Globals.State.ActiveRequests[matchIndex];
				var req = Globals.State.Requests[reqIndex];
				Globals.State.ActiveRequests.Remove(reqIndex);
				Globals.State.CompletedRequests.Add(reqIndex);
				Globals.GameplayUI.RemoveRequest(reqIndex, req);

				// Calculate score based on difficulty
				var score = 100;
				if (bag.Modifier != 0)
					score += 100;
				if (bag.Quantity != 1)
					score += 100;

				Globals.State.Score += score;
				UnityEngine.Debug.Log("Matched request: " + Utils.DiceRequestToString(req));
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
