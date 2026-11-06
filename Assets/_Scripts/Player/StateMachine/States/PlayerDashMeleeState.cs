using System.Collections;
using UnityEngine;

public class PlayerDashMeleeState : PlayerState
{
    //IsArmMeleeStart
    GameObject target;
    float dashSpeed = 100f;
    Transform playerTransform;
    Transform targetTransform;
    bool exitFlag;
    public PlayerDashMeleeState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        playerTransform = player.transform;
        base.baseCooldown = .375f; //- .1f;
    }
    public override void EnterState()
    {
        if (target == null)
        {
            player.playerStateMachine.ChangeState(player.playerMeleeState);
            return;
        }
        dashTimer = 0f;
        gunAnimator = player.gunAnimator;
        player.playerWalkingState.allowMovement = false;
        armAnimator.SetBool("IsArmMeleeStart", true);
        player.playerInventory.HideWeaponGameobject(true);
        exitFlag = false;
        player.PlayerStateController.allowAltFireCharging = false;
        player.meleeCollider.gameObject.SetActive(true);
        player.meleeCollider.targetEnemy = target;
    }
    public override void ExitState()
    {
        armAnimator.SetBool("IsArmMeleeStart", false);
        armAnimator.ResetTrigger("ArmMelee1");
        player.playerInventory.HideWeaponGameobject(false);
        player.playerWalkingState.allowMovement = true;
        player.PlayerStateController.allowAltFireCharging = true;
        exitFlag = false;
        deltaTime = 0f;
        RemoveLockedOnTarget();
        player.meleeCollider.gameObject.SetActive(false);
        player.meleeCollider.targetEnemy = null;
        dashTimer = 0f;
    }
    // fields
    float dashTimer;
    const float maxDashTime = 0.6f;       // bail-out so you can never get stuck
    const float arriveTolerance = 0.1f;   // fixes the float-equality problem



    public override void FrameUpdate()
    {
        gunAnimator = player.gunAnimator;
        if (exitFlag)
        {
            deltaTime += Time.deltaTime;
            if (deltaTime > baseCooldown)
            {
                player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);
                deltaTime = 0f;
            }
            return;
        }

        if (target == null || targetTransform == null || !target.activeInHierarchy)
        {
            player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);
            return;
        }

        dashTimer += Time.deltaTime;

        // aim at the enemy's body center, not their feet pivot
        Vector3 targetPos = targetTransform.position;
        if (target.TryGetComponent<Collider>(out Collider col))
            targetPos = col.bounds.center;

        // full 3D direction, so looking/dashing down or up works
        Vector3 toTarget = targetPos - playerTransform.position;
        float distance = toTarget.magnitude;
        float stopDistance = 1.51f;

        bool arrived = distance <= stopDistance + arriveTolerance;
        bool timedOut = dashTimer >= maxDashTime;

        if (!arrived && !timedOut)
        {
            float step = Mathf.Min(dashSpeed * Time.deltaTime, distance - stopDistance);
            Vector3 before = playerTransform.position;
            player.characterController.Move(toTarget.normalized * step);

            // blocked by floor/wall: moved far less than intended, so treat it as arrived instead of hanging
            if ((playerTransform.position - before).magnitude < step * 0.1f)
                arrived = true;
        }

        if (arrived || timedOut)
        {
            GameObject impactPrefab = Prefabs.Instance.GetImpactPrefab();
            exitFlag = true;
            armAnimator.SetBool("IsArmMeleeStart", false);
            armAnimator.SetTrigger("ArmMelee1");
            ObjectPoolManager.SpawnObject(impactPrefab, playerTransform.position, playerTransform.rotation, ObjectPoolManager.PoolType.GameObjects);
            HitLag.Instance.Freeze(StaticStuff.hitLagTime);
        }
    }
    /*IEnumerator ExitAfterTime()
    {

        player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);
    }*/
    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
    public void RemoveLockedOnTarget()
    {
        this.target = null;
        this.targetTransform = null;
    }
    public void UpdateLockedOnTarget(GameObject target)
    {
        this.target = target;
        this.targetTransform = target.transform;
    }
}
