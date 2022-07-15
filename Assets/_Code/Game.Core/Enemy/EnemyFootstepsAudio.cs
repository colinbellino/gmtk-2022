using UnityEngine;
using Game.Core;

public class EnemyFootstepsAudio : MonoBehaviour {

    [SerializeField] private float footStepSpeed;
	 private EnemyHealth enemyHealth;

    private void Awake()
    {
		 enemyHealth = GetComponent<EnemyHealth>();
        InvokeRepeating("CallFootsteps", 0, footStepSpeed);
    }

    private void CallFootsteps ()
    {
        if (gameObject.activeInHierarchy && !enemyHealth.getDead())
            AudioHelpers.PlayOneShot(GameManager.Game.Config.VillagerEnemyMovement, transform.position);
    }
}
