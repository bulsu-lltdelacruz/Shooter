using Unity.VisualScripting;
using UnityEngine;

public class PlayerState
{
    protected Player player;
    protected PlayerStateMachine playerStateMachine;
    protected Animator armAnimator;
    protected Animator gunAnimator;
    protected float baseCooldown = .375f;
    protected float deltaTime;
    protected PlayerState(Player player, PlayerStateMachine playerStateMachine, Animator armAnimator, Animator gunAnimator)
    {
        this.player = player;
        this.playerStateMachine = playerStateMachine;
        this.armAnimator = armAnimator;
        this.gunAnimator = gunAnimator;
    }
    public virtual void EnterState()
    {

    }
    public virtual void ExitState()
    {

    }
    public virtual void FrameUpdate()
    {

    }
    public virtual void PhysUpdate()
    {

    }
    public virtual void AnimationTriggerEvent()
    {

    }
}
