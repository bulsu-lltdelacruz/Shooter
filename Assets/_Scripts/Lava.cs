using UnityEngine;

public class Lava : MonoBehaviour
{
    [SerializeField] private float damageInterval = 1.0f;
    private float deltaTime;
    // Update is called once per frame
    IDamageable player;
    void Update()
    {
        if(player == null)
            return;
        deltaTime += Time.deltaTime;
        if(deltaTime >= damageInterval)
        {
            deltaTime = 0;
            player.TakeDamage(10, Vector3.zero, Vector3.zero);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") )
        {
            player = EntityManager.Instance.GetPlayerDamageable();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
        }
    }
}
