using System.Collections;
using UnityEngine;

public class BulletTrailHelper : MonoBehaviour
{
    [SerializeField] private float timeShown = 0.5f;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float trailSpeed;
    private Vector3 direction;

    private float lifeTimer;
    private Vector3 finalPos;

    public void SetFinalPos(Vector3 targetPosition)
    {
        finalPos = targetPosition;
        direction = finalPos - transform.position;
        direction.Normalize();
    }

  
    private bool hasHit;

    private void OnEnable()
    {
        hasHit = false;
        if (trailRenderer != null)
            trailRenderer.emitting = true;

        if (trailRenderer != null)
            trailRenderer.Clear();
    }

    private void Update()
    {
        if (hasHit) return;

        lifeTimer += Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, finalPos, trailSpeed * Time.deltaTime);

        if ((finalPos - transform.position).magnitude <= .5f)
        {
            OnHit();
        }
    }

    private void OnHit()
    {
        hasHit = true;
        if (trailRenderer != null)
            trailRenderer.emitting = false; // stops new points, existing trail still fades normally

        float delay = trailRenderer != null ? Mathf.Max(timeShown, trailRenderer.time) : timeShown;
        Invoke(nameof(ReturnToPool), delay);
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

}
