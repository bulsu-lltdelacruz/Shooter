using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField] Player player;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.menuHandler.SetGameOverVisible(true, "Game Over");
        }
    }
}
