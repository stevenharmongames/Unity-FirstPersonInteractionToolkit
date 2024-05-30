using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */

public class Trampoline : MonoBehaviour
{
    private Vector3 newVelocity = Vector3.zero;
    private Renderer trampRend;
    private Color originColor;
    public Color targetColor = Color.yellow;

    void Start()
    {
        trampRend = GetComponent<Renderer>();
        originColor = trampRend.material.color;
    }

    private void OnTriggerEnter(Collider coll)
    {
        Rigidbody collRigid = coll.gameObject.GetComponent<Rigidbody>();
        Vector3 normalVel = collRigid.velocity.normalized;
        newVelocity = new Vector3(normalVel.x * .1f, .2f+ Mathf.Abs(normalVel.y)*1.3f, normalVel.z * .1f);
        if (collRigid != null)
        {
            
            collRigid.AddForce(newVelocity, ForceMode.Impulse);
        }
        trampRend.material.color = targetColor;
    }

    private void OnCollisionExit()
    {
        trampRend.material.color = originColor;    
    }
}
