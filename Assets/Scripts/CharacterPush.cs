using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */
public class CharacterPush : MonoBehaviour {

    // this script pushes all rigidbodies that the character touches
    float pushPower = 5.0f;
    float weight = 6.0f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        Vector3 force;

        // no rigidbody
        if (body == null || body.isKinematic) {
            return;
        }

        // We use gravity and weight to push things down, we use
        // our velocity and push power to push things other directions
        if (hit.moveDirection.y < -0.3f)
        {
            force = new Vector3(0, -0.5f, 0) * 10 * weight;
        }
        else
        {
            force = hit.controller.velocity * pushPower;
        }

        // Apply the push
        body.AddForceAtPosition(force, hit.point);
    }
}
