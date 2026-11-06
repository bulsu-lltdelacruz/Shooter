using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HintDamageManager : MonoBehaviour
{
    public static HintDamageManager Instance;

    UIDocument doc;
    VisualElement root;
    VisualElement damageScreen;
    Label hint;

    Coroutine hintRoutine;
    Coroutine damageRoutine;

    const float DefaultHintTime = 1f;
    const float DamageTime = .2f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;
        damageScreen = root.Q<VisualElement>("damage-screen");
        hint = root.Q<Label>("hint");
    }

    public void ShowDamage()
    {
        if (damageScreen == null) return;

        damageScreen.style.display = DisplayStyle.Flex;

        if (damageRoutine != null)
            StopCoroutine(damageRoutine);
        damageRoutine = StartCoroutine(RemoveDamageAfter());
    }

    IEnumerator RemoveDamageAfter()
    {
        yield return new WaitForSeconds(DamageTime);
        damageScreen.style.display = DisplayStyle.None;
        damageRoutine = null;
    }

    public void ShowHint(string text)
    {
        ShowHint(text, DefaultHintTime);
    }

    public void ShowHint(string text, float time)
    {
        if (hint == null) return;

        hint.text = text;
        hint.style.display = DisplayStyle.Flex;

        if (hintRoutine != null)
            StopCoroutine(hintRoutine);
        hintRoutine = StartCoroutine(RemoveHintAfter(time));
    }

    IEnumerator RemoveHintAfter(float time)
    {
        yield return new WaitForSeconds(time);
        hint.text = "";
        hint.style.display = DisplayStyle.None;
        hintRoutine = null;
    }
}