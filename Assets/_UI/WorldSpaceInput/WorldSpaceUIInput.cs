using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class WorldSpaceUIInput : MonoBehaviour
{
    [Header("Raycast Setup")]
    [SerializeField] private Camera raycastCamera;       // usually your main/player camera
    [SerializeField] private Collider targetCollider;    // collider on the quad/mesh showing the panel's RenderTexture
    [SerializeField] private LayerMask uiLayerMask = ~0;
    [SerializeField] private float maxRayDistance = 10f;

    private UIDocument uiDocument;
    private PanelSettings panelSettings;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        panelSettings = uiDocument.panelSettings;

        if (panelSettings == null)
        {
            Debug.LogError("WorldSpaceUIInput: no PanelSettings assigned on UIDocument.");
            return;
        }

        if (panelSettings.targetTexture == null)
        {
            Debug.LogWarning("WorldSpaceUIInput: PanelSettings has no targetTexture — " +
                              "world-space UI needs Render Mode set to render into a RenderTexture.");
        }

        panelSettings.SetScreenToPanelSpaceFunction(ScreenToPanelSpace);
    }

    private void OnDisable()
    {
        if (panelSettings != null)
        {
            panelSettings.SetScreenToPanelSpaceFunction(null);
        }
    }

    // Called automatically by UI Toolkit's runtime input system every frame it
    // needs to resolve a screen-space pointer position into panel-space coords.
    private Vector2 ScreenToPanelSpace(Vector2 screenPosition)
    {
        if (raycastCamera == null || targetCollider == null || panelSettings.targetTexture == null)
            return InvalidPosition();

        Ray ray = raycastCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, uiLayerMask))
            return InvalidPosition();

        if (hit.collider != targetCollider)
            return InvalidPosition(); // hit something else in the world, not our UI quad

        Vector2 uv = hit.textureCoord;

        // UV space: (0,0) = bottom-left. Panel space: (0,0) = top-left. Flip Y.
        return new Vector2(
            uv.x * panelSettings.targetTexture.width,
            (1f - uv.y) * panelSettings.targetTexture.height
        );
    }

    // Returning NaN tells UI Toolkit "pointer is outside this panel" —
    // triggers proper PointerOut / PointerLeave events instead of a false hit.
    private static Vector2 InvalidPosition() => new Vector2(float.NaN, float.NaN);
}