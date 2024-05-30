using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */

public class Lightswitch : MonoBehaviour {
    [Header("Lightswitch that toggles light")]
    private bool turnedOn;
    private bool canInteract = true;
    [Tooltip("0 switch up, 1 switch down")]
    public AudioClip[] clips;
    [Tooltip("Light AudioSource")]
    public AudioSource Source;
    [Tooltip("LightSwitch Renderer")]
    public Renderer rend;
    [Tooltip("Rotations of switch mesh 0 - off, 1 - on")]
    public Vector3[] switchRotations = new[] { new Vector3(-100, 0, 0), new Vector3(-77, 0, 0) };
    private Color originColor;
    private bool over = false;
    [Tooltip("Color when hovered over (looked at)")]
    public Color targetColor = Color.yellow;
    private GameObject MainCam;
    private Interact InteractionScript;
    [Tooltip("Turn Light On, Turn Light Off")]
    public string[] prompts = { "Turn Light On", "Turn Light Off" };
    [Tooltip("The Light")]
    public Light lightSource;
    [Tooltip("Light Renderer, the bulb, must have emissive color not black!")]
    public Renderer lightRend;
    
    void Start()
    {
        MainCam = GameObject.FindWithTag("MainCamera");
        if (MainCam == null)
        {
            MainCam = GameObject.FindObjectOfType<Camera>().gameObject;
        }
        InteractionScript = MainCam.GetComponent<Interact>();
        originColor = rend.material.color;
        if (lightSource.isActiveAndEnabled)
        {
            turnedOn = true;
            rend.gameObject.transform.localRotation = Quaternion.Euler(switchRotations[1]);
            lightRend.material.EnableKeyword("_EMISSION");
        }
        else
        {
            turnedOn = false;
            rend.gameObject.transform.localRotation = Quaternion.Euler(switchRotations[0]);
            lightRend.material.DisableKeyword("_EMISSION");
        }
    }

    public void Hovering()
    {
        over = true;
        StartCoroutine(Fadeout());
        if (!turnedOn)
            InteractionScript.message = prompts[0];
        else if (turnedOn)
            InteractionScript.message = prompts[1];
    }

    public void Interacting()
    {
        if (canInteract)
        {
            if (!turnedOn)
            {
                Source.clip = clips[0];
                InteractionScript.message = prompts[1];
                StartCoroutine(ToggleLight());
                rend.gameObject.transform.localRotation = Quaternion.Euler(switchRotations[1]);
            }
            else if (turnedOn)
            {
                Source.clip = clips[1];
                InteractionScript.message = prompts[0];
                StartCoroutine(ToggleLight());
                rend.gameObject.transform.localRotation = Quaternion.Euler(switchRotations[0]);
            }
            Source.Play();
            turnedOn = !turnedOn;
            canInteract = false;
        }
    }

    void FixedUpdate()
    {
        if (over)
        {
            rend.material.color = Color.Lerp(rend.material.color, targetColor, Time.deltaTime * 4);
        }
        else if (!over)
        {
            rend.material.color = Color.Lerp(rend.material.color, originColor, Time.deltaTime * 2);
        }
    }

    private IEnumerator ToggleLight()
    {
        yield return new WaitForSeconds(.2f);
        lightSource.enabled = !lightSource.enabled;
        if (lightSource.enabled)
        {
            lightRend.material.EnableKeyword("_EMISSION");
        }
        else if (!lightSource.enabled)
        {
            lightRend.material.DisableKeyword("_EMISSION");
        }
        canInteract = true;
    }

    private IEnumerator Fadeout()
    {
        yield return new WaitForSeconds(1);
        over = false;
    }
}
