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

        dashTimer += Time.deltaTime;

        // flat direction: ignore height difference so you don't dive into the floor
        Vector3 toTarget = targetTransform.position - playerTransform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        float stopDistance = 1.51f;

        if (distance <= stopDistance + arriveTolerance || dashTimer >= maxDashTime)
        {
            GameObject impactPrefab = Prefabs.Instance.GetImpactPrefab();
            exitFlag = true;
            armAnimator.SetBool("IsArmMeleeStart", false);
            armAnimator.SetTrigger("ArmMelee1");
            ObjectPoolManager.SpawnObject(impactPrefab, playerTransform.position, playerTransform.rotation, ObjectPoolManager.PoolType.GameObjects);
            HitLag.Instance.Freeze(StaticStuff.hitLagTime);
            return;
        }

        // never step past the stop distance
        float step = Mathf.Min(dashSpeed * Time.deltaTime, distance - stopDistance);
        player.characterController.Move(toTarget.normalized * step);
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
