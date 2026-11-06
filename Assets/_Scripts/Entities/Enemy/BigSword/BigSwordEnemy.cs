using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BigSwordEnemy : Enemy
{
    #region combos
    [SerializeField] private ComboSequenceSO combo;
    EnemyComboAttackState comboAttackState;
    #endregion combos
    //[SerializeField] CapsuleCollider enemyCollider;

    private float deltaDistance;
    private Vector3 currentFinalDashTarget;


    float normalSpeed, normalAcceleration;
   // [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashSpeed = 60f;
    [SerializeField] private float dashAcceleration = 60f;
    [SerializeField] private float maxDashTime = 1.5f;
    private float dashTimer;

    // Changed to LayerMask to store the bitmask properly
    private LayerMask playerLayerMask;
    private LayerMask originalExcludeLayers;
    private LayerMask originalIncludeLayers;
    int wallLayer;
    CharacterController controller;
    EnemyStateMachine enemyStateMachine;
    Vector3 currentTarget;
    [SerializeField] UIDocument DEBUG_Document;
    VisualElement root; 
    
    [SerializeField] private float attackTurnSpeed = 720f; // degrees/sec
    private bool canTrackTarget;

    public void EnableTracking() => canTrackTarget = true;
    public void DisableTracking() => canTrackTarget = false;

    private void TrackTarget()
    {
        if (!canTrackTarget) return;

        Vector3 dir = currentTarget - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, look, attackTurnSpeed * Time.deltaTime);
    }
    private void Start()
    {
        //DEBUG
        root = DEBUG_Document.rootVisualElement;
        StartBase();
        normalSpeed = agent.speed;
        normalAcceleration = agent.acceleration;
        controller = GetComponent<CharacterController>();
        animator.SetBool("IsRunning", true);
        enemyStateMachine = new EnemyStateMachine();
        comboAttackState = new(this, animator, combo);
        //enemyStateMachine.Initialize();

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
        OnParryEvt += OnParry;
    }
    private void OnDisable()
    {
        OnDisableeBase();
        OnStunEvt -= OnStun;
        OnParryEvt -= OnParry;
    }

    public override void Attack(E_AttackType attackType, Vector3 target)//gets called by the ranged detector
    {//called inside rangedetector, just tries attacking
        if(IsPatrolling)
            return;
        currentTarget = target;
        comboAttackState.TryAttacking();
        //Debug.LogError("FLICKER!!!");
    }

    public override void Dash(Vector3 target)
    {
        if (isAttacking || isDashing || isStunned || !agent.enabled || IsPatrolling) return;
        isDashing = true;
        dashTimer = 0f;

        Vector3 stop = GetStopPoint(target);
        if (NavMesh.SamplePosition(stop, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            stop = hit.position;

        currentFinalDashTarget = stop;
        agent.speed = dashSpeed;
        agent.acceleration = dashAcceleration;
        agent.SetDestination(stop);
    }

    private void EndDash()
    {
        isDashing = false;
        agent.speed = normalSpeed;
        agent.acceleration = normalAcceleration;
    }
    private Vector3 GetStopPoint(Vector3 target)
    {
        float stopDistance = 1.5f;
        Vector3 dir = target - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= stopDistance * stopDistance)
            return transform.position;

        return target - dir.normalized * stopDistance;
    }
    public override void Attack()
    {
        EndDash();
        StartAttacking();
        attackCollider.SetActive(true);
        controller.excludeLayers = originalExcludeLayers | playerLayerMask;

        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", false);
        FaceDirection(currentTarget);
        agent.updateRotation = false;   // stop the agent from fighting our rotation
        canTrackTarget = true;
    }
   // public override void Attack()
    //{//called inside coomboattackStat4e actually attacks
    //    IsDashFirst = true;
      //  agent.enabled = false;
    //}
    public override void IncrementCharacterMovement(float amount)
    {
        if (amount == 0)
            return;
        transform.position += transform.forward * (amount * Time.deltaTime);
    }
    public override void StopAttack()
    {
        //EndAttacking();
        //  attackCollider.SetActive(false);
        deltaDistance = 0;
        deltaAttackDelay = 0;

        animator.SetBool("IsRunning", false);
        //animator.SetBool("IsIdle", true);

        //isAttacking = false;
        currentFinalDashTarget = Vector3.zero;

        controller.excludeLayers = originalExcludeLayers;
        controller.includeLayers = originalIncludeLayers;
        canTrackTarget = false;
        agent.updateRotation = true;
        ResetState();
    }
    public void ResetState()
    {
        EndAttacking();
    }
    public void DamageEnemy(IDamageable playerDamageable)
    {
        playerDamageable.TakeDamage(20, transform.position, transform.forward);
    }
    private void UpdateAttack()
    {
        float deltaTime = Time.deltaTime;
        //controller.Move(currentDashDirection * dashSpeed * deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, currentFinalDashTarget, dashSpeed*Time.deltaTime);
        deltaDistance += dashSpeed * deltaTime;
      /*  if (deltaDistance >= dashDistance)
        {
            StartAttacking();

            controller.excludeLayers = originalExcludeLayers | playerLayerMask;

            animator.SetBool("IsRunning", false);
            animator.SetBool("IsIdle", false);
            FaceDirection(currentTarget);
        }*/
    }

    private void Update()
    {
        root.Q<Label>("label-1").text = "Attack  : " + isAttacking;
        root.Q<Label>("label-2").text = "Combo   : " + comboAttackState.currentComboNameForDebugging;
        root.Q<Label>("label-3").text = "Parried : " + isParried;
        if(IsPatrolling)
        {
            base.UpdatePatrol();
            return;
        }
        if (isStunned)
        {
            CountDownStun();
            return;
        }
        if(isParried)
        {
            Debug.Log("parry me ass");
            CountDownParried();
            return;
        }
        if (isInParryWindow)
        {
            return;
        }
        comboAttackState.UpdateState();//only gets updated (set to stop attacking when we can attack, 
        if (isAttacking)
        {
            //UpdateAttack();
            TrackTarget();
            return;
        }
        if (isDashing)
        {
            dashTimer += Time.deltaTime;
            bool arrived = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
            if (arrived || dashTimer >= maxDashTime)
                EndDash();
            return;   // don't let FrameUpdate2 overwrite the dash destination
        }
        base.FrameUpdate2();
        // Debug.Log(isInAttackDelay);


    }
    /*EVENTS*/
    private void OnStun()
    {
        EndDash();
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", false);
    }
    public void OnParry()
    {
        EndDash();
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", false);
    }
    private void OnRemoveStun()
    {

    }
}

