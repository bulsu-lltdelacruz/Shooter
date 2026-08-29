using UnityEngine;

public class PlayerChargingAltFireState : PlayerState
{
    Transform cameraTransform;
    LayerMask mask;
    PlayerStateController playerStateController;
    public PlayerChargingAltFireState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        cameraTransform = player.cameraHandler.gunCamera.transform;
        mask = LayerMask.GetMask("Damageable","Enemy");
    }

    public override void EnterState()
    {
        playerStateController = player.PlayerStateController;
        gunAnimator = player.gunAnimator;
    }
    public override void ExitState()
    {
        gunAnimator = player.gunAnimator;
    }
    public override void FrameUpdate()
    {
        if (!playerStateController.isChargingAltFire)
            player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, 999, mask ))
        {
            player.playerDashMeleeState.UpdateLockedOnTarget(hit.collider.gameObject);
        }
    }
    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
}
