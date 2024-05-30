using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */

public class ScifiDoor : MonoBehaviour
{
    [Header("Updates Scifi door positions based on Valve.cs script")]
    public Transform leftDoor;
    public Transform rightDoor;

    public void UpdatePerc(float percentage)
    {
        float negPerc = percentage * -1;
        leftDoor.localPosition = new Vector3(negPerc, 0, 0);
        rightDoor.localPosition = new Vector3(percentage, 0, 0);
    }
}
