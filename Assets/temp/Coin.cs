using UnityEngine;

public class Coin : MonoBehaviour
{
    MiniGame miniGame;
    private void Start()
    {
        miniGame = FindAnyObjectByType<MiniGame>();
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if(!other.CompareTag("Player"))
            return;
        miniGame.AddCoins();
        Destroy(gameObject);
    }
}
