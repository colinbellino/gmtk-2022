using UnityEngine;

namespace Game.Core
{
	public class Entity : MonoBehaviour
	{
		[SerializeField] public Animator Animator;
		[SerializeField] public SpriteRenderer SpriteRenderer;

		[HideInInspector] public Vector3 SpawnPosition;
		[HideInInspector] public bool Ready;

		private void OnEnable()
		{

		}

		private void OnDisable()
		{
			if (Ready)
			{
				transform.localPosition = SpawnPosition;
			}
		}
	}
}
