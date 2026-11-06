using UnityEngine;

public class LocoPlayerDashingState : PlayerState
{
    float dashSpeed;
    float maxDashDistance;
    float deltaDashDistance;
    Vector3 forward;
    public LocoPlayerDashingState(
        Player player,
        PlayerStateMachine playerStateMachine,
        InputHandler playerInputs,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        dashSpeed = player.playerStats.dashSpeed;
        maxDashDistance = player.playerStats.maxDashDistance;
        forward = player.transform.forward;
    }
    public override void EnterState()
    {
        SoundFXManager.instance.PlaySoundFXClip(PlayerSounds.Instance.dash_sound, player.transform, 1, SoundType.SOUNDFX);
        player.playerPrefabs.dashPrefab.SetActive(true);
    }
    public override void ExitState()
    {
        player.playerPrefabs.dashPrefab.SetActive(false);
        deltaDashDistance = 0f;
    }
    public override void FrameUpdate()
    {
        _Dash();
    }
    private void _Dash()
    {
        float frameDashDistance = dashSpeed * Time.deltaTime;
        Vector3 dashStep = forward * frameDashDistance;
        player.characterController.Move(dashStep);

        deltaDashDistance += frameDashDistance;

        if (deltaDashDistance >= maxDashDistance)
        {
            player.PlayerStateController.ForceWalkState();
        }
    }
    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
    public void SetDirection(Vector3 forward)
    {
        this.forward = forward;
    }
}
