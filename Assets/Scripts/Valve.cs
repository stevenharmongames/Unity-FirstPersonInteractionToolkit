using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */

[RequireComponent(typeof(AudioSource))]
public class Valve : MonoBehaviour
{
    private Renderer rend; //valve's renderer, mesh
    private Color originColor;
    private bool over = false;
    private GameObject MainCam;
    private Interact InteractionScript;
    [Tooltip("Color when hovered over (looked at)")]
    public Color targetColor = Color.yellow;
    private float rotDir = 0, rotVal = 0, rotPerc = 0, rotSpd = -0.001f;
    [Tooltip("Max time for rotating until goal reached")]
    public float maxRotTime = 3;
    [Tooltip("Valve starts in closed extreme of rotation")]
    public bool startClosed = true;
    [Tooltip("ScifiDoor.cs or any other script listening for UpdatePerc function")]
    public GameObject listenerScript;
    private AudioSource valveSource;

    // Start is called before the first frame update
    void Start()
    {
        MainCam = GameObject.FindWithTag("MainCamera");
        if (MainCam == null)
        {
            MainCam = GameObject.FindObjectOfType<Camera>().gameObject;
        }
        InteractionScript = MainCam.GetComponent<Interact>();
        rend = gameObject.GetComponent<Renderer>();
        originColor = rend.material.color;
        valveSource = GetComponent<AudioSource>();
        valveSource.loop = true;
    }

    public void Hovering(Vector3 rayHitPoint)
    {
        over = true;
        //check which side the raycast hit, but with the parent that doesn't rotate
        Vector3 relativePosition = transform.parent.InverseTransformPoint(rayHitPoint);


        if (relativePosition.x > 0)
        {
            //Debug.Log("on left side");
            rotDir = 1;
            valveSource.pitch = .95f;
            InteractionScript.message = "Turn Valve Left";
        }
        else
        {
            //Debug.Log("on right side");
            rotDir = -1;
            valveSource.pitch = 1;
            InteractionScript.message = "Turn Valve Right";
        }
        rotSpd = 90;
    }

    public void UnHover()
    {
        over = false;
    }

    bool IsClose(float a, float b, float absTol = 0.001f)
    {
        return Mathf.Abs(a - b) <= absTol;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (over)
        {
            rend.material.color = Color.Lerp(rend.material.color, targetColor, Time.deltaTime * 4);

            if (Input.GetButton("Interact"))
            {
                float step = rotSpd * rotDir * Time.deltaTime;
                if (!IsClose(rotVal,maxRotTime) && !IsClose(rotVal,0))
                {
                    transform.Rotate(Vector3.up, step);
                    if (!valveSource.isPlaying)
                    {
                        valveSource.Play();
                    }
                }
                else
                {
                    if (valveSource.isPlaying)
                    {
                        valveSource.Pause();
                    }
                }
 

                rotVal = Mathf.Clamp(rotVal + Time.deltaTime * rotDir, 0, maxRotTime);
                rotPerc = (rotVal / maxRotTime);
                listenerScript.SendMessage("UpdatePerc", rotPerc, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                if (valveSource.isPlaying)
                {
                    valveSource.Pause();
                }
            }
        }
        else if (!over)
        {
            rend.material.color = Color.Lerp(rend.material.color, originColor, Time.deltaTime * 2);
            if (valveSource.isPlaying)
            {
                valveSource.Stop();
            }
        }
    }
}
