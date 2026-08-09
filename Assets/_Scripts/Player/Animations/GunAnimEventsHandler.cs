using UnityEngine;

public class GunAnimEventsHandler : AnimationEventHandlerBase
{
    [SerializeField] Transform mag;
    public void OnMagShrink()
    {
        mag.localScale = Vector3.zero;
    }
    public void OnRevertMagShrink()
    {
        mag.localScale = Vector3.one;
    }
}
