using UnityEngine;

public class RangeDetector : MonoBehaviour
{
    [SerializeField]private E_AttackType attackType;
    [SerializeField] private Enemy enemy;
    [SerializeField] private float attackDetectionDelay;
    private GameObject player;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            player = other.gameObject;
        }if(other.CompareTag("Wall"))
        {
           // enemy.StopAttack();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
        }
    }
    private void Update()
    {
        if(player == null)
            return;
        enemy.Attack(E_AttackType.ATTACK_1, player.transform.position);
    }
}
