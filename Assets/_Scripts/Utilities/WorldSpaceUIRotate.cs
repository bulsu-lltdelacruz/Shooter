using UnityEngine;

public class WorldSpaceUIRotate : MonoBehaviour
{
    Transform player;
    private void Start()
    {
        player = EntityManager.Instance.GetPlayerObject().transform;
    }
    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
    }
}
