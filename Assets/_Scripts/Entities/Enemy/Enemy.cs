using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Enemy : Entity
{
    [SerializeField] public UIDocument HPBarDoc;
    protected VisualElement HPBar;
    protected float HPBarWidth;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator animator;
    [SerializeField] protected float navDelay = .25f;
    protected float deltaTime;
    protected IDamageable playerDamageable;
    protected GameObject playerObject;
    protected Transform playerTransform;

    [SerializeField] public GameObject attackCollider;
    [SerializeField] protected float attackCooldown = .5f;
    private float deltaCooldown;
    public bool isAttacking = false;

    protected float deltaAttackDelay;
    protected bool isInParryWindow;
    public bool isStunned;
    [SerializeField] protected float stunDuration;
    protected float deltaStunTime;
    EnemyComboAttackState comboAttackState;
    protected EnemySounds sounds;
    //protected WaitForSeconds attackDelayWait;
    protected void StartBase()
    {
        playerObject = EntityManager.Instance.GetPlayerObject();
        playerDamageable = EntityManager.Instance.GetPlayerDamageable();
        playerTransform = playerObject.transform;
        currentPoise = maxPoise;
        currentHP = maxHP;
        HPBar = HPBarDoc.rootVisualElement.Q<VisualElement>("hp-bar");
        HPBarWidth = HPBar.style.width.value.value;
        sounds = GetComponentInChildren<EnemySounds>();
        //agent.updateRotation = false;
    }
    protected void OnEnableBase()
    {
        Debug.Log("add");
        EntityManager.Instance.RegisterEnemy(gameObject);
        OnStunEvt += OnStun;
    }

    protected void OnDisableeBase()
    {
        EntityManager.Instance.UnRegisterEnemy(gameObject);
        OnStunEvt -= OnStun;
    }
    
    #region ATTACKS
    public void StartAttacking()
    {
        isAttacking = true;
        agent.enabled = false;
    }
    public void EndAttacking()
    {
        isAttacking = false;
        agent.enabled = true;
        deltaCooldown = 0;
    }
    public virtual void Attack()
    {
        //the actyual attack logic
    }
    public virtual void Attack(E_AttackType attackType, Vector3 target)
    {
        //gets called inside rangeddetector to try attacking if possible
    }
    public virtual void StopAttack()
    {

    }
    #endregion END ATTACKS
    public void FaceDirection(Vector3 targetDirection)
    {
        Vector3 faceDirection = targetDirection - transform.position;
        faceDirection.Normalize();
        faceDirection.y = 0;
        Vector3 finalFaceDirection = new Vector3(faceDirection.x, 0, faceDirection.z);
        transform.rotation = Quaternion.LookRotation(new Vector3(finalFaceDirection.x, 0, finalFaceDirection.z));
    }
    public virtual void IncrementCharacterMovement(float amount)
    {
        /*currentDashDirection = target - transform.position;
        currentDashDirection.Normalize();
        currentDashDirection.y = 0;
        Vector3 faceDirection = new Vector3(currentDashDirection.x, 0, currentDashDirection.z);
        transform.rotation = Quaternion.LookRotation(faceDirection);*/
    }
    public override void TakeDamageWithImpact(Vector3 source, DamageType damageType)
    {
        base.TakeDamageWithImpact(source, damageType);
        if(currentHP <=0)
        {
            if (isStunned && damageType.Equals(DamageType.MELEE))
            {
                Die(4);
            }else
            {

                Die(1);
            }
            
        }
        float hpPercent = (currentHP/maxHP)*100;
        HPBar.style.width = Length.Percent(hpPercent);
        if (isInParryWindow)
        {
            StopAttack();
        }
        EndParryWindow();
    }
    #region EVENT RELATED
    private void OnStun()
    {
        isStunned = true;
        animator.SetBool("IsStunned", isStunned);
        EndAttacking();
    }
    private void OnRemoveStun()
    {
        currentPoise = maxPoise;
        isStunned = false;
        deltaStunTime = 0;
    }
    public virtual void StartParryWindow()
    {
        isInParryWindow = true;
    }
    public virtual void EndParryWindow()
    {
        deltaAttackDelay = 0f;
        isInParryWindow = false;
    }
    #endregion END EVENT RELATED
    

    #region COUNTDOWNS
    protected void FrameUpdate2()
    {
        if (isAttacking)
        {
            Debug.Log("FUCKLER " + isAttacking);
            return;
        }
        base.FrameUpdate();
        deltaTime += Time.deltaTime;
        if (deltaTime > navDelay)
        {
            if (!agent.enabled)
                return;

            deltaTime += Time.deltaTime;
            if (deltaTime > navDelay)
            {
                agent.SetDestination(playerTransform.position);
                deltaTime = 0;
            }

            bool isIdle = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
            animator.SetBool("IsIdle", isIdle);
            animator.SetBool("IsRunning", !isIdle);

        }
        UpdateCooldown();
    }
    protected void CountDownStun()
    {
        deltaStunTime += Time.deltaTime;
        if (stunDuration - deltaStunTime <= .25f)
        {
            animator.SetBool("IsStunned", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsIdle", true);
        }
        if (deltaStunTime >= stunDuration)
        {
            OnRemoveStun();
        }
    }
    public bool CheckCooldownFinished()
    {
        return deltaCooldown >= attackCooldown;
    }
    protected void UpdateCooldown()
    {
        if (!isAttacking)
        {
            if (deltaCooldown > attackCooldown)
            {
                return;
            }

            deltaCooldown += Time.deltaTime;
        }
    }
    #endregion END COUNTDOWNS
}
public enum E_AttackType
{
    ATTACK_1,
    ATTACK_2,
    ATTACK_3,
    ATTACK_4,
    ATTACK_5
}
