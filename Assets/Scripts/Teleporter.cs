using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */

public class Teleporter : MonoBehaviour
{
    [Header("Teleports player OnTriggerEnter")]
    [Tooltip("Other teleport pad")]
    public Transform teleportDestination;

    // Update is called once per frame
    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Interactable")
        {
            coll.transform.parent = null;
            //teleport while correcting height (so you don't teleport inside or under the floor)
            coll.transform.position = new Vector3(teleportDestination.position.x, teleportDestination.position.y + coll.bounds.max.y, teleportDestination.position.z);
            coll.attachedRigidbody.isKinematic = false;
        }
        else
        {
            if(coll.tag == "Player")
            {
                //disable character controller, otherwise it would be overridenssss
                coll.GetComponent<CharacterController>().enabled = false;
                //teleport while correcting height (so you don't teleport inside or under the floor)
                coll.transform.position = new Vector3(teleportDestination.position.x, teleportDestination.position.y + coll.bounds.max.y, teleportDestination.position.z);
                coll.GetComponent<CharacterController>().enabled = true;
            }
        }
    }
}
