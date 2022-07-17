using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
	public class Bag : MonoBehaviour
	{
		[SerializeField] private Image _spriteRenderer;

		private Color _defaultColor;
		private List<Die> _dice = new List<Die>();
		private List<DieModifier> _modifiers = new List<DieModifier>();

		private void Awake()
		{
			_defaultColor = _spriteRenderer.color;
		}

		public void Highlight()
		{
			ColorUtility.TryParseHtmlString("#ffe6cc", out var color);
			_spriteRenderer.DOColor(color, 0.2f);
		}

		public void Dehighlight()
		{
			_spriteRenderer.DOColor(_defaultColor, 0.2f);
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

		public void AddModifier(DieModifier modifier)
		{
			if (_modifiers.Contains(modifier))
				return;

			// UnityEngine.Debug.Log(modifier + " => " + name);
			_modifiers.Add(modifier);
		}

		public void RemoveModifier(DieModifier modifier)
		{
			if (_modifiers.Contains(modifier) == false)
				return;

			// UnityEngine.Debug.Log(modifier + " <= " + name);
			_modifiers.Remove(modifier);
		}

		public void SubmitBag()
		{
			var bag = new DiceBag();

			foreach (var die in _dice)
			{
				if (bag.Die > 0 && bag.Die != die.DieType)
				{
					UnityEngine.Debug.Log("Invalid bag: multiple types of dice.");
					Empty();
					return;
				}

				bag.Die = die.DieType;
				bag.Quantity += 1;
			}

			bag.Modifier = _modifiers.Count;

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
				_ = Globals.GameplayUI.RemoveRequests(new List<int> { reqIndex });

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

			foreach (var modifier in _modifiers)
				GameObject.Destroy(modifier.gameObject);
			_modifiers.Clear();
		}
	}
}
