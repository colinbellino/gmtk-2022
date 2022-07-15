using UnityEngine;
using Game.Core;
using DG.Tweening;

public class MeleeEnemy : MonoBehaviour
{
   [SerializeField] private SpriteRenderer enemyBodySR;
   private Rigidbody2D enemyRB;
   private Animator enemyAnimator;
   private EnemyHealth enemyHealth;
	private PlayerHealth playerHealth;
	[SerializeField] private Transform pitchforkTransform;
	[SerializeField] private SpriteRenderer pitchforkSpriteRenderer;
	private Entity enemyEntity;

	[SerializeField] private float moveSpeed;
	private Vector2 directionToPlayer;

	void Awake()
	{
		enemyRB = GetComponent<Rigidbody2D>();
		enemyAnimator = GetComponent<Animator>();
		enemyHealth = GetComponent<EnemyHealth>();
		enemyEntity = GetComponent<Entity>();
	}

	void OnEnable()
	{
		if (enemyEntity.Ready)
		{
			playerHealth = GameManager.Game.State.Player.GetComponent<PlayerHealth>();
		}

		pitchforkSpriteRenderer.transform.DOLocalMoveX(pitchforkSpriteRenderer.transform.localPosition.x -0.1f, 0.2f).SetLoops(-1, LoopType.Yoyo);
	}

   void Update() {
      if (!enemyHealth.getDead() && !playerHealth.getDead()) {
         directionToPlayer = (playerHealth.gameObject.transform.position - transform.position).normalized;

         if (playerHealth.gameObject.transform.position.x < transform.position.x) {
            enemyBodySR.flipX = false;
         }  else {
            enemyBodySR.flipX = true;
         }
      }

		if (enemyHealth.getDead()) {
			enemyRB.mass = float.MaxValue;
		}
   }

   void FixedUpdate() {
      if (!enemyHealth.getDead() && !playerHealth.getDead()) {
         attackPlayer(directionToPlayer);

 			float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
			pitchforkTransform.localRotation = Quaternion.Euler(0, 0, angle-180);
      } else {
			enemyRB.velocity = Vector2.zero;
		}
	}

	private void attackPlayer(Vector2 playerDirection)
	{
		enemyRB.MovePosition((Vector2)transform.position + (playerDirection * moveSpeed * Time.deltaTime));
	}
}
