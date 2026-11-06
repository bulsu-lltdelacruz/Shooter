using UnityEngine;

public class BasicRangedEnemy : Enemy
{
    [Header("Beam")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private float beamDistance = 5f;
    [SerializeField] private float beamSpeed = 20f;
    [Tooltip("Where the beam spawns. If empty, uses this transform + Fire Height.")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireHeight = 1f;
    [Tooltip("Only used if the player has no collider to aim at.")]
    [SerializeField] private float fallbackAimHeight = 1f;

    private float attackTimer;
    private Collider playerCollider;

    private void Start()
    {
        StartBase();
        animator.SetBool("IsRunning", true);

        playerTransform = EntityManager.Instance.GetPlayerObject().transform;
        playerCollider = playerTransform.GetComponentInChildren<Collider>();
    }

    private void OnEnable()
    {
        OnEnableBase();
    }

    private void OnDisable()
    {
        OnDisableeBase();
    }

    public override void Attack(E_AttackType attackType, Vector3 target)
    {
        if (isAttacking || isStunned || isParried) // don't fire while already attacking or stunned
            return;

        if (!CheckCooldownFinished())
            return;

        FaceDirection(playerTransform.position);

        // Aim in 3D (the enemy's own rotation is flattened, so don't use transform.forward for the beam)
        Vector3 origin = GetFireOrigin();
        Vector3 aimDir = GetAimPoint(target) - origin;
        aimDir = aimDir.sqrMagnitude > 0.0001f ? aimDir.normalized : transform.forward;

        GameObject beamObj = ObjectPoolManager.SpawnObject(
            beamPrefab,
            origin,
            Quaternion.LookRotation(aimDir),
            ObjectPoolManager.PoolType.GameObjects);

        // The beam moves and despawns itself from here on, independent of this enemy
        beamObj.GetComponent<BeamHelper>().Launch(gameObject, beamSpeed, beamDistance);

        attackTimer = 0f;
        StartAttacking();
    }

    public void DamageEnemy(IDamageable playerDamageable)
    {
        playerDamageable.TakeDamage(2, transform.position, transform.forward);
        StopAttack();
    }

    public override void StopAttack()
    {
        EndAttacking();
        attackTimer = 0f;
    }

    private Vector3 GetFireOrigin()
    {
        return firePoint != null ? firePoint.position : transform.position + Vector3.up * fireHeight;
    }

    private Vector3 GetAimPoint(Vector3 target)
    {
        if (playerCollider != null)
            return playerCollider.bounds.center;
        return target + Vector3.up * fallbackAimHeight;
    }

    private void Update()
    {
        if (IsPatrolling)
        {
            base.UpdatePatrol();
            return;
        }

        // Tick stun/parry recovery (previously nothing in this class did, so the enemy stayed stunned)
        if (isStunned)
        {
            CountDownStun();
            return;
        }
        if (isParried)
        {
            CountDownParried();
            return;
        }

        base.FrameUpdate2();

        Attack(E_AttackType.ATTACK_1, playerTransform.position);

        // The enemy only "holds" its attack pose for as long as the beam takes to travel its range.
        // The beam itself no longer depends on this.
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= beamDistance / Mathf.Max(beamSpeed, 0.01f))
            {
                StopAttack();
            }
        }
    }
}