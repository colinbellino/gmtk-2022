using UnityEngine;
using Game.Core;

public class PlayerHealth : Game.Core.Health
{
	[SerializeField] private int healthLossPerSecond;
	[SerializeField] private float healthLossCooldown;
	private float timer;

	[SerializeField] private float invicibilityDuration;
	private float invincibilityCounter;
	private float redColorValue;

	[SerializeField] float heartbeatSfxDuration;
	[SerializeField] private Material onHitFlashMaterial;
	[SerializeField] private GameObject healEffect;

	protected override void Awake() {
		base.Awake();

		redColorValue = entitySR.color.r;
		InvokeRepeating("CallHeartbeat", 0, heartbeatSfxDuration);
	}

	protected override void Death()
	{
		base.Death();
		AudioHelpers.PlayOneShot(GameManager.Game.Config.PlayerDeath);
	}

	protected override void Update()
	{
		base.Update();

		if (currentHP > 0)
		{
			timer += Time.deltaTime;
			if (timer >= healthLossCooldown)
			{
				timer = 0f;
				DealDamage(healthLossPerSecond, Vector3.zero, false);
			}
		}

		if (invincibilityCounter > 0) {
			invincibilityCounter -= Time.deltaTime;

			if (invincibilityCounter <= 0) {

			}
		}
	}

	public override void DealDamage(int damageDone, Vector3 damageSourceDirection, bool screenshake = true) {
		// Player can't take damage in assist mode
		if (GameManager.Game.State.PlayerSettings.AssistMode) {
			return;
		}

		if (invincibilityCounter <= 0) {
			if (currentHP - damageDone < 0) {
				setCurrentHP(0);
			} else {
				setCurrentHP(currentHP - damageDone);
			}

			if (currentHP > 0) {
				if (screenshake) {
					AudioHelpers.PlayOneShot(GameManager.Game.Config.PlayerDamage);
					_ = Utils.Shake(0.3f, 100);
				}

				if (damageSourceDirection.magnitude > 0) {
					knockbackDirection = (transform.position - damageSourceDirection).normalized;
					knockbackCounter = knockBackDuration;

					entitySR.material = onHitFlashMaterial;
				}

				invincibilityCounter = invicibilityDuration;
			}
		}
	}

	public void Heal(int healAmount)
	{
		_ = Utils.Shake(0.1f, 100);
		GameObject.Instantiate(healEffect, transform.position, Quaternion.identity, transform);
		AudioHelpers.PlayOneShot(GameManager.Game.Config.PlayerHeal);
		if (currentHP + healAmount > maxHP) {
			setCurrentHP(maxHP);
		} else {
			setCurrentHP(currentHP + healAmount);
		}
	}

	private void CallHeartbeat() {
		if ((float)currentHP / (float)maxHP < 0.3) {
			AudioHelpers.PlayOneShot(GameManager.Game.Config.HeartBeat, transform.position);
		}
	}
}

