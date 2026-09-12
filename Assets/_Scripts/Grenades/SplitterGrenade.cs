using UnityEngine;

public class SplitterGrenade : BaseGrenade, IDamageable
{
    [SerializeField] private int reboundCount;
    private bool hasBeenHit = false;
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (hasBeenHit)
            return;
        hasBeenHit = true;
        //fix later with level spawner  since its slow as shit
        Enemy[] enemies = FindObjectsByType<Enemy>();
        int currentCount = 0;
        foreach (Enemy enemy in enemies)
        {
            player.weaponController.FireShotAutoTarget(enemy.gameObject, hitPoint);
            currentCount++;
            if (currentCount >= reboundCount)
                break;
        }
        ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.GameObjects);
    }
    public void TakeDamageWithImpact(float amount, float impactAmount, Vector3 source)
    {
        if (hasBeenHit)
            return;
        hasBeenHit = true;
        //fix later with level spawner  since its slow as shit
        Enemy[] enemies = FindObjectsByType<Enemy>();
        int currentCount = 0;
        foreach (Enemy enemy in enemies)
        {
            player.weaponController.FireShotAutoTarget(enemy.gameObject, transform.position);
            currentCount++;
            if (currentCount >= reboundCount)
                break;
        }
        ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.GameObjects);
    }
    private void OnEnable()
    {
        OnEnableBase();
        hasBeenHit = false;
    }
    private void OnDisable()
    {
        OnDisableBase();
        hasBeenHit = false;
    }
    private void OnDestroy()
    {
        OnDestroyBase();
        hasBeenHit = false;
    }

    public void TakeDamageWithImpact(Vector3 source)
    {

    }

    public void AddForceAndDamageAmount(float damage, float forceAmount)
    {

    }

    public void AddForce(Vector3 direction, float forceAmount)
    {

    }
}
