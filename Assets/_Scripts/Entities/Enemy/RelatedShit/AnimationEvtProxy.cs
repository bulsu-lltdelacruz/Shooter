using UnityEngine;
using UnityEngine.Audio;

public class AnimationEvtProxy : MonoBehaviour
{
    [SerializeField]Enemy enemy;
    [SerializeField] AudioClip slash;
    [SerializeField] AudioSource source;
    public void StartParryWindow()
    {
        enemy.StartParryWindow();
    }
    public void EndParryWindow(int agag)
    {
        enemy.EndParryWindow();
    }
    public void EnableHitBox()
    {
        if(enemy.attackCollider != null)
            enemy.attackCollider.SetActive(true);
        if (slash)
        {
            source.pitch = Random.Range(0.7f, 0.85f);
            source.PlayOneShot(slash);
        }
    }
    public void DisableHitBox()
    {
        if (enemy.attackCollider != null)
            enemy.attackCollider.SetActive(false);
    }
}
