using UnityEngine;
using Game.Core;

public class EnemyHealth : Game.Core.Health
{
	[SerializeField] private int drainHealthToPlayerAmt;
	[SerializeField] private int damageToPlayer;
	[SerializeField] private SpriteRenderer weaponSprite;
	private int drainHealthToPlayer = 0;

	[SerializeField] private Material onHitFlashMaterial;
	[SerializeField] private GameObject stunEffectPrefab;

	public GameObject stunEffectObject;

	private bool drained;

	void OnEnable()
	{
		if (dead)
		{
			entityAnimator.SetBool("isDead", true);
		}
		else
		{
			currentHP = maxHP;
		}
	}

	protected override void Update()
	{
		base.Update();
	}

	protected override void Death()
	{
		base.Death();

		AudioHelpers.PlayOneShot(Globals.Config.EnemyStun);
		enterStunState();
	}

	public override void DealDamage(int damageDone, Vector3 damageSourceDirection, bool screenshake = true)
	{
		setCurrentHP(currentHP - damageDone);
		AudioHelpers.PlayOneShot(Globals.Config.EnemyDamage);

		if (currentHP > 0)
		{
			if (damageSourceDirection.magnitude > 0)
			{
				knockbackDirection = (transform.position - damageSourceDirection).normalized;
				knockbackCounter = knockBackDuration;
				entitySR.material = onHitFlashMaterial;
			}
		}
	}

	public int GetDrainHealthToPlayer()
	{
		return drainHealthToPlayer;
	}
	public int GetDamageToPlayer()
	{
		return damageToPlayer;
	}

	private void enterStunState()
	{
		stunEffectObject = GameObject.Instantiate(stunEffectPrefab, transform);
		stunEffectObject.transform.localPosition = new Vector3(0f, 0.1f);

		entityAnimator.SetBool("isDead", true);
		weaponSprite.gameObject.SetActive(false);
		damageToPlayer = 0;
		drainHealthToPlayer = drainHealthToPlayerAmt;
	}

	public bool getDrained()
	{
		return drained;
	}
	public void setDrained(bool drained)
	{
		this.drained = drained;
	}
}

