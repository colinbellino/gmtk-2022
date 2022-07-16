using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core;

public class PlayerProjectile : MonoBehaviour
{
	[SerializeField] private GameObject batProjectile;
	[SerializeField] private float shootCooldown;

	private PlayerHealth playerHealth;
	private SpriteRenderer handSR;
	private float shootCounter;
	private Vector2 mousePosition;
	private Vector3 worldMousePosition;
	private Vector3 aimDirection;
	private Vector2 aimInput;

	void Awake()
	{
		playerHealth = GetComponentInParent<PlayerHealth>();
		handSR = GetComponentInChildren<SpriteRenderer>();
	}

	void OnEnable()
	{
		GameManager.Game.Controls.Gameplay.Shoot.performed += GetShootInput;
	}
	void OnDisable()
	{
		GameManager.Game.Controls.Gameplay.Shoot.performed -= GetShootInput;
	}

	void Update()
	{
		mousePosition = GameManager.Game.Controls.Global.MousePosition.ReadValue<Vector2>();
		worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

		if (shootCounter > 0)
		{
			shootCounter -= Time.deltaTime;
		}

		if (handSR.transform.position.x < playerHealth.transform.position.x)
		{
			handSR.flipY = true;
		}
		else
		{
			handSR.flipY = false;
		}

		float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
		transform.localRotation = Quaternion.Euler(0, 0, angle);
	}

	private void GetShootInput(InputAction.CallbackContext context)
	{
		if (GameManager.Game.State.Paused)
			return;

		if (shootCounter <= 0 && !playerHealth.getDead())
		{
			GameObject projectile = Instantiate(batProjectile, transform.position, transform.rotation);
			AudioHelpers.PlayOneShot(GameManager.Game.Config.PlayerAttack);
			shootCounter = shootCooldown;
		}
	}
}
