using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    private int groundContacts = 0;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ground"))
        {
            PlayerStatus.Instance.isGrounded = true;
            groundContacts++;
        }
            
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            groundContacts--;
            PlayerStatus.Instance.isGrounded = groundContacts > 0;
        }

    }
}
