using System.Collections;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerSpellState : PlayerState
{
    //IsArmMeleeStart
    GameObject target;
    float dashSpeed = 100f;
    Transform playerTransform;
    Transform targetTransform;
    bool exitFlag;
    WaitForSeconds waitTime;
    GameObject currentSpell;
    public PlayerSpellState(
        Player player,
        PlayerStateMachine playerStateMachine,
        Animator armAnimator,
        Animator gunAnimator) :
        base(player, playerStateMachine, armAnimator, gunAnimator)
    {
        playerTransform = player.transform;
        base.baseCooldown = .3f; //- .1f;
        waitTime = new WaitForSeconds(baseCooldown);
    }

    public override void EnterState()
    {
        armAnimator.SetTrigger("ArmPower");
        armAnimator.SetLayerWeight(leftArmLayerIndex, 1f);
        player.weaponController.weaponState.gameObject.SetActive(false);
        player.StartCoroutine(ExitAfterTime());
        /*GameObject spellPrefab = */player.playerPrefabs.spellPrefab.SetActive(true);
        //currentSpell = ObjectPoolManager.SpawnObject(spellPrefab, spellPrefab.transform.localPosition, spellPrefab.transform.localRotation);
        //currentSpell.transform.parent = player.leftHand.transform;
       // ObjectPoolManager.ReturnObjectToPool(currentSpell);
    }
    public override void ExitState()
    {
        armAnimator.ResetTrigger("ArmPower");
        armAnimator.SetLayerWeight(leftArmLayerIndex, 1f);
        player.weaponController.weaponState.gameObject.SetActive(true);
        player.playerPrefabs.spellPrefab.SetActive(false);
        //ObjectPoolManager.ReturnObjectToPool(currentSpell, ObjectPoolManager.PoolType.ParticleSystems);
    }
    public override void FrameUpdate()
    {
        
    }
    IEnumerator ExitAfterTime()
    {
        yield return waitTime;
        player.PlayerStateController.TryChangeState(E_PlayerStates.IDLE);
    }
    public override void PhysUpdate()
    {

    }
    public override void AnimationTriggerEvent()
    {

    }
}
