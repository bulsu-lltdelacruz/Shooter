using Unity.VisualScripting;
using UnityEngine;

public class EnemyState
{
    protected Enemy enemy;
    protected EnemyStateMachine playerStateMachine;
    protected Animator armAnimator;
    protected Animator gunAnimator;
    protected float baseCooldown = .375f;
    protected float deltaTime;
    protected int leftArmLayerIndex;
    protected EnemyState(Enemy enemy, EnemyStateMachine playerStateMachine, Animator armAnimator, Animator gunAnimator)
    {
        this.enemy = enemy;
        this.playerStateMachine = playerStateMachine;
        this.armAnimator = armAnimator;
        this.gunAnimator = gunAnimator;
        leftArmLayerIndex = armAnimator.GetLayerIndex("Left Arm");
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
