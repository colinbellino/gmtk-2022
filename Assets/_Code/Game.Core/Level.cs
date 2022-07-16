using System;
using UnityEngine;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game/Level")]
	public class Level : ScriptableObject
	{
		public DiceRequest[] Requests;
	}
}
