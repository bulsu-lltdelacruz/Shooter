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
    }
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

        /* Vector3 rawDirection = (targetTransform.position + (-playerTransform.forward * 1.2f)) - playerTransform.position;
         if (player.meleeCollider.isTargetInRange)
         {
             GameObject impactPrefab = Prefabs.Instance.GetImpactPrefab();
             exitFlag = true;
             armAnimator.SetBool("IsArmMeleeStart", false);
             armAnimator.SetTrigger("ArmMelee1");
             ObjectPoolManager.SpawnObject(impactPrefab, playerTransform.position, playerTransform.rotation, ObjectPoolManager.PoolType.GameObjects);
             return;
             // target.
         }
         playerTransform.position = Vector3.MoveTowards(
             playerTransform.position,
             targetTransform.position,
             dashSpeed * Time.deltaTime
         );*/
        Vector3 toTarget = targetTransform.position - playerTransform.position;
        float distance = toTarget.magnitude;
        float stopDistance = 1.32f; // melee range from the enemy's center
        if (distance <= stopDistance)
        {
            GameObject impactPrefab = Prefabs.Instance.GetImpactPrefab();
            exitFlag = true;
            armAnimator.SetBool("IsArmMeleeStart", false);
            armAnimator.SetTrigger("ArmMelee1");
            ObjectPoolManager.SpawnObject(impactPrefab, playerTransform.position, playerTransform.rotation, ObjectPoolManager.PoolType.GameObjects);
            return;
        }

        // Stop point sits on the line between you and the enemy, so it's always reachable
        Vector3 stopPoint = targetTransform.position - toTarget.normalized * stopDistance;

        playerTransform.position = Vector3.MoveTowards(
            playerTransform.position,
            stopPoint,
            dashSpeed * Time.deltaTime
        );
        /*     Vector3 rawDirection =(targetTransform.position+(-playerTransform.forward * 1.2f)) - playerTransform.position;
             float distance = rawDirection.magnitude;
             float stopDistance = .5f; //
             if (distance <= stopDistance)
             {
                 GameObject impactPrefab = Prefabs.Instance.GetImpactPrefab();
                 exitFlag = true;
                 armAnimator.SetBool("IsArmMeleeStart", false);
                 armAnimator.SetTrigger("ArmMelee1");
                 ObjectPoolManager.SpawnObject(impactPrefab, playerTransform.position, playerTransform.rotation, ObjectPoolManager.PoolType.GameObjects);
                 return;
                 // target.
             }
             playerTransform.position = Vector3.MoveTowards(
                 playerTransform.position,
                 targetTransform.position,
                 dashSpeed * Time.deltaTime
             );*/

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
