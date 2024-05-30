using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */
public class Door : MonoBehaviour
{
    [Header("Traditional Pull/Push door. Uses hinge joint")]
    [Tooltip("Rotation of the door closed")]
    public Vector3 closedRot = new Vector3(0, 0, 0);
    public Vector3 openRot = new Vector3(0, 100, 0);
    private Vector3 heading = Vector3.zero;
    private Vector3 storedRayHitPt = Vector3.zero;
    private float dot = 0;
    private bool isLocked = true;
    private GameObject playerObj;
    private Rigidbody doorRigid;
    [Tooltip("Door Renderer")]
    public Renderer doorRend;
    private Color originColor;
    private bool over = false;
    [Tooltip("Color when hovered over (looked at)")]
    public Color targetColor = Color.yellow;
    private GameObject MainCam;
    private Interact InteractionScript;
    [Tooltip("Hover prompt - 0 Open Door, 1 Close Door")]
    public string[] prompts = { "Open Door", "Close Door" };
    [Tooltip("Door AudioSource")]
    public AudioSource Source;
    [Tooltip("0 door swing out, 1 door swing in, 2 door shut")]
    public AudioClip[] clips;
    

    // Start is called before the first frame update
    void Start()
    {
        MainCam = GameObject.FindWithTag("MainCamera");
        if (MainCam == null)
        {
            MainCam = GameObject.FindObjectOfType<Camera>().gameObject;
        }
        InteractionScript = MainCam.GetComponent<Interact>();
        playerObj = GameObject.FindWithTag("Player");
        if(playerObj == null)
        {
            playerObj = GameObject.FindObjectOfType<PlayerMovement>().gameObject;
        }
        originColor = doorRend.material.color;
        doorRigid = GetComponent<Rigidbody>();
    }

    public void Hovering(Vector3 rayHitPoint)
    {
        over = true;
        StartCoroutine(Fadeout());
        if (isLocked)
        {
            InteractionScript.message = prompts[0];
        }
        else
        {
            InteractionScript.message = prompts[1];
        }
        storedRayHitPt = rayHitPoint;
    }

    public void Interacting()
    {
        doorRigid.isKinematic = false;
        heading = (playerObj.transform.position - transform.position).normalized;
        dot = Vector3.Dot(heading, transform.up);
        if(dot > 0)
        {
            Source.clip = clips[0];//swing open
            //so you don't lock yourself in
            if (isLocked)
            {
                doorRigid.AddForceAtPosition(transform.forward * -5, storedRayHitPt, ForceMode.Impulse);
            }
            else
            {
                doorRigid.AddForceAtPosition(heading * 5, storedRayHitPt, ForceMode.Impulse);
            }
        }
        else
        {
            Source.clip = clips[1];//swing closed
            doorRigid.AddForceAtPosition(heading * 5, storedRayHitPt, ForceMode.Impulse);
        }
        isLocked = false;
        Source.Stop();
        Source.pitch = Random.Range(0.7f, 1);
        Source.Play();
        
        StartCoroutine(IsClosed());
    }

    void FixedUpdate()
    {
        if (over)
        {
            doorRend.material.color = Color.Lerp(doorRend.material.color, targetColor, Time.deltaTime * 4);
        }
        else if (!over)
        {
            doorRend.material.color = Color.Lerp(doorRend.material.color, originColor, Time.deltaTime * 2);
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

    private IEnumerator IsClosed()
    {
        //wait a sec to see if door is closed (if so, lock it)
        yield return new WaitForSeconds(1);
        if (IsClose(Mathf.Abs(transform.localRotation.eulerAngles.y), closedRot.y, 1))
        {
            isLocked = true;
            doorRigid.isKinematic = true;
            doorRigid.rotation = Quaternion.Euler(closedRot);
            Source.Stop();
            Source.pitch = Random.Range(0.8f, 1.2f);
            Source.clip = clips[2];//Door Shut
            Source.Play();
        }
    }

}
