using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core
{
	public class PlayerController : MonoBehaviour
	{
		private Rigidbody2D playerRB;
		private SpriteRenderer playerSR;
		private Animator playerAnimator;
		private AfterImage afterImage;
		[HideInInspector] public PlayerHealth Health;

		[SerializeField] private CapsuleCollider2D entitiesCollider;
		[SerializeField] private float xMoveSpeed;
		[SerializeField] private float yMoveSpeed;
		[SerializeField] private float diagonalXMoveSpeed;
		[SerializeField] private float xDashSpeed;
		[SerializeField] private float yDashSpeed;
		[SerializeField] private float dashCooldown;
		[SerializeField] private float dashDuration;
		[SerializeField] private GameObject _dustParticle;

		private float dashCounter;
		private bool isDashing;
		private Vector2 rawMovementInput;

		private bool facingRight = true;

		private string currentAnimState;
		private string animIdle = "vampire_animations_Idle";
		private string animDash = "vampire_animations_Dash";
		private string animWalk = "vampire_animations_Walk";
		private string animDeath = "vampire_animations_Death";
		private bool _deathAnimDone;
		private float _dustTimestamp;

		public float DashProgress => 1 - (dashCounter / dashCooldown);
		public bool IsFullyDead => _deathAnimDone;

		void Awake()
		{
			playerRB = GetComponent<Rigidbody2D>();
			playerSR = GetComponentInChildren<SpriteRenderer>();
			playerAnimator = GetComponentInChildren<Animator>();
			Health = GetComponent<PlayerHealth>();
			afterImage = playerSR.GetComponent<AfterImage>();
			ChangeAnimationState(animIdle);
		}

		void OnEnable()
		{
			Globals.Controls.Gameplay.Dash.performed += GetDashInput;
			Health.CurrentHPChanged += OnCurrentHPChanged;
		}

		void OnDisable()
		{
			Globals.Controls.Gameplay.Dash.performed -= GetDashInput;
			Health.CurrentHPChanged -= OnCurrentHPChanged;
		}

		void Update()
		{
			if (Health.getDead())
				return;

			if (isDashing)
			{
				entitiesCollider.enabled = false;
				if (dashCooldown - dashCounter > dashDuration)
				{
					isDashing = false;
					afterImage.Activate(false);
				}
			}
			else
			{
				entitiesCollider.enabled = true;
				rawMovementInput = Globals.Controls.Gameplay.Move.ReadValue<Vector2>();
			}

			if (rawMovementInput.magnitude > 0 && Time.time >= _dustTimestamp)
			{
				GameObject.Instantiate(_dustParticle, transform.position + new Vector3(0, -0.55f), Quaternion.identity);
				_dustTimestamp = Time.time + 0.2f;
			}

			if (dashCounter > 0)
			{
				dashCounter -= Time.deltaTime;
			}
			else
			{
				dashCounter = 0;
			}

			playerSR.flipX = !facingRight;
		}

		void FixedUpdate()
		{
			if (Health.getDead())
				return;

			if (isDashing)
			{
				ChangeAnimationState(animDash);
				playerRB.velocity = new Vector2(rawMovementInput.x * xDashSpeed, rawMovementInput.y * yDashSpeed);
			}
			else if (Health.getKnockbackCounter() <= 0)
			{
				handleMovement(rawMovementInput);
			}
		}

		private void handleMovement(Vector2 RawMovementInput)
		{
			if (RawMovementInput != Vector2.zero)
			{
				ChangeAnimationState(animWalk);

				if (RawMovementInput.y == 0)
				{
					playerRB.velocity = new Vector2(RawMovementInput.x * xMoveSpeed, 0);
				}
				else
				{
					playerRB.velocity = new Vector2(RawMovementInput.x * diagonalXMoveSpeed, RawMovementInput.y * yMoveSpeed);
				}

				if (playerRB.velocity.x < 0)
				{
					facingRight = false;
				}
				else
				{
					facingRight = true;
				}
			}
			else
			{
				ChangeAnimationState(animIdle);
				playerRB.velocity = Vector2.zero;
			}
		}

		private void GetDashInput(InputAction.CallbackContext context)
		{
			if (context.action.triggered && dashCounter <= 0 && Health.getKnockbackCounter() <= 0)
			{
				isDashing = true;
				dashCounter = dashCooldown;
				AudioHelpers.PlayOneShot(Globals.Config.PlayerDash, transform.position);
				afterImage.Activate(true);
			}
		}

		void ChangeAnimationState(string newState)
		{
			if (currentAnimState == newState) return;

			playerAnimator.Play(newState);

			currentAnimState = newState;
		}

		void OnTriggerEnter2D(Collider2D col)
		{
			GameObject collidedWith = col.gameObject;

			if (collidedWith.tag == "enemy" || collidedWith.tag == "enemyWeapon")
			{
				EnemyHealth enemyHealth;
				if (collidedWith.tag == "enemy")
				{
					enemyHealth = collidedWith.GetComponent<EnemyHealth>();
				}
				else
				{
					enemyHealth = collidedWith.GetComponentInParent<EnemyHealth>();
				}

				if (enemyHealth.getDead())
				{
					if (isDashing && !enemyHealth.getDrained())
					{
						Health.Heal(enemyHealth.GetDrainHealthToPlayer());
						enemyHealth.setDrained(true);
						enemyHealth.stunEffectObject.SetActive(false);
					}
				}
				else if (!isDashing)
				{
					Health.DealDamage(enemyHealth.GetDamageToPlayer(), collidedWith.transform.position);
				}
			}
		}

		void OnTriggerStay2D(Collider2D col)
		{
			GameObject collidedWith = col.gameObject;

			if (collidedWith.tag == "enemy" || collidedWith.tag == "enemyWeapon")
			{
				EnemyHealth enemyHealth;
				if (collidedWith.tag == "enemy")
				{
					enemyHealth = collidedWith.GetComponent<EnemyHealth>();
				}
				else
				{
					enemyHealth = collidedWith.GetComponentInParent<EnemyHealth>();
				}

				if (!isDashing && !enemyHealth.getDead())
				{
					Health.DealDamage(enemyHealth.GetDamageToPlayer(), collidedWith.transform.position);
				}
			}
		}

		private async void OnCurrentHPChanged(int current, int max)
		{
			if (current <= 0)
			{
				GetComponentInChildren<PlayerProjectile>().gameObject.SetActive(false);
				playerRB.bodyType = RigidbodyType2D.Static;
				ChangeAnimationState(animDeath);
				await UniTask.Delay(1500);
				_deathAnimDone = true;
			}
		}

		public bool getIsDashing()
		{
			return isDashing;
		}
	}
}

