using UnityEngine;

public class BaseWeaponClass : MonoBehaviour
{
    //public GameObject prefab;
    public WeaponData data;
    public Transform[] fireOrigin;
    public Texture2D icon;
    public string GUN_idleTrigger;
    public string GUN_shootTrigger;
    public string GUN_reloadTrigger;
    public string ARM_idleTrigger;
    public string ARM_shootTrigger;
    public string ARM_reloadTrigger;
    [Header("Local Positions")]
    public Vector3 position;
    public Vector3 rotation;
    public float scale;
    [Header("Gun Camera Z Position")]
    public float gunCameraZ;
}
