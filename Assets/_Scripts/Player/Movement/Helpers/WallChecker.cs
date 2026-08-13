using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WallChecker : MonoBehaviour
{
    LayerMask wallMask;
    private void Start()
    {
        wallMask = LayerMask.GetMask("Wall");
    }
    private void Update()
    {
        float rayLen = 1f;
        bool hitRightWall = Physics.Raycast(transform.position, transform.right, out RaycastHit hitRight, rayLen, wallMask);
        bool hitLeftWall = Physics.Raycast(transform.position, -transform.right, out RaycastHit hitLeft, rayLen, wallMask);

        if (hitRightWall || hitLeftWall)
        {

            PlayerStatus.Instance.isWithinWall = true;
            if (hitRightWall && Vector3.Dot(hitRight.normal, Vector3.up) >= 0)//either right or skightly upwards
            {
                PlayerStatus.Instance.wallNormal = hitRight.normal;
            }
            else if (hitLeftWall && Vector3.Dot(hitLeft.normal, Vector3.up) >= 0)
            {
                PlayerStatus.Instance.wallNormal = hitLeft.normal;
            }
        }
        else if(!PlayerStatus.Instance.isWallJumping) //kasi pag ganon tataas nanaman vector kapag naka jumop na palayo
        {
            PlayerStatus.Instance.isWithinWall = false;
            PlayerStatus.Instance.wallNormal = Vector3.up;
        }
        Debug.DrawRay(transform.position, transform.right * rayLen, Color.red, .1f);
        Debug.DrawRay(transform.position, -transform.right* rayLen, Color.red, .1f);
    }
    /*private int wallContacts = 0;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Wall"))
        {
            wallContacts++;
            PlayerStatus.Instance.isWithinWall = true;
            PlayerStatus.Instance.wall.Add(other.gameObject);
            
        }
            
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            wallContacts--;
            PlayerStatus.Instance.isWithinWall = wallContacts > 0;
            PlayerStatus.Instance.wall.Remove(other.gameObject);
        }

    }*/
}
