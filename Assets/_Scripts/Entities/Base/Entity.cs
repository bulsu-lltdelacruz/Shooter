using System;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Entity : MonoBehaviour, IDamageable
{
    [Header("Base Stats")]
    [SerializeField] protected float maxHP;
    [SerializeField] protected float maxPoise;
    protected float currentPoise;
    protected float currentHP;
    [SerializeField] private float weight;
    private float forceDecel = 20f;
    private float forceDecelDecel = 1f;
    private Vector3 forceDir;
    private float forceAmount = 0;
    protected float totalPelletDamage;
    protected float totalPoiseDamage;
    //Events
    protected Action OnStunEvt;
    public virtual void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        Debug.Log("Damaged with ");
    }
    public virtual void TakeDamageWithImpact(float damageAmount, float impactAmount, Vector3 source, DamageType damageType)
    {//DONT USER< FOR IMPACT JUST USE ADD IMPOACT THEN CALL THE METHOD BELOW
        /*Vector3 forceDirection = transform.position - source;
        forceDirection.Normalize();
        forceDirection *= impactAmount;*/
        Vector3 direction = transform.position - source;
        direction.y = 0;
        AddForce(direction, impactAmount);
    }
    public virtual void TakeDamageWithImpact(Vector3 source, DamageType damageType)
    {
        currentPoise = Mathf.Max(0, currentPoise - totalPoiseDamage);
        currentHP = Mathf.Max(0, currentHP - totalPelletDamage);
        if (currentPoise <= 0)
        {
            HitLag.Instance.SlowMo(.5f,.5f);
            Debug.Log("Stun");
            OnStunEvt?.Invoke();
        }
        totalPelletDamage = 0;
        totalPoiseDamage = 0;
        Vector3 direction = transform.position - source;
        direction.y = 0;
        AddForce(direction, forceAmount);
    }
    public virtual void AddForceAndDamageAmount(float damage, float forceAmount, float poiseDamage)
    {
        this.forceAmount += forceAmount;
        this.totalPelletDamage += damage;
        totalPoiseDamage+= poiseDamage;
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
    public virtual void Die(int multiplier)
    {
        GameObject healthDrop = Prefabs.Instance.healthDropPrefab;
        GameObject bulletDropPrefab = Prefabs.Instance.bulletDropPrefab;
        int dropNumber = (2* multiplier)+ UnityEngine.Random.Range(0,4);

        Vector3[] directions = MathStuff.GetRadialSpread(dropNumber, Vector3.up);
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject obj = null;
            if (i >= directions.Length/2)
                obj = ObjectPoolManager.SpawnObject(bulletDropPrefab, transform.position, Quaternion.LookRotation(directions[i]));
            else
                obj = ObjectPoolManager.SpawnObject(healthDrop, transform.position, Quaternion.LookRotation(directions[i]));
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            float randomForce = UnityEngine.Random.Range(1f, 2f);
            Vector3 forceDirection = (obj.transform.forward + Vector3.up * 0.3f).normalized;
            rb.AddForce(forceDirection * randomForce, ForceMode.Impulse);
        }



        EntityManager.Instance.UnRegisterEnemy(gameObject);
        Destroy(gameObject);
    }
    public virtual void Die()
    {
        GameObject dropPrefab = Prefabs.Instance.healthDropPrefab;
        int dropNumber = 2;
        Vector3[] directions = MathStuff.GetRadialSpread(dropNumber, Vector3.up);
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject obj = ObjectPoolManager.SpawnObject(dropPrefab, transform.position, Quaternion.LookRotation(directions[i]));
            Rigidbody rb = obj.GetComponent<Rigidbody>();

            Vector3 forceDirection = (obj.transform.forward + Vector3.up * 0.3f).normalized;
            rb.AddForce(forceDirection * 2f, ForceMode.Impulse);
        }



        EntityManager.Instance.UnRegisterEnemy(gameObject);
        Destroy(gameObject);
    }
    public virtual void Heal(float healAmount)
    {
        currentHP = Mathf.Clamp(currentHP + healAmount, 0, maxHP);
        Debug.Log($"{currentHP} from {healAmount} amount of healing");
    }
}
