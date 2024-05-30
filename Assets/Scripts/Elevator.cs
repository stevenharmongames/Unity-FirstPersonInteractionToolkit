using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */

public class Elevator : MonoBehaviour
{
    private bool movingDown = false;
    private bool movingUp = false;
    public float minHeight = 0.025f;
    public float maxHeight = 8.723f;

    public void CallUp()
    {
        movingUp = true;
        movingDown = false;
    }

    public void CallDown()
    {
        movingUp = false;
        movingDown = true;
    }

    void FixedUpdate()
    {
        if (movingDown)
        {
            if (transform.position.y >= minHeight)
            {
                Vector3 tempPos = new Vector3(transform.position.x, transform.position.y - Time.deltaTime, transform.position.z);
                transform.position = tempPos;
            }
        }
        if(movingUp)
        {
            if (transform.position.y <= maxHeight)
            {
                Vector3 tempPos = new Vector3(transform.position.x, transform.position.y + Time.deltaTime, transform.position.z);
                transform.position = tempPos;
            }
        }
    }
}
