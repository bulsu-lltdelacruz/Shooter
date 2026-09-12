using UnityEngine;

public class AnimationEvtProxy : MonoBehaviour
{
    [SerializeField]Enemy enemy;
    public void StartParryWindow()
    {
        enemy.StartParryWindow();
    }
    public void EndParryWindow(int agag)
    {
        enemy.EndParryWindow();
    }
}
