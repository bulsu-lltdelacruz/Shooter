using UnityEngine;

public class BulletTrailHelper : MonoBehaviour
{
    [SerializeField] private float timeShown;
    private WaitForSeconds waitInstruction;
    private Coroutine disableCoroutine;

    private void Awake()
    {
        waitInstruction = new WaitForSeconds(timeShown);
    }

    private void OnEnable()
    {
        if (disableCoroutine != null)
            StopCoroutine(disableCoroutine);

        disableCoroutine = StartCoroutine(DisableAfterTime());
    }

    private void OnDisable()
    {
        StopCoroutine(nameof(DisableAfterTime));
        disableCoroutine = null;
    }

    private System.Collections.IEnumerator DisableAfterTime()
    {
        yield return waitInstruction;
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}
