using System.Collections;
using System.Net;
using UnityEngine;

public class GrenadeController : MonoBehaviour
{
    private Player player;
    [SerializeField]private GameObject activeGrenade_1;
    [SerializeField]private GameObject activeGrenade_2;
    [SerializeField]private GameObject activeGrenade_3;
    [SerializeField]private Transform aimCamera;
    public void ThrowGrenade(string slot)
    {
        GameObject prefab = _GetGrenade(slot);
        Transform grenadeOrigin = player.grenadeOrigin;
        GameObject grenadeObject = ObjectPoolManager.SpawnObject(prefab, grenadeOrigin.transform.position,
            grenadeOrigin.transform.rotation,
             ObjectPoolManager.PoolType.GameObjects);

        Vector3 dir = aimCamera.forward;
        //dir = dir + (aimCamera.right * .1f);
        dir.Normalize();
        grenadeObject.GetComponent<BaseGrenade>().Throw(dir, player.playerWalkingState.currentVelocity, player);
        //grenade = Instantiate(grenade);
    }
    private GameObject _GetGrenade(string slot)
    {
        slot = slot.ToLower();
        switch (slot)
        {
            case "c":
                return activeGrenade_1;
            case "f":
                return activeGrenade_2;
            case "v":
                return activeGrenade_3;
            default: return null;
        }
    }
    public void Initialize(Player player)
    {
        this.player = player;
    }
}
