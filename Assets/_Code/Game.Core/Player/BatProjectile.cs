using UnityEngine;
using Game.Core;

public class BatProjectile: MonoBehaviour {
	[SerializeField] private float projectileSpeed;
	[SerializeField] private int damage = 1;
	private SpriteRenderer batSR;

	void Awake() {
		batSR = GetComponentInChildren<SpriteRenderer>();
		batSR.transform.rotation = Quaternion.identity;
	}

	void Update() {
		transform.position += transform.right * projectileSpeed * Time.deltaTime;
	}

	void OnTriggerEnter2D(Collider2D col) {
		GameObject collidedWith = col.gameObject;

		switch (collidedWith.tag) {
			case "enemy": {
				EnemyHealth enemyHealth = collidedWith.GetComponent<EnemyHealth>();
				if (!enemyHealth.getDead()) {
					enemyHealth.DealDamage(damage, transform.position);

					Destroy(gameObject);
				}
				break;
			}
			case "wall": {
				AudioHelpers.PlayOneShot(GameManager.Game.Config.BatProjectileExplode, transform.position);
				Destroy(gameObject);
				break;
			}
			case "props": {
				AudioHelpers.PlayOneShot(GameManager.Game.Config.BatProjectileExplode, transform.position);
				Destroy(gameObject);
				break;
			}
			default: break;
		}
	}
}
