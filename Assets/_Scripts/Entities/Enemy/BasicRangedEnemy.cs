using UnityEngine;

public class BasicRangedEnemy : Enemy
{
    [SerializeField] private float beamDistance = 5f;
    [SerializeField] private float beamSpeed = 20f;
    private Vector3 playerLastPosition;
    //[SerializeField] CapsuleCollider enemyCollider;
    //[SerializeField] GameObject attckCollider;

    private float deltaDistance;
    private Vector3 currentDashDirection;

    [SerializeField]GameObject beamPrefab;
    Transform currentBeam;
    CharacterController controller;
    private void Start()
    {
        StartBase();
        controller = GetComponent<CharacterController>();
        animator.SetBool("IsRunning", true);

        int playerLayerIndex = LayerMask.NameToLayer("Player");
        playerTransform = EntityManager.Instance.GetPlayerObject().transform;
    }

    public override void Attack(E_AttackType attackType, Vector3 target)
    {
        if (isAttacking == true) // dont do shit if already attacking
            return;

        if (!CheckCooldownFinished())
            return;
        FaceDirection(playerTransform.position);
        currentBeam = ObjectPoolManager.SpawnObject(beamPrefab, transform.position,
                transform.rotation,
                ObjectPoolManager.PoolType.GameObjects).transform;
        currentBeam.GetComponent<BeamHelper>().parent = gameObject;
        playerLastPosition = target;

        StartAttacking();
    }

    public void DamageEnemy(IDamageable playerDamageable)
    {
        playerDamageable.TakeDamage(2, transform.position, transform.forward);
        StopAttack();
    }
    private void UpdateAttack()
    {
        float deltaTime = Time.deltaTime;
        currentBeam.position += currentBeam.forward * beamSpeed * deltaTime;
        //controller.Move(currentDashDirection * dashSpeed * deltaTime);
        deltaDistance += beamSpeed * deltaTime;

        if (deltaDistance > beamDistance)
        {
            StopAttack();
        }
    }

    public override void StopAttack()
    {
        EndAttacking();
        //attckCollider.SetActive(false);
        deltaDistance = 0;
       // ObjectPoolManager.ReturnObjectToPool(currentBeam.gameObject);
        currentBeam = null;
    }
    private void Update()
    {
        base.FrameUpdate2();

        Attack(E_AttackType.ATTACK_1, playerTransform.position);
        if (isAttacking)
        {
            UpdateAttack();
        }

    }
}
