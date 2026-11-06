using UnityEngine;

public class GameFinish : MonoBehaviour
{
    [SerializeField] Player player;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.menuHandler.SetGameOverVisible(true, "Game Finished");
        }
    }
}
