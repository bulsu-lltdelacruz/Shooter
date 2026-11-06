using UnityEngine;

public class Ladder : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player != null && player.playerWalkingState != null)
            player.playerWalkingState.SetLadder(this);
    }

    void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player != null && player.playerWalkingState != null)
            player.playerWalkingState.ClearLadder(this);
    }
}