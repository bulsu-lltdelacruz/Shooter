using System;
using System.Collections;
using UnityEngine;

public class ArmAnimEventsHandler : AnimationEventHandlerBase
{
    [SerializeField]Transform armParent;
    public Action evtArmMelee;
    public Action evtRevertArmMelee;
    [SerializeField]float stretchAmount;//.33f default
    //.33
    private void Start()
    {
        evtArmMelee += OnMeleeStretch;
        evtRevertArmMelee += OnRevertMeleeStretch;
    }
    public void OnMeleeStretch()
    {
        if(armParent.localPosition.z < stretchAmount)
        {
            Vector3 localPosition = armParent.transform.localPosition;
            armParent.localPosition =  new Vector3(localPosition.x, localPosition.y, stretchAmount);
        }
            
    }
    public void OnRevertMeleeStretch()
    {
        if (armParent.localPosition.z >= stretchAmount)
        {
            StartCoroutine(LerpMeleeUnStretch(armParent.localPosition, 
                new Vector3(armParent.localPosition.x, armParent.localPosition.y, 0)));
        }
    }
    IEnumerator LerpMeleeUnStretch(Vector3 startPos, Vector3 endPos)
    {
        Vector3 localPosition = armParent.transform.localPosition;
        float timeElapsed = 0f;
        float duration = .2f;

        while (timeElapsed < duration)
        {
            float percentageComplete = timeElapsed / duration;
            armParent.localPosition = Vector3.Lerp(startPos, endPos, percentageComplete);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        /*while (armParent.localPosition.z >= stretchAmount)
        {
            armParent.localPosition = new Vector3(localPosition.x, localPosition.y, 0);
            yield return null;
        }*/
    }
}
