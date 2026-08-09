using UnityEngine;
using UnityEngine.UIElements;

public class DEBUGGER_HANDLER : MonoBehaviour
{
    UIDocument document;
    VisualElement root;
    Label l_state;
    [SerializeField] Player player;
    void Start()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement as VisualElement;
        l_state = root.Q<Label>("state-label");
    }

    void Update()
    {
        l_state.text = $"STATE {player.PlayerStateController.DEBUG_GET_CURRENT_STATE()}";
    }
}
