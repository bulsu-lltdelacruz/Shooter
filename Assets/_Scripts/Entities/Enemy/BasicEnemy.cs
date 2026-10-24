using UnityEngine;

public class BasicEnemy : Enemy
{
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashSpeed = 4f;
    //[SerializeField] CapsuleCollider enemyCollider;
    [SerializeField] GameObject attckCollider;

    private float deltaDistance;
    private Vector3 currentDashDirection;

    // Changed to LayerMask to store the bitmask properly
    private LayerMask playerLayerMask;
    private LayerMask originalExcludeLayers;
    private LayerMask originalIncludeLayers;
    int wallLayer;
    CharacterController controller;
    private void Start()
    {
        StartBase();
        controller = GetComponent<CharacterController>();
        animator.SetBool("IsRunning", true);

        int playerLayerIndex = LayerMask.NameToLayer("Player");
        wallLayer = LayerMask.NameToLayer("Wall");
        playerLayerMask = 1 << playerLayerIndex;

        originalExcludeLayers = controller.excludeLayers;
        originalIncludeLayers = controller.includeLayers;
        //Add events on top of the base event form enemy that propagates to entity
    }
    private void OnEnable()
    {
        OnEnableBase();
        OnStunEvt += OnStun;
    }
    private void OnDisable()
    {
        OnDisableeBase();
        OnStunEvt -= OnStun;
    }


    public override void Attack(E_AttackType attackType, Vector3 target)
    {
        Debug.Log("FI(CK");
        if (isAttacking == true) // dont do shit if already attacking
            return;

        if (!CheckCooldownFinished())
            return;
        StartAttacking();

        attckCollider.SetActive(true);
        if(controller == null) 
            Debug.Log("wawawa");
        controller.excludeLayers = originalExcludeLayers | playerLayerMask;

        animator.SetBool("IsRunning", false);
        animator.SetBool("IsAttacking", true);
        animator.SetBool("IsIdle", false);

        currentDashDirection = target - transform.position;
        currentDashDirection.Normalize();
        currentDashDirection.y = 0;
      //  FaceDirection(currentDashDirection);
         Vector3 faceDirection = new Vector3(currentDashDirection.x, 0, currentDashDirection.z);
         transform.rotation = Quaternion.LookRotation(faceDirection);
         StartParryWindow();
    }
    
    public void DamageEnemy(IDamageable playerDamageable)
    {
        playerDamageable.TakeDamage(10, transform.position, transform.forward);
    }
    private void UpdateAttack()
    {
        float deltaTime = Time.deltaTime;
        //transform.position += currentDashDirection * dashSpeed * deltaTime;
        controller.Move(currentDashDirection * dashSpeed * deltaTime);
        deltaDistance += dashSpeed * deltaTime;

        if (deltaDistance > dashDistance)
        {
            StopAttack();
        }
    }

    public override void StopAttack()
    {
        EndAttacking();
        attckCollider.SetActive(false);
        deltaDistance = 0;
        deltaAttackDelay = 0;

        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", true);
        animator.SetBool("IsAttacking", false);

        currentDashDirection = Vector3.zero;

        controller.excludeLayers = originalExcludeLayers;
        controller.includeLayers = originalIncludeLayers;

    }
    private void Update()
    {
        if (isStunned)
        {
            CountDownStun();
            return;
        }
        base.FrameUpdate2();
       // Debug.Log(isInAttackDelay);
        if(isInParryWindow)
        {
            return;
        }
        if (isAttacking)
        {
            UpdateAttack();
        }

    }
    /*EVENTS*/
    private void OnStun()
    {
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", false); 
    }
    private void OnRemoveStun()
    {

    }
    /*public override void TakeDamageWithImpact(Vector3 hitPoint)
    {
        //base.TakeDamage(amount, hitPoint, hitNormal);
        // Debug.Log(isInAttackDelay + "  :  "+Time.deltaTime  );
        if(isInAttackDelay)
        {
            Debug.Log("parried bros ass");
            StopAttack();
        }else
        {
            Debug.Log("Normal shit");
        }
        EndParryWindow();
    }*/
}
