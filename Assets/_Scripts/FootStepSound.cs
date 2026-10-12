using UnityEngine;

public class FootStepSound : MonoBehaviour
{
    LocoPlayerWalkingState walkState;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            walkState = other.gameObject.GetComponent<Player>().playerWalkingState;
            walkState.ChangeFootStepType(FootStepType.WATER);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            walkState = other.gameObject.GetComponent<Player>().playerWalkingState;
            walkState.ChangeFootStepType(FootStepType.DEFAULT);
        }
    }
}
