using NUnit.Framework.Internal.Commands;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(EffectController))]
public class Enemy : Entity
{
    protected bool isDead;
    [Header("Sounds")]
    [SerializeField] private AudioClip parryHitSound;
    [SerializeField] private AudioClip normalHitSound;
    [SerializeField] private AudioClip stunHitSound;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private float footstepInterval = .4f;
    [SerializeField] private float footstepVolume = .5f;
    private float deltaStepInterval;
    [Header("prefabs")]
    [SerializeField] public GameObject parryPrefab;
    [SerializeField] private GameObject stunPrefab;
    [SerializeField] public UIDocument HPBarDoc;
    protected VisualElement HPBar;
    protected VisualElement PoiseBar;
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
    [SerializeField] protected float stunDuration;
    private float parryDuration = .1f;
    protected float deltaStunTime;
    EnemyComboAttackState comboAttackState;
    protected EnemySounds sounds;
    protected bool isDashing = false;
    protected bool isIdle = false;
    [Header("Patrolling")]
    [SerializeField] float patrolSpeed;
    float normalAgentSpeed;
    float normalAgentStopDistance;
    [SerializeField] float patrolStayTime;
    protected bool IsPatrolling = true;
    [SerializeField] Transform patrolParent;
    private Transform[] patrolLocations;
    private int currentPatrolIndex;
    [SerializeField] float distanceBeforeAlert;
    [SerializeField] LayerMask patrolLayers;
    [Header("Detection")]
    [SerializeField] float eyeHeight = 1.6f;
    [SerializeField] float playerTargetHeight = .5f;
    [SerializeField] float loseInterestDistance = 20f;
    int sightMask;
    int playerLayer;
    public Event takeDamageEvts;
    Color ogColor;
    Color friedColor;
    string originalColor = "#FFFFFF";
    string orangeColor = "#FF8312";
    //EffectController effectController;
    //protected WaitForSeconds attackDelayWait;

    public void SetEmission(bool enable)
    {
        if (enable)
        {
            PoiseBar.style.backgroundColor = friedColor;
        }
        else
        {
            PoiseBar.style.backgroundColor = ogColor;
        }
    }
    float deltaDBTime;
    float totalDBTime;
    public void ApplyDebuffForTime(float time, EffectBase effect)
    {
        effectController.AddEffect(effect);
        SetEmission(true);
        StartCoroutine(removeDebuffAfter(time, effect));
    }
    IEnumerator removeDebuffAfter(float time, EffectBase e)
    {
        yield return new WaitForSeconds(time);
        SetEmission(false);
        effectController.RemoveEffect(e);
    }
    protected void StartBase()
    {
        if (ColorUtility.TryParseHtmlString(orangeColor, out Color hexColor))
        {
            friedColor = hexColor;
        }
        if (ColorUtility.TryParseHtmlString(originalColor, out Color hexColors))
        {
            ogColor = hexColors;
        }
        playerObject = EntityManager.Instance.GetPlayerObject();
        playerDamageable = EntityManager.Instance.GetPlayerDamageable();
        //effectController = GetComponent<EffectController>();
        playerTransform = playerObject.transform;
        currentPoise = maxPoise;
        currentHP = maxHP;// StartBase()
        if (HPBarDoc != null)
        {
            HPBar = HPBarDoc.rootVisualElement.Q<VisualElement>("hp-bar");
            PoiseBar = HPBarDoc.rootVisualElement.Q<VisualElement>("poise-bar");
            HPBarWidth = HPBar.style.width.value.value;
        }
        sounds = GetComponentInChildren<EnemySounds>();
        //agent.updateRotation = false;
        patrolLocations = patrolParent.Cast<Transform>().ToArray();
        normalAgentSpeed = agent.speed;
        normalAgentStopDistance = agent.stoppingDistance;
        playerLayer = LayerMask.NameToLayer("Player");
        sightMask = patrolLayers.value | (1 << playerLayer) | LayerMask.GetMask("Wall");
        patrolParent.SetParent(null);
        TogglePatrol(true);
    }
    protected void OnEnableBase()
    {
        if (EntityManager.Instance == null)
            Debug.Log("actually entity null");
        EntityManager.Instance.RegisterEnemy(gameObject);
        OnStunEvt += OnStun;
        OnParryEvt += OnParry;
    }

