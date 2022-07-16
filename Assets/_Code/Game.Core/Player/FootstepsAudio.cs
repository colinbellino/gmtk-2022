using UnityEngine;
using Game.Core;

public class FootstepsAudio : MonoBehaviour
{
	private bool isMoving;
	[SerializeField] private float footStepSpeed;
	private Vector2 rawMovementInput;
	private PlayerController playerController;

	private void Awake()
	{
		playerController = GetComponent<PlayerController>();
		InvokeRepeating("CallFootsteps", 0, footStepSpeed);
	}

	private void Update()
	{
		rawMovementInput = Globals.Controls.Gameplay.Move.ReadValue<Vector2>();
		if (rawMovementInput != Vector2.zero)
		{
			isMoving = true;
		}
		else
		{
			isMoving = false;
		}
	}

	private void OnDisable()
	{
		isMoving = false;
	}

	private void CallFootsteps()
	{
		if (isMoving && !playerController.getIsDashing())
		{
			AudioHelpers.PlayOneShot(Globals.Config.PlayerFootsteps, transform.position);
		}
	}
}
