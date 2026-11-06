using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Terminal : Interactable
{
    KeyType[] completeArray = { KeyType.red, KeyType.blue, KeyType.yellow};
    [SerializeField] FinalDoor doorConnected;
    [SerializeField]UIDocument doc;
    [SerializeField] AudioClip correctSound;
    [SerializeField] AudioClip wrongSound;
    VisualElement bg;
    VisualElement keys;
    Label result;
    WaitForSeconds wait = new WaitForSeconds(3);
    private void Start()
    {
        VisualElement root = doc.rootVisualElement;
        bg = root.Q<VisualElement>("bg");
        keys = root.Q<VisualElement>("keys");
        result = root.Q<Label>("result");
        bg.style.display = DisplayStyle.None;
        keys.style.display = DisplayStyle.Flex;
    }
    public void Interact()
    {
        Player player = EntityManager.Instance.GetPlayerObject().GetComponent<Player>();
        if(player.keyInventory.CheckKeyComplete(completeArray))
        {
            doorConnected.OpenDoor();
            bg.RemoveFromClassList("red");
            bg.AddToClassList("green");
            result.text = "SUCCESS";
            ShowDisplayForTime();
            SoundFXManager.instance.PlaySoundFXClip(correctSound, transform, 1, SoundType.SOUNDFX);
        }
        else
        {
            bg.RemoveFromClassList("green");
            bg.AddToClassList("red");
            result.text = "DENIED";
            ShowDisplayForTime();
            SoundFXManager.instance.PlaySoundFXClip(wrongSound, transform, 1, SoundType.SOUNDFX);
        }
    }
    private void ShowDisplayForTime()
    {
        bg.style.display = DisplayStyle.Flex;
        keys.style.display = DisplayStyle.None;
        StartCoroutine(CloseDisplay());
    }
    IEnumerator CloseDisplay()
    {
        yield return wait;
        bg.style.display = DisplayStyle.None;
        keys.style.display = DisplayStyle.Flex;
    }
}