    protected void OnDisableeBase()
    {
        EntityManager.Instance.UnRegisterEnemy(gameObject);
        OnStunEvt -= OnStun;
        OnParryEvt -= OnParry;
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
    public virtual void Dash(Vector3 target)
    {
        isDashing = true;
        agent.enabled = false;
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
    public override void TakeDamageWithImpact(Vector3 source, DamageType damageType, Vector3 hitPoint, Quaternion hitRotation)
    {
        if (isDead) return;
        TogglePatrol(false);
        float poiseRes = this.poiseRes;
        float damageRes = (isStunned) ? 0 : this.damageRes;
        if (damageType != DamageType.MELEE)
        {
            if (normalHitSound)
                SoundFXManager.instance.PlaySoundFXClip(normalHitSound, transform, .5f, SoundType.SOUNDFX);
        }
        if (isInParryWindow && !isStunned)
        {
            if (parryHitSound)
                SoundFXManager.instance.PlaySoundFXClip(parryHitSound, transform, .5f, SoundType.SOUNDFX);
            poiseRes = 0;
            damageRes = 0;
            ObjectPoolManager.SpawnObject(
                GetParryPrefab(),
                hitPoint,
                hitRotation,
                ObjectPoolManager.PoolType.ParticleSystems
            );
            /*Debug.Log("MERGER>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            HitLag.Instance.SlowMo(.2f, .65f);
            StopAttack();
            OnParryEvt.Invoke();*/
            HitLag.Instance.SlowMo(.5f, .5f);
            OnParryEvt?.Invoke();
        }
        float modifiedBasePoise = effectController.Bus.Modify(GameEvent.DamageTaken, totalPoiseDamage);

        float finalPoiseDamage = Mathf.Max(0, modifiedBasePoise * (1 - poiseRes));

        float finalHPDamage = CalculateHealthDamage((totalPelletDamage * (1 - damageRes)), damageType);
        finalHPDamage = (isStunned) ? finalHPDamage * 1.5f : finalHPDamage;
        currentPoise = Mathf.Max(0, currentPoise - finalPoiseDamage);
        currentHP = Mathf.Max(0, currentHP - finalHPDamage);

        base.TakeDamageWithImpact(source, damageType, hitPoint, hitRotation);
        if (currentHP <= 0)
        {
            isDead = true;

            if (isStunned && damageType.Equals(DamageType.MELEE))
            {
                Die(4);
            }
            else
            {
                Die(1);
            }
            PlayerStatus.Instance.AddSpellCharge();
        }
        float hpPercent = (currentHP / maxHP) * 100;
        float poisePercent = (currentPoise / maxPoise) * 100;
        HPBar.style.width = Length.Percent(hpPercent);
        PoiseBar.style.width = Length.Percent(poisePercent);
        EndParryWindow();
    }
    #region EVENT RELATED
    private void OnStun()
    {
        if (stunHitSound)
            SoundFXManager.instance.PlaySoundFXClip(stunHitSound, transform, .5f, SoundType.SOUNDFX);
        isStunned = true;
        animator.SetBool("IsStunned", isStunned);
        EndAttacking();
    }
    private void OnParry()
    {
        isParried = true;
        animator.SetBool("IsStunned", isParried);
        EndAttacking();
    }
    private void OnRemoveParry()
    {
        isStunned = false;
        isParried = false;
        deltaStunTime = 0;
    }
    private void OnRemoveStun()
    {
        currentPoise = maxPoise;
        float poisePercent = (currentPoise / maxPoise) * 100;
        PoiseBar.style.width = Length.Percent(poisePercent);
        isStunned = false;
        isParried = false;
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
    protected void TogglePatrol(bool isPatrol)
    {
        if (isPatrol)
        {
            IsPatrolling = true;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0;
        }
        else
        {
            IsPatrolling = false;
            agent.speed = normalAgentSpeed;
            agent.stoppingDistance = normalAgentStopDistance;
        }
    }
    public bool CanSeePlayer(float maxDistance)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = playerTransform.position + Vector3.up * playerTargetHeight;
        Vector3 toTarget = target - origin;
        float dist = toTarget.magnitude;

        if (dist > maxDistance) return false;
        if (dist < 0.01f) return true;

        Vector3 dir = toTarget / dist;
        Debug.DrawRay(origin, dir * dist, Color.red);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, sightMask, QueryTriggerInteraction.Ignore))
            return hit.collider.gameObject.layer == playerLayer;

        return false;
    }

    protected bool CheckLoseInterest()
    {
        float limit = Mathf.Max(loseInterestDistance, distanceBeforeAlert + 1f);
        if ((playerTransform.position - transform.position).sqrMagnitude <= limit * limit)
            return false;

        ReturnToPatrol();
        return true;
    }

    private void ReturnToPatrol()
    {
        TogglePatrol(true);

        if (agent.enabled && agent.isOnNavMesh)
            agent.ResetPath(); // stop walking toward the player's last position

        // resume from the closest patrol point
        if (patrolLocations != null && patrolLocations.Length > 0)
        {
            float best = float.MaxValue;
            for (int i = 0; i < patrolLocations.Length; i++)
            {
                float d = (patrolLocations[i].position - transform.position).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    currentPatrolIndex = i;
                }
            }
        }

        deltaPatrolStayTime = patrolStayTime; // head to the patrol point right away
    }
    private float deltaPatrolStayTime;
    protected void UpdatePatrol()
    {
        if (CanSeePlayer(distanceBeforeAlert))
        {
            TogglePatrol(false);
            return;
        }
        if (!agent.enabled || patrolLocations == null || patrolLocations.Length == 0)
            return;
        if (agent.pathPending)
            return;

        bool arrived = !agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.1f;
        if (arrived)
        {
            deltaPatrolStayTime += Time.deltaTime;
            if (deltaPatrolStayTime >= patrolStayTime)
            {
                agent.SetDestination(patrolLocations[currentPatrolIndex].position);
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolLocations.Length;
                deltaPatrolStayTime = 0f;
            }
        }

        bool idle = agent.velocity.sqrMagnitude < 0.01f;
        animator.SetBool("IsIdle", idle);
        animator.SetBool("IsRunning", !idle);
        UpdateFootsteps(!idle);
    }
    #region FOOTSTEPS
    protected void UpdateFootsteps(bool isWalking)
    {
        if (!isWalking || footstepSound == null)
        {
            ResetWalkSound();
            return;
        }
        deltaStepInterval += Time.deltaTime;
        if (deltaStepInterval >= footstepInterval)
        {
            deltaStepInterval = 0;
            SoundFXManager.instance.PlaySoundFXClip(footstepSound, transform, footstepVolume, SoundType.SOUNDFX);
        }
    }
    public void ResetWalkSound()
    {
        deltaStepInterval = footstepInterval;
    }
    #endregion END FOOTSTEPS
    #region COUNTDOWNS
    protected void FrameUpdate2()
    {
        if (isAttacking)
        {
            ResetWalkSound();
            return;
        }
        if (CheckLoseInterest())
            return;
        base.FrameUpdate();
        UpdateFootsteps(agent.enabled && agent.velocity.sqrMagnitude > 0.01f);
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
    protected void CountDownParried()
    {
        deltaStunTime += Time.deltaTime;
        if (deltaStunTime >= parryDuration)
        {
            animator.SetBool("IsStunned", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsIdle", true);
            OnRemoveParry();
            Debug.Log("BYE STUN");
        }
    }
    protected void CountDownStun()
    {
        deltaStunTime += Time.deltaTime;
        if (stunDuration - deltaStunTime <= .25f)
        {
            animator.SetBool("IsStunned", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsIdle", true);
            Debug.Log("BYE STUN");
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