using UnityEngine;

public class PlayerMeleeState : PlayerState
{
    Vector3 gunCamOrigPosition;

    const float hitDelay = 0.1f;
    const float hitForwardOffset = 0.75f;
    bool hasHit;

    public PlayerMeleeState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        base.baseCooldown = .375f - .07f;
    }
    string val;
    public override void EnterState()
    {
        hasHit = false;
        deltaTime = 0f;

        gunCamOrigPosition = player.cameraHandler.gunCamera.transform.localPosition;
        player.cameraHandler.gunCamera.transform.localPosition = new Vector3(gunCamOrigPosition.x, gunCamOrigPosition.y, -0.35f);
        player.playerInventory.HideWeaponGameobject(true);
        val = Random.Range(1, 3).ToString();
        armAnimator.SetTrigger($"ArmMelee{1}");
    }
    public override void ExitState()
    {
        player.cameraHandler.gunCamera.transform.localPosition = gunCamOrigPosition;
        player.playerInventory.HideWeaponGameobject(false);
        //reset stretch, the other one is in armanimcontroller
        player.animationEventBus.armEvents.evtRevertArmMelee?.Invoke();
        armAnimator.ResetTrigger($"ArmMelee{1}");
    }
    public override void FrameUpdate()
    {
        deltaTime += Time.deltaTime;

        if (!hasHit && deltaTime >= hitDelay)
        {
            hasHit = true;
            SpawnImpact();
        }

        if (deltaTime > baseCooldown)
        {
            player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);
            deltaTime = 0f;
        }
    }

    void SpawnImpact()
    {
        Transform playerTransform = player.transform;

        Vector3 pos = playerTransform.position + player.cameraHandler.GetForward() * hitForwardOffset;

        GameObject impactPrefab = Prefabs.Instance.GetImpactPrefab();
        ObjectPoolManager.SpawnObject(impactPrefab, pos, playerTransform.rotation, ObjectPoolManager.PoolType.GameObjects);
    }

    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
}