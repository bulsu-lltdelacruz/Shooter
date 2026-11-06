using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PsuedoInventory : MonoBehaviour
{
    List<Key> keysObtained;
    VisualElement root;
    private void Start()
    {
       keysObtained = new List<Key>();   
    }
    public void AddKey(Key key)
    {
        keysObtained.Add(key);
        root.Q<VisualElement>(key.keyType.ToString()).style.visibility = Visibility.Visible;
    }
    public void Initialize(UIDocument HUD)
    {
        root = HUD.rootVisualElement;
    }
    public bool CheckKeyComplete(KeyType[] completeArray)
    {
        if (completeArray == null || completeArray.Length == 0) return true;
        if (keysObtained == null || keysObtained.Count == 0) return false;

        foreach (KeyType requiredType in completeArray)
        {
            bool hasThisKey = false;

            foreach (Key ownedKey in keysObtained)
            {
                if (ownedKey.keyType == requiredType)
                {
                    hasThisKey = true;
                    break; 
                }
            }
            if (!hasThisKey) return false;
        }
        return true;
    }

}
