using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStatus:MonoBehaviour
{
    [SerializeField] UIDocument doc;
    private VisualElement root;
    public static PlayerStatus Instance { get; private set; }
    public bool isJumping;
    public bool isWallJumping;//no need to check in inputs since  its only enabled when also isjumping
    public bool isGrounded;
    public bool isWithinWall;
    //public bool isDashing;
    //public bool isSlamming;
    public int maxJumps = 1;
    public Vector3 wallNormal;
    public int spellCharge;
    public int maxSpellCharge = 3;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

    }
    private void Start()
    {
        root = doc.rootVisualElement;
        root.Q<Label>("spell-count").text = $"{spellCharge}/{maxSpellCharge}";
    }
    public void AddSpellCharge()
    {
        spellCharge = Mathf.Clamp(spellCharge+1, 0, 3);
        root.Q<Label>("spell-count").text = $"{spellCharge}/{maxSpellCharge}";
    }
    public void UseSpellCharge()
    {
        spellCharge = 0;
        root.Q<Label>("spell-count").text = $"{spellCharge}/{maxSpellCharge}";
    }

}
