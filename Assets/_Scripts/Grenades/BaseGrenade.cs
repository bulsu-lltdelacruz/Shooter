using UnityEngine;

public class BaseGrenade : MonoBehaviour
{
    private Vector3 localVelocity; // toss's own velocity (x/z world-space, y is the arc) — independent of the player's velocity after the throw
    [SerializeField] private float throwStrength;
    [SerializeField] private LayerMask grenadeDespawnMask;
    [SerializeField] private float gravity;
    [SerializeField] private float playerVelocityInfluence = 1f;
    private Transform playerTransform;
    protected Player player;
    private Vector3 localOffset; // grenade's position relative to the player, driven purely by the toss arc

   /* private void Start()
    {
        EntityManager.Instance.RegisterEnemy(gameObject);
    }*/
    protected void OnEnableBase()
    {
        localVelocity = Vector3.zero;
        localOffset = Vector3.zero;
        playerTransform = null;
        EntityManager.Instance.RegisterEnemy(gameObject);
    }
    protected void OnDisableBase()
    {
        EntityManager.Instance.UnRegisterEnemy(gameObject);
    }
    protected void OnDestroyBase()
    {
        EntityManager.Instance.UnRegisterEnemy(gameObject);
    }
    public virtual void Throw(Vector3 direction, Vector3 playerVelocity, Player thrower)
    {
        this.player = thrower;
        playerTransform = thrower.transform;
        localOffset = transform.position - playerTransform.position;

        Vector3 forceDirection = direction + (thrower.cameraHandler.gunCamera.transform.up * 1.3f);
        forceDirection.Normalize();

        Vector3 flatPlayerVelocity = new Vector3(playerVelocity.x, 0f, playerVelocity.z);
        localVelocity = forceDirection * throwStrength;
        localVelocity.y = 0f;
        localVelocity += flatPlayerVelocity * playerVelocityInfluence;

        localVelocity.y = forceDirection.y * throwStrength;
    }

    private void Update()
    {
        localVelocity.y -= gravity * Time.deltaTime;
        localOffset += localVelocity * Time.deltaTime;

        if (playerTransform != null)
        {
            transform.position = playerTransform.position + localOffset;
        }
        else
        {
            transform.position += localVelocity * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((grenadeDespawnMask.value & (1 << other.gameObject.layer)) != 0)
        {
            playerTransform = null;
            ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.GameObjects);
        }
    }
}