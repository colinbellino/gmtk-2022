using Unity.Mathematics;
using UnityEngine;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game/Level")]
	public class Level : ScriptableObject
	{
		public int Timer;
		public DiceRequest[] Requests;

		[ContextMenu("Calculate Timer")]
		private void CalculateTimer()
		{
			var timer = 0.0f;
			foreach (var req in Requests)
			{
				timer += req.Offset + Utils.GetDuration(req);
			}

			UnityEngine.Debug.Log("Timer: " + timer);
			Timer = (int)math.ceil(timer);
		}
	}

}
