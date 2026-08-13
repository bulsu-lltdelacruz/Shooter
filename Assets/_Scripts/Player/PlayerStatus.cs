using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStatus:MonoBehaviour
{
    public static PlayerStatus Instance { get; private set; }
    public bool isJumping;
    public bool isWallJumping;//no need to check in inputs since  its only enabled when also isjumping
    public bool isGrounded;
    public bool isWithinWall;
    //public bool isDashing;
    //public bool isSlamming;
    public int maxJumps = 2;
    public Vector3 wallNormal;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

    }
    

}
