using System;
using System.Collections;
using UnityEngine;

public class EnemyComboAttackState
{
    public Action OnAttackStart;
    public Action OnAttackEnd;
    private readonly Enemy enemy;
    private readonly ComboSequenceSO combo;
    private Animator animator;
    private int stepIndex;
    private float timer;
    private enum Phase {Startup, Active, Recovery, ChainWindow, None}
    private Phase phase;
    int[] comboTreeStartSteps;//mga start ng combo sequence(1,2,3,4), but not index, since it can start at 3rd sequence, but its on the second index
    int currentComboTreeStartIndex;//the index I mentioned above
    public string currentComboNameForDebugging;
    public string currentCooldownStateForDebugging;
    int previousStepIndex = 0;
    private bool CanAttack = true;
    public EnemyComboAttackState(Enemy enemy, Animator animator, ComboSequenceSO combo)
    {
        this.enemy = enemy;
        this.combo = combo;
        this.animator = animator;
        phase = Phase.None;
        comboTreeStartSteps = new int[combo.comboTree.Length];
        for (int i =0;i<combo.comboTree.Length; i++)
        {
            comboTreeStartSteps[i] = combo.comboTree[i].currentSequence;
        }
    }
    private int _GetRandomComboStart(float amount)
    {
        float cumulative = 0f;

        for (int i = 0; i < combo.steps.Length; i++)
        {
            cumulative += combo.steps[i].probability;
            if (amount <= cumulative)
            {
                return i;
            }
        }
        return -1;
    }

    public void TryAttacking()
    {
        if(!CanAttack)
            return;
        float randPoint = UnityEngine.Random.Range(0, 1);
        //= UnityEngine.Random.Range(0,comboTreeStartSteps.Length);
        int randomComboStartIndex = _GetRandomComboStart(randPoint);
        if (phase == Phase.None)//if currently not attacking
        {
            currentComboTreeStartIndex = randomComboStartIndex;
            stepIndex = comboTreeStartSteps[currentComboTreeStartIndex];
            BeginStep("TryAttacking()above");
        }
        else if(phase == Phase.ChainWindow) // if available to chain
        {
            int availSeqLen = combo.comboTree[currentComboTreeStartIndex].availableSequence.Length;//getcurrent combo, then its abailable sequences lengt
            int randAvailSeq = UnityEngine.Random.Range(0, availSeqLen);//randomfrom it
            int availSequence = combo.comboTree[currentComboTreeStartIndex].availableSequence[randAvailSeq];//get random available sequnece 
            
            
            if (availSequence < combo.steps.Length && availSequence != -1)
            {
                stepIndex = availSequence;
                currentComboTreeStartIndex = stepIndex;
                BeginStep($"TryAttacking({stepIndex})");//problem: not the exit state, its the combonode is not progressing so its not getting to negative 1
                return;
            }
            else
            {
                //after exiting chain, player also enters reagne and it starts immadatae again
                ExitState();
                phase = Phase.None;
                stepIndex = 0;
            }
            // enemy.fsm.ChangeState(ctx.states.Chase); // combo finished, go back to normal behavior
        }
        //enemy.agent.isStopped = true;
        //  BeginStep();
    }
    public void ExitState()
    {
        //shouldnt only be when cahin ends, also should be when its the last attack in the combo string
        enemy.StartCoroutine(StartCooldown(combo.steps[stepIndex].attackCooldown));
        phase = Phase.None;
        animator.SetBool(combo.steps[stepIndex].animTrigger, false);
      //  Debug.Log("Start cooldown : " + combo.steps[stepIndex].attackCooldown + "disable : " + combo.steps[stepIndex].animTrigger);
        //if (stepIndex < combo.steps.Length)
        //  {

        // }
    }
    private void BeginStep(string f)
    {
        var step = combo.steps[stepIndex];
        var previousStep = combo.steps[previousStepIndex];
        animator.SetBool(previousStep.animTrigger, false);
        animator.SetBool(step.animTrigger, true);

        previousStepIndex = stepIndex;
        phase = Phase.Startup;
        timer = step.startupTime;
    }

    public void UpdateState()
    {
        if (phase == Phase.None)
        {
            /* animator.SetBool("IsIdle", true);
             if (enemy is BigSwordEnemy e)
                 e.ResetState();*/
            currentComboNameForDebugging = "none";
            return;
        }
        var step = combo.steps[stepIndex];
        currentComboNameForDebugging = step.name;
        currentComboNameForDebugging = CanAttack.ToString();
    timer -= Time.deltaTime;
        switch (phase)
        {
            case Phase.Startup:
                if (timer <= 0f) { phase = Phase.Active; timer = step.activeTime; enemy.Attack(); }
                break;

            case Phase.Active:
                if (timer <= 0f) { phase = Phase.Recovery; timer = step.recoveryTime; enemy.StopAttack(); } else { enemy.IncrementCharacterMovement(step.dashAmount); }
                    break;

            case Phase.Recovery:
                if (timer <= 0f) { phase = Phase.ChainWindow; timer = step.chainWindow; }
                break;

            case Phase.ChainWindow:
                if (timer <= 0f)
                {
                    ExitState();
                }
                break;
            case Phase.None:
                
                break;
        }
    }
    private IEnumerator StartCooldown(float cooldown)
    {
        CanAttack = false;
        yield return new WaitForSeconds(cooldown);
        CanAttack = true;
    }
    //private bool CanContinueCombo()
   // {
       // float dist = Vector3.Distance(ctx.transform.position, ctx.target.position);
       // return dist <= ctx.attackRange && ctx.target.IsAlive;
  //  }

    //public void ExitState()
    //{
       // ctx.hitbox.SetActive(false);
      //  ctx.agent.isStopped = false;
  //  }
}