using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaBombTargetAcquisition : MonoBehaviour
{
    //The distance to check before we assign our target
    public float proximityCheck;

    //Private variables
    private NovaBombScript nova;

    private void Awake()
    {
        //Reference our nova bomb script
        nova = GetComponentInParent<NovaBombScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Do our checks before assigning a target
        if (other.tag == "Enemy" && !nova.target)
        {
            //If we have enabled proximity check
            if(proximityCheck > 0)
            {
                //Calculate the distance between both transforms
                var distance = Vector3.Distance(other.transform.position, transform.position);
                //If distance is bigger than our proximity check we assign this collider's transform as target
                if(distance > proximityCheck)
                {
                    AssignTarget(other.transform);
                }
            }
            else
            {
                //If we dont have proximity check enabled then just assign this collider's transform as target
                AssignTarget(other.transform);
            }
        }
    }

    void AssignTarget(Transform target)
    {
        //Assign the target
        nova.target = target;
        //Get the current nova bomb speed
        var magnitude = nova.rb.velocity.magnitude;
        //Calculate the direction on which to direct our nova bomb
        Vector3 dir = nova.target.position - transform.position;
        //Reset the rigidbody velocity
        nova.rb.velocity = Vector3.zero;
        //Add a force towards our target
        nova.rb.AddForce(dir.normalized * magnitude, ForceMode.VelocityChange);
    }
}
