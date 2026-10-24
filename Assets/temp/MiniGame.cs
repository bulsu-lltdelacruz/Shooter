using UnityEngine;

public class MiniGame : MonoBehaviour
{
    [SerializeField] int coinsNeeded;
    [SerializeField] int currentCoins;
    [SerializeField] float timeLimit;
    [SerializeField] float deltaTime;
    [SerializeField] Menu menu;
    bool isWon = false;
    private void Update()
    {
        if (isWon)
            return;
        deltaTime += Time.deltaTime;
        if(deltaTime >= timeLimit)
        {
            menu.Show("Game Over");
        }
    }
    public void AddCoins()
    {
        currentCoins++;
        if(currentCoins >= coinsNeeded)
        {
            isWon = true;
            menu.Show("WIN");
        }
    }
}
