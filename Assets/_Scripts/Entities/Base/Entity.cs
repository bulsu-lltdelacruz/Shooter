using System;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Entity : MonoBehaviour, IDamageable
{
    [Header("prefabs")]
    public GameObject parryPrefabs;
    [Header("Base Stats")]
    [SerializeField] protected float maxHP;
    [SerializeField] protected float maxPoise;
    [SerializeField][Tooltip("Poise Resistance Outside Parry")] protected float poiseRes;
    [SerializeField][Tooltip("Damage Resistance Outside Parry and Stun")] protected float damageRes;
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
    protected Action OnParryEvt;
    public bool isStunned;
    public bool isParried;

    // Inside Entity.cs:
    // Change from explicit to a standard public virtual property
    public virtual EffectController effectController { get; protected set; } = new();

    public virtual void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
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
    public virtual void TakeDamageWithImpact(Vector3 source, DamageType damageType, Vector3 hitPoint, Quaternion hitRotation)
    {
        
        if (currentPoise <= 0 && !damageType.Equals(DamageType.MELEE) && !isStunned)
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
       // this.forceDir = direction.normalized;
        //this.forceAmount = forceAmount;
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

    public GameObject GetParryPrefab()
    {
        return parryPrefabs;
    }

    public void TakeDamageWithImpact(Vector3 source, DamageType damageType)
    {
        
    }
    protected float CalculateHealthDamage(float damage, DamageType damageType)
    {
        switch(damageType)
        {
            case DamageType.HANDGUN:
                return (isStunned) ? damage: damage*0.7f;
            case DamageType.SHOTGUN:
                return (isStunned) ? damage * 2f : damage*0.4f;
            case DamageType.CROSSGUN:
                return (isStunned) ? damage* 0.7f: damage;
            case DamageType.MELEE:
                return damage;
            default:
                return damage;
        }
    }
    protected float CalculatePoiseDamage(float poiseDamage, DamageType damageType)
    {
        /*switch (damageType)
        {
            case DamageType.HANDGUN:
                return (isStunned) ? poiseDamage : poiseDamage * 0.7f;
            case DamageType.SHOTGUN:
                return (isStunned) ? poiseDamage * 2f : poiseDamage * 0.4f;
            case DamageType.CROSSGUN:
                return (isStunned) ? poiseDamage * 0.7f : poiseDamage;
            case DamageType.MELEE:
                return poiseDamage;
            default:
                return poiseDamage;
        }*/
        return poiseDamage;
    }
}
