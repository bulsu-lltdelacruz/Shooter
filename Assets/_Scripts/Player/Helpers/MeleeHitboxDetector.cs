using UnityEngine;

public class MeleeHitboxDetector : MonoBehaviour
{
    public bool isTargetInRange = false;
    public GameObject targetEnemy = null;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == targetEnemy)
        {
            isTargetInRange = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == targetEnemy)
        {
            isTargetInRange = false;
        }
    }
    private void OnDisable()
    {
        isTargetInRange=false;
        targetEnemy = null;
    }
}
