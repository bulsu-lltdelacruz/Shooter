using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BigSwordEnemy : Enemy
{
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashSpeed = 4f;
    #region combos
    [SerializeField] private ComboSequenceSO combo;
    EnemyComboAttackState comboAttackState;
    #endregion combos
    //[SerializeField] CapsuleCollider enemyCollider;

    private float deltaDistance;
    private Vector3 currentFinalDashTarget;


    float normalSpeed, normalAcceleration;
    [SerializeField] private float dashAcceleration = 60f;

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
    }
    private void OnDisable()
    {
        OnDisableeBase();
        OnStunEvt -= OnStun;
    }


    public override void Attack(E_AttackType attackType, Vector3 target)//gets called by the ranged detector
    {//called inside rangedetector, just tries attacking
        currentTarget = target;
        comboAttackState.TryAttacking();
    }
    public override void Dash(Vector3 target)
    {
        if (isAttacking || isDashing) return;
        isDashing = true;

        Vector3 stop = GetStopPoint(target);
        if (NavMesh.SamplePosition(stop, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            stop = hit.position;                 // snap the point onto the navmesh

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
            return transform.position;   // already close enough

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
        if (deltaDistance >= dashDistance)
        {
            StartAttacking();

            // attackCollider.SetActive(true);
            controller.excludeLayers = originalExcludeLayers | playerLayerMask;

            animator.SetBool("IsRunning", false);
            animator.SetBool("IsIdle", false);
            FaceDirection(currentTarget);
            /*currentDashDirection = target - transform.position;
            currentDashDirection.Normalize();
            currentDashDirection.y = 0;
            Vector3 faceDirection = new Vector3(currentDashDirection.x, 0, currentDashDirection.z);
            transform.rotation = Quaternion.LookRotation(faceDirection);*/
        }
    }

    private void Update()
    {
        root.Q<Label>("label-1").text = "Attack  : " + isAttacking;
        root.Q<Label>("label-2").text = "Combo   : " + comboAttackState.currentComboNameForDebugging;
        root.Q<Label>("label-3").text = "DASHING: " + isDashing;
        if (isStunned)
        {
            CountDownStun();
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
            return;
        }
        if (isDashing)
        {
            Debug.Log("DASHING");
            transform.position = Vector3.MoveTowards(transform.position, currentFinalDashTarget, dashSpeed * Time.deltaTime);
            if ((currentFinalDashTarget - transform.position).sqrMagnitude <= 0.0001f)
            {
                EndDash();
                agent.enabled = true;
            }
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
    private void OnRemoveStun()
    {

    }
}

