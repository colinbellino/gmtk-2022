using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    [SerializeField] private float lifeTime;

    void Update()
    {
        Destroy(gameObject, lifeTime);
    }
}
