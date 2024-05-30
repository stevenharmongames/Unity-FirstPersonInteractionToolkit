using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */
public class ElevatorButton : MonoBehaviour
{
    private Renderer buttonRend;
    private Color originColor;
    private bool over = false;
    [Tooltip("Color when hovered over (looked at)")]
    public Color targetColor = Color.yellow;
    private GameObject MainCam;
    private Interact InteractionScript;
    [Tooltip("Hover prompt - 0 Call Elevator Up, 1 Call Elevator Down")]
    public string[] prompts = { "Call Elevator Up", "Call Elevator Down" };
    [Tooltip("Is the button that calls elevator up")]
    public bool isButtonUp = false;
    private bool pushingIn = false;
    private Vector3 pushedInPos = Vector3.zero;
    private Vector3 pushedOutPos = Vector3.zero;
    [Tooltip("Elevator script reference")]
    public Elevator elevatorScript;

    // Start is called before the first frame update
    void Start()
    {
        MainCam = GameObject.FindWithTag("MainCamera");
        if (MainCam == null)
        {
            MainCam = GameObject.FindObjectOfType<Camera>().gameObject;
        }
        InteractionScript = MainCam.GetComponent<Interact>();
        buttonRend = GetComponent<Renderer>();
        originColor = buttonRend.material.color;
        pushedOutPos = transform.localPosition;
        pushedInPos = new Vector3(pushedOutPos.x, pushedOutPos.y, pushedOutPos.z - .015f /* -.015f is the distance of the button press-in */);
    }

    public void Hovering(Vector3 rayHitPoint)
    {
        over = true;
        if (isButtonUp)
        {
            InteractionScript.message = prompts[0];
        }
        else
        {
            InteractionScript.message = prompts[1];
        }
        StartCoroutine(Fadeout());
    }

    public void Interacting()
    {
        pushingIn = true;
    }

    void FixedUpdate()
    {
        if (over)
        {
            buttonRend.material.color = Color.Lerp(buttonRend.material.color, targetColor, Time.deltaTime * 4);
        }
        else if (!over)
        {
            buttonRend.material.color = Color.Lerp(buttonRend.material.color, originColor, Time.deltaTime * 2);
        }
        if (pushingIn)
        {
            
            if (!IsClose(pushedInPos.z, transform.localPosition.z))
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, pushedInPos, Time.deltaTime * .35f);
            }
            else
            {
                //call elevator here
                if (isButtonUp)
                {
                    elevatorScript.CallUp();
                }
                else
                {
                    elevatorScript.CallDown();
                }
                pushingIn = false;
            }
        }
        else
        {
            if (!IsClose(pushedOutPos.z, transform.localPosition.z))
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, pushedOutPos, Time.deltaTime * .1f);
            }
        }
    }

    bool IsClose(float a, float b, float absTol = 0.001f)
    {
        return Mathf.Abs(a - b) <= absTol;
    }

    private IEnumerator Fadeout()
    {
        yield return new WaitForSeconds(1);
        over = false;
    }
}
