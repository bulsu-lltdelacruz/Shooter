using UnityEngine;

public class Guide : Interactable
{
    [TextArea]public string text;
    public float timeShown;

    public void ShowText()
    {
        HintDamageManager.Instance.ShowHint(text, timeShown);
    }
}
