using UnityEngine;

public class EffectDestroy : MonoBehaviour
{
    [SerializeField] float timeShown;
    float deltaTime;
    void Update()
    {
        deltaTime+=Time.deltaTime;
        if(deltaTime >= timeShown)
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
            deltaTime = 0;
        }    
    }
}
