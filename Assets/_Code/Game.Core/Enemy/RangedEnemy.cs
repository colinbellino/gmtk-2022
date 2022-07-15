using UnityEngine;
using Game.Core;

public class RangedEnemy : MonoBehaviour {
	private EnemyHealth enemyHealth;
	private PlayerHealth playerHealth;
	[SerializeField] private Transform crossTransform;
	private SpriteRenderer crossSR;
	[SerializeField] private SpriteRenderer enemyBodySR;
   private Rigidbody2D enemyRB;
   private Animator enemyAnimator;
	private Entity enemyEntity;
	[SerializeField] private GameObject enemyProjectile;

	private Vector2 directionToPlayer;
	private float shootCounter;
	[SerializeField] private float shootCooldown;

	void Awake()
	{
		enemyRB = GetComponent<Rigidbody2D>();
		enemyAnimator = GetComponentInChildren<Animator>();
		enemyHealth = GetComponent<EnemyHealth>();
		enemyEntity = GetComponent<Entity>();
		crossSR = crossTransform.gameObject.GetComponentInChildren<SpriteRenderer>();
	}

	void OnEnable()
	{
		if (enemyEntity.Ready)
		{
			playerHealth = GameManager.Game.State.Player.GetComponent<PlayerHealth>();
		}

		shootCounter = shootCooldown;
	}

   void Update() {
      if (!enemyHealth.getDead() && !playerHealth.getDead()) {
			crossTransform.right = (playerHealth.gameObject.transform.position - transform.position).normalized * -1;

         if (playerHealth.gameObject.transform.position.x < transform.position.x) {
            enemyBodySR.flipX = false;
				crossSR.flipY = false;
         }  else {
            enemyBodySR.flipX = true;
				crossSR.flipY = true;
         }

			if (shootCounter > 0) {
				shootCounter -=Time.deltaTime;
			}
      }
   }

   void FixedUpdate() {
      if (!enemyHealth.getDead() && !playerHealth.getDead()) {
			if (shootCounter <= 0) {
				attackPlayer();
			}

 			// float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
			// crossTransform.localRotation = Quaternion.Euler(0, 0, angle);
      }
	}

	private void attackPlayer() {
		AudioHelpers.PlayOneShot(GameManager.Game.Config.PriestEnemyAttack, transform.position);
		GameObject projectile = Instantiate(enemyProjectile, crossTransform.position, crossTransform.rotation);
		shootCounter = shootCooldown;
	}
}

