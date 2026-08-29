using UnityEngine;

public class Entity : MonoBehaviour, IDamageable
{
    [SerializeField] private float weight;
    private float forceDecel = 20f;
    private float forceDecelDecel = 1f;
    private Vector3 forceDir;
    private float forceAmount = 0;
    public virtual void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        Debug.Log("Damaged with ");
    }
    public virtual void TakeDamageWithImpact(float damageAmount, float impactAmount, Vector3 source)
    {
        /*Vector3 forceDirection = transform.position - source;
        forceDirection.Normalize();
        forceDirection *= impactAmount;*/
        Vector3 direction = transform.position - source;
        direction.y = 0;
        AddForce(direction, impactAmount);
    }
    public virtual void AddForce(Vector3 direction, float forceAmount)
    {
        this.forceDir = direction.normalized;
        this.forceAmount = forceAmount;
    }
    protected void FrameUpdate()
    {
        if (forceAmount <= 0.05f)
            return;
        if(weight > 100f)
            return;
        float deltaTime = Time.deltaTime;
        transform.position += (forceDir * forceAmount) * deltaTime;
        forceAmount = Mathf.Clamp(forceAmount-(forceDecel *(weight/2) * deltaTime), 0, 10000);
        forceDecel += forceDecelDecel*deltaTime;
    }
}
