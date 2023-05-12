using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBumper : MonoBehaviour
{
    public float pushForceHorizontal = 2f;
    public float pushForceLift = .5f;

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 force = other.transform.position - transform.position;
            force.y = 0f;
            force.Normalize();
            force *= pushForceHorizontal;
            force.y = pushForceLift;
            rb.AddForce(force, ForceMode.VelocityChange);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // Possibility: Enemies grabbing items (for that, should implement enemy bumper).
        //IInteractable itr = other.GetComponent<WorldItem>();
        //if (itr.Method == InteractionMethod.ByBump)
        //{
        //        itr.Interact();
        //}
    }
}
