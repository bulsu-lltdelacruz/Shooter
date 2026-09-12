using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Sherbert.Framework.Generic;

public class Prefabs : MonoBehaviour
{
    public GameObject impaactPrefab;
    public static Prefabs Instance;
    private void Start()
    {
        if(Instance == null)
            Instance = this;
    }
    public GameObject GetImpactPrefab()
    {
        return impaactPrefab;
    }
}
public enum E_Prefabs
{
    DASHIMPACT
}