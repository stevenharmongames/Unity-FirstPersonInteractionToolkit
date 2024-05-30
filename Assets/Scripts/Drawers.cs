using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */

[RequireComponent(typeof(AudioSource))]
public class Drawers : MonoBehaviour {
    [Header("For filing cabinets/desk drawers/sliding doors")] 
    private bool moved;
    [Tooltip("Audio clips - 0 open drawer, 1 close drawer")]
    public AudioClip[] clips;
    private AudioSource Source;
    private Vector3 originPos;
    [Tooltip("Sets the direction of slide, use empty GameObject")]
    public Transform endPos;//x .45 away max
    private Vector3 dir;
    private float timer;
    private bool canInteract = true;//only can interact when not in mid-movement
    [Tooltip("Move distance in sliding direction, .25f is default")]
    public float moveDist = 0.25f;//.25 by default
    [Tooltip("Move speed in sliding direction, 2 is default")]
    public float moveSpd = 2;//2 by default
    private Renderer[] renderers;
    private Color[] originColor;
    private bool over = false;
    [Tooltip("Target hover over color, default yellow")]
    public Color targetColor = Color.yellow;
    private GameObject MainCam;
    private Interact InteractionScript;
    [Tooltip("Hint text 0 Open Drawer, 1 Close Drawer")]
    public string[] prompts = {"Open Drawer", "Close Drawer" };//0 open drawer, 1 close drawer

    // Use this for initialization
    void Start () {
        Source = gameObject.GetComponent<AudioSource>();
        originPos = transform.position;
        renderers = GetComponentsInChildren<Renderer>();
        originColor = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originColor[i] = renderers[i].material.color;
        }
        MainCam = GameObject.FindWithTag("MainCamera");
        if(MainCam == null)
        {
            MainCam = GameObject.FindObjectOfType<Camera>().gameObject;
        }
        InteractionScript = MainCam.GetComponent<Interact>();
    }
	
    public void Hovering()
    {
        over = true;
        StartCoroutine(Fadeout());
        if(!moved)
            InteractionScript.message = prompts[0];
        else if(moved)
            InteractionScript.message = prompts[1];
    }

	public void Interacting() {
        if (canInteract)
        {
            moved = !moved;
            dir = (endPos.position - transform.position).normalized;
            timer = 0;
            if (!moved)
            {
                Source.clip = clips[0];
                Source.Play();
                InteractionScript.message = prompts[0];
            }
            else if (moved)
            {
                Source.clip = clips[1];
                Source.Play();
                InteractionScript.message = prompts[1];
            }
            StartCoroutine(Reset());
            canInteract = false;
        }
    }

    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (!moved)
        {
            if (/*Vector3.Distance(transform.position, originPos) > .01f ||*/ timer < moveDist)
            {
                transform.position += dir * Time.deltaTime * moveSpd;
            }
        }
        else if (moved)
        {
            if (timer < moveDist)
            {
                transform.position += dir * Time.deltaTime * moveSpd;
            }
        }

        if (over)
        {
            foreach (Renderer rend in renderers)
            {
                rend.material.color = Color.Lerp(rend.material.color, targetColor, Time.deltaTime * 4);
            }
        }
        else if (!over)
        {
            int i = 0;
            foreach (Renderer rend in renderers)
            {
                rend.material.color = Color.Lerp(rend.material.color, originColor[i], Time.deltaTime * 2);
                i++;
            }
        }
    }

    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(moveDist);
        canInteract = true;
    }

    private IEnumerator Fadeout()
    {
        yield return new WaitForSeconds(1);
        over = false;
    }
}
