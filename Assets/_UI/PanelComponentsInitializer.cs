using UnityEngine;
using UnityEngine.UIElements;

public class PanelComponentsInitializer : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument; // drag your Tablet GameObject's UIDocument here

    private void Start()
    {
        StartCoroutine(WireUpPanel());
    }

    private System.Collections.IEnumerator WireUpPanel()
    {
        while (uiDocument == null || uiDocument.rootVisualElement == null || uiDocument.rootVisualElement.panel == null)
        {
            yield return null;
        }

        IPanel panel = uiDocument.rootVisualElement.panel;

        var panelRaycaster = GetComponent<PanelRaycaster>();
        if (panelRaycaster != null)
            panelRaycaster.panel = panel;

        var panelEventHandler = GetComponent<PanelEventHandler>();
        if (panelEventHandler != null)
            panelEventHandler.panel = panel;
    }
}