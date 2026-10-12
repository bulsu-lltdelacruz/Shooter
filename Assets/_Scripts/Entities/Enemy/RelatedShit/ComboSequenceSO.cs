using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Enemy/Combo Sequence")]
public class ComboSequenceSO : ScriptableObject
{
    public ComboStep[] steps; 
    [SerializeField]public ComboNode[] comboTree;
}

[System.Serializable]
public class ComboStep
{
    public string name;
    public string animTrigger;
    public float startupTime;
    public float activeTime;
    public float recoveryTime;
    public float chainWindow;
    public float damage;
    public float dashAmount;
    public float attackCooldown;//only if its the last sequence maybe?
    public float probability;
}
[Serializable]
public struct ComboNode
{
    public int currentSequence;
    public int[] availableSequence;
}