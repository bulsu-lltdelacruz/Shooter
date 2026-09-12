using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Enemy : Entity
{
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator animator;
    [SerializeField] protected float navDelay = .25f;
    protected float deltaTime;
    protected IDamageable playerDamageable;
    protected GameObject playerObject;
    protected Transform playerTransform;

    [Header("Attack cooldowns")]
    [SerializeField] protected float attackCooldown = .5f;
    private float deltaCooldown;
    protected bool isAttacking = false;

    protected float deltaAttackDelay;
    protected bool isInAttackDelay;
    //protected WaitForSeconds attackDelayWait;
    protected void StartBase()
    {
        EntityManager.Instance.RegisterEnemy(gameObject);
        playerObject = EntityManager.Instance.GetPlayerObject();
        playerDamageable = EntityManager.Instance.GetPlayerDamageable();
        playerTransform = playerObject.transform;
        //agent.updateRotation = false;
    }

    private void OnDestroy()
    {
        EntityManager.Instance.UnRegisterEnemy(gameObject);
    }
    public virtual void Attack(E_AttackType attackType, Vector3 target)
    {

    }

    public void StartAttacking()
    {
        isAttacking = true;
        agent.enabled = false;
    }
    public void FaceDirection(Vector3 targetDirection)
    {
        Vector3 faceDirection = targetDirection - transform.position;
        faceDirection.Normalize();
        faceDirection.y = 0;
        Vector3 finalFaceDirection = new Vector3(faceDirection.x, 0, faceDirection.z);
        transform.rotation = Quaternion.LookRotation(new Vector3(finalFaceDirection.x, 0, finalFaceDirection.z));
    }
    public void EndAttacking()
    {
        isAttacking = false;
        agent.enabled = true;
        deltaCooldown = 0;
    }
    public bool CheckCooldownFinished()
    {
        return deltaCooldown >= attackCooldown;
    }
    public virtual void StopAttack()
    {

    }
    public virtual void StartParryWindow()
    {
        isInAttackDelay = true;
    }
    public virtual void EndParryWindow()
    {
        deltaAttackDelay = 0f;
        isInAttackDelay = false;
    }
    protected void FrameUpdate2()
    {
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

        if (!isAttacking)
        {
            if (deltaCooldown > attackCooldown)
            {
                return;
            }

            deltaCooldown += Time.deltaTime;
        }

    }
}
public enum E_AttackType
{
    ATTACK_1
}
