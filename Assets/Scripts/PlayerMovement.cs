using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private float inputX = 0;
    private float inputY = 0;
    [Header("Movement Vars")]
    [Tooltip("Walk Speed, 3 is default")]
    public float walkSpeed = 3.0f;
    [Tooltip("Crouch Speed, 1.5 is default")]
    public float crouchSpeed = 1.5f;
    [Tooltip("Run Speed, double walk speed default")]
    public float runSpeed = 6.0f;
    [Tooltip("If true, diagonal speed (when strafing + moving forward or back) can't exceed normal move speed; otherwise it's about 1.4 times faster")]
    public bool limitDiagonalSpeed = true;
    // If checked, the run key toggles between running and walking. Otherwise player runs if the key is held down and walks otherwise
    // There must be a button set up in the Input Manager called "Run"
    [Tooltip("Toggle or Hold Run button")]
    public bool toggleRun = false;
    [HideInInspector]
    public bool running = false;
    [Tooltip("Jump Speed, default 8")]
    public float jumpSpeed = 8.0f;
    [Tooltip("Gravity, default 20 quick, 9.8 platformy")]
    public float defGravity = 20.0f;
    // If the player ends up on a slope which is at least the Slope Limit as set on the character controller, then he will slide down
    [Tooltip("Will Slide over slope limit in controller y/n")]
    public bool slideWhenOverSlopeLimit = false;
    // If checked and the player is on an object tagged "Slide", he will slide down it regardless of the slope limit
    [Tooltip("Will Slide over objects tagged Slide")]
    public bool slideOnTaggedObjects = true;
    [Tooltip("Slide Speed, default 10")]
    public float slideSpeed = 10.0f;
    [Tooltip("If checked, then the player can change direction while in the air")]
    public bool airControl = false;

    // Units that player can fall before a falling damage function is run. To disable, type "infinity" in the inspector
    [Tooltip("Units that player can fall before damage function is run, default 10")]
    public float fallingDamageThreshold = 10.0f;
    [Tooltip("Multiplier for fall damage based on fall height, 1.85 is default")]
    public float fallMultiplier = 1.85f;

    
    // Small amounts of this results in bumping when walking down slopes, but large amounts results in falling too fast
    private float antiBumpFactor = .75f;
    // Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
    private int antiBunnyHopFactor = 1;
    [HideInInspector]
    public Vector3 moveDirection = Vector3.zero;
    private bool grounded = false;
    private CharacterController controller;
    private Transform myTransform;
    private float speed;
    private RaycastHit hit;
    private float fallStartLevel;
    private bool falling;
    private float slideLimit;
    private float rayDistance;
    private Vector3 contactPoint;
    private bool playerControl = false;
    private int jumpTimer;
    private GameObject mainCam;



    [HideInInspector]
    public bool crouching = false;
    [Tooltip("Toggle or hold crouch")]
    public bool toggleCrouch = false;
    [Tooltip("AudioSource at Hips for crouching")]
    public AudioSource CrouchSource;
    [Tooltip("AudioSource at Feet for footsteps")]
    public AudioSource footstepSource;
    [Tooltip("AudioSource at Feet for ladderclimb, set loop, add sfx")]
    public AudioSource ladderClimbSource;
    [Tooltip("AudioSource at shins/hips for sliding, set loop, add sfx")]
    public AudioSource slideSource;
    [Tooltip("0 footsteps, 1 crouch, 2 get up")]
    public AudioClip[] clips;//0 footsteps, 1 crouch, 2 get up
    [Tooltip("AudioSource at feet for trampoline sfx, no loop")]
    public AudioSource trampSource;
    [Tooltip("Trampoline clips")]
    public AudioClip[] trampClips;
    //private Vector3 headStartPos;
    //private Vector3 headCrouchPos = new Vector3(0, -.25f, 0);
    private bool crouchSoundPlayed = false;
    private bool crouchSoundPlayed2 = false;
    [HideInInspector]
    public bool moving = false;
    [Tooltip("Player -> NeckJoint (w Animation) -> MainCamera")]
    public GameObject neckJoint;
    [Tooltip("Headbob Anim, legacy, loop, (.6 -> .65 -> .6) in ~.35 sec")]
    public Animation headbobAnim;
    //if disabled, disables movement entirely
    private bool isWorking = true;
    private float climbSpeed = 0, gravity = 0;
    private bool onLadder = false;
    private bool onPlatform = false;
    private float defSlopeLimit = 45;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        myTransform = transform;
        speed = walkSpeed;
        rayDistance = controller.height * .5f + controller.radius;
        slideLimit = controller.slopeLimit - .1f;
        jumpTimer = antiBunnyHopFactor;
        //headStartPos = neckJoint.transform.localPosition;
        footstepSource.loop = true;
        gravity = defGravity;
        mainCam = GameObject.FindWithTag("MainCamera");
        if(mainCam == null)
        {
            mainCam = FindObjectOfType<Camera>().gameObject;
        }
        if (ladderClimbSource != null)
        {
            ladderClimbSource.loop = true;
        }
        if(slideSource != null)
        {
            slideSource.loop = true;
        }
        defSlopeLimit = controller.slopeLimit;
    }

    void FixedUpdate()
    {
        if (isWorking)
        {
            // If both horizontal and vertical are used simultaneously, limit speed (if allowed), so the total doesn't exceed normal move speed
            float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f && limitDiagonalSpeed) ? .7071f : 1.0f;

            float maxRot = 1.5f; //Max neg and pos strafe rotation value 1.5f default
            float rate = 2.0f; //Rate of change for Lerp
            float currentAngle = neckJoint.transform.rotation.eulerAngles.y;

            if (inputX != 0 || inputY != 0)
            {
                moving = true;
            }
            else
            {
                moving = false;
            }

            if (moving && grounded)
            {
                headbobAnim.Play("An_Headbob");
            }
            else
            {
                headbobAnim.Stop();
            }

            if (!running)
            {
                if (!crouching)
                    headbobAnim["An_Headbob"].speed = 1.0f;
                else if (crouching)
                    headbobAnim["An_Headbob"].speed = -.5f;
                footstepSource.pitch = 1.05f;
                neckJoint.transform.rotation = Quaternion.Lerp(neckJoint.transform.rotation, Quaternion.Euler(inputY * maxRot * 0.1f, currentAngle, -inputX * maxRot * 0.5f), Time.deltaTime * rate);
            }

            else if (running)
            {
                headbobAnim["An_Headbob"].speed = 1.5f;
                footstepSource.pitch = 1.5f;
                neckJoint.transform.rotation = Quaternion.Lerp(neckJoint.transform.rotation, Quaternion.Euler(maxRot * 2, currentAngle, 0), Time.deltaTime * rate);
            }

            if (!onLadder)
            {
                // Apply gravity
                moveDirection.y -= gravity * Time.deltaTime;

                // Move the controller, and set grounded true or false depending on whether we're standing on something
                grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
                Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - controller.height / 2 + .1f, transform.position.z), transform.TransformDirection(Vector3.forward) * .65f, Color.blue);
            }
            else
            {
                controller.Move(new Vector3(0, climbSpeed, 3) * Time.deltaTime);
                Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - controller.height / 2 + .1f, transform.position.z), transform.TransformDirection(Vector3.forward) * .65f, Color.green);
            }

            if (grounded)
            {
                bool sliding = false;
                // See if surface immediately below should be slid down. We use this normally rather than a ControllerColliderHit point,
                // because that interferes with step climbing amongst other annoyances
                if (Physics.Raycast(myTransform.position, -Vector3.up, out hit, rayDistance))
                {
                    if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
                        sliding = true;
                }
                // However, just straight down from the center can fail when on steep slopes
                // So if the above raycast didn't catch anything, raycast down from the stored ControllerColliderHit point instead
                else
                {
                    Physics.Raycast(contactPoint + Vector3.up, -Vector3.up, out hit);
                    if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
                        sliding = true;
                }

                // If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
                if (falling)
                {
                    falling = false;
                    if (myTransform.position.y < fallStartLevel - fallingDamageThreshold)
                    {
                        FallingDamageAlert(fallStartLevel - myTransform.position.y);
                    }
                }

                if (speed > walkSpeed && moving)
                    running = true;
                else
                    running = false;
                
                // If sliding (and it's allowed), or if we're on an object tagged "Slide", get a vector pointing down the slope we're on
                if ((sliding && slideWhenOverSlopeLimit) || (slideOnTaggedObjects && hit.collider.tag == "Slide"))
                {
                    Vector3 hitNormal = hit.normal;
                    moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                    Vector3.OrthoNormalize(ref hitNormal, ref moveDirection);
                    moveDirection *= slideSpeed;
                    playerControl = false;
                    if((slideOnTaggedObjects && hit.collider.tag == "Slide"))
                    {
                        if (!slideSource.isPlaying)
                        {
                            slideSource.Play();
                        }
                    }
                    else
                    {
                        if (slideSource.isPlaying)
                        {
                            slideSource.Pause();
                        }
                    }
                }
                // Otherwise recalculate moveDirection directly from axes, adding a bit of -y to avoid bumping down inclines
                else
                {
                    moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
                    moveDirection = myTransform.TransformDirection(moveDirection) * speed;
                    playerControl = true;
                    if (slideSource.isPlaying)
                    {
                        slideSource.Pause();
                    }
                }

                //check if on trampoline surface
                if (hit.collider.tag == "Trampoline")
                {
                    if(controller.velocity.y < -5)
                    {
                        float upwardsVel = controller.velocity.y * -1;
                        moveDirection.y = upwardsVel;
                        if (!trampSource.isPlaying)
                        {
                            trampSource.clip = trampClips[Random.Range(0, trampClips.Length)];
                            trampSource.pitch = Random.Range(.8f, 1.2f);
                        }
                        trampSource.Play();
                    }
                }

                //check if on elevator pad surface
                if (hit.collider.tag == "MovingPlatform")
                {
                    if (transform.parent != hit.collider.gameObject)
                    {
                        transform.parent = hit.collider.transform;
                        onPlatform = true;
                    }
                }
                else
                {
                    transform.parent = null;
                    onPlatform = false;
                }

                // Jump! But only if the jump button has been released and player has been grounded for a given number of frames
                if (!Input.GetButton("Jump"))
                    jumpTimer++;
                else if (jumpTimer >= antiBunnyHopFactor)
                {
                    moveDirection.y = jumpSpeed;
                    jumpTimer = 0;
                }

                if (!onLadder)
                {
                    if (inputX != 0 || inputY != 0)
                    {
                        if (!footstepSource.isPlaying)
                        {
                            footstepSource.clip = clips[0];
                            footstepSource.Play();
                        }
                    }
                    else if (inputX == 0 || inputY == 0)
                    {
                        footstepSource.Stop();
                    }
                }
                else
                {
                    footstepSource.Stop();
                }
            }
            else
            {
                //not grounded, feet aren't touching ground so stop footsteps sound
                footstepSource.Stop();

                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.Normalize(controller.velocity)), out hit, Mathf.Infinity, 1))
                {
                    //Debug.Log("Hit dist: " + hit.distance + " vs fallDist: " + fallStartLevel * fallMultiplier);
                    if (hit.distance >= fallStartLevel * fallMultiplier && transform.TransformDirection(Vector3.Normalize(controller.velocity)).y <= 0)
                    {
                        // The character is far from the ground and our velocity on the y axis is negative -> we are falling
                        falling = true;
                    }
                    else
                    {
                        // The character is about to reach the ground so we can start the landing phase
                        falling = false;
                    }
                }
                // If we stepped over a cliff or something, set the height at which we started falling
                if (!falling)
                {
                    falling = true;
                    fallStartLevel = myTransform.position.y;
                }

                // If air control is allowed, check movement but don't touch the y component
                if (airControl && playerControl)
                {
                    moveDirection.x = inputX * speed * inputModifyFactor;
                    moveDirection.z = inputY * speed * inputModifyFactor;
                    moveDirection = myTransform.TransformDirection(moveDirection);
                }
            }

            if (crouching)
            {
                controller.height = Mathf.Lerp(controller.height, 0.65f, Time.deltaTime * 3);
                controller.center = Vector3.Lerp(controller.center, new Vector3(0, .4f, 0), Time.deltaTime * 3);
                //neckJoint.transform.localPosition = Vector3.Lerp(neckJoint.transform.localPosition, headCrouchPos, Time.deltaTime * 3/* * smooth*/);
                if (speed > crouchSpeed)
                    speed = crouchSpeed;
            }
            else if (!crouching)
            {
                controller.height = Mathf.Lerp(controller.height, 1.65f, Time.deltaTime * 6);
                controller.center = Vector3.Lerp(controller.center, new Vector3(0, 0, 0), Time.deltaTime * 6);
                //neckJoint.transform.localPosition = Vector3.Lerp(neckJoint.transform.localPosition, headStartPos, Time.deltaTime  * 2/* * smooth*/);
                // If running isn't on a toggle, then use the appropriate speed depending on whether the run button is down
            }
        }
    }

    void Update()
    {
        if (isWorking)
        {
            //we adjust the slope limit to 90 so the slope code doesn't fight the elevator code
            if (onPlatform)
            {
                controller.slopeLimit = 90;
            }
            else
            {
                controller.slopeLimit = defSlopeLimit;
            }

            if (!onLadder)
            {
                inputX = Input.GetAxis("Horizontal");
            }
            else
            {
                inputX = 0;
            }
            inputY = Input.GetAxis("Vertical");
            // If the run button is set to toggle, then switch between walk/run speed. (We use Update for this...
            // FixedUpdate is a poor place to use GetButtonDown, since it doesn't necessarily run every frame and can miss the event)
            if (!crouching && grounded)
            {
                if (toggleRun)
                {
                    if (Input.GetButtonDown("Run"))
                    {
                        speed = (speed == walkSpeed ? runSpeed : walkSpeed);
                    }
                }
                else
                {
                    speed = Input.GetButton("Run") ? runSpeed : walkSpeed;
                }
            }
            if (toggleCrouch)
            {
                if (Input.GetButtonDown("Crouch"))
                {

                    if (crouching)
                    {
                        if (!crouchSoundPlayed)
                        {
                            CrouchSource.clip = clips[2]; //get up
                            CrouchSource.Play();
                            crouchSoundPlayed2 = false;
                            crouchSoundPlayed = true;
                        }
                        crouching = false;
                    }
                    else if (!crouching)
                    {
                        if (!crouchSoundPlayed2)
                        {
                            crouchSoundPlayed = false;
                            CrouchSource.clip = clips[1]; //crouch
                            CrouchSource.Play();
                            crouchSoundPlayed2 = true;
                        }
                        crouching = true;
                    }
                }
            }
            else if (!toggleCrouch)
            {
                if (Input.GetButton("Crouch"))
                {
                    crouching = true;
                }
                else
                {
                    crouching = false;
                }
            }

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(transform.position.x,transform.position.y-controller.height/2 +.1f, transform.position.z), transform.TransformDirection(Vector3.forward), out hit, .65f))
            {
                if (hit.collider.tag == "Ladder")
                {
                    onLadder = true;
                    gravity = 0;
                    float topY = hit.collider.bounds.max.y;
                    if (transform.position.y < topY)
                    {
                        inputY = 0;
                        //use camera pitch as input
                        climbSpeed = Mathf.Sin(mainCam.transform.localRotation.x) * -3;
                        //-1.8 to 1.8
                        if(climbSpeed > .5f || climbSpeed < -.5f)
                        {
                            if (!ladderClimbSource.isPlaying)
                            {
                                ladderClimbSource.Play();
                            }
                        }
                        ladderClimbSource.pitch = Mathf.Clamp(Mathf.Abs(climbSpeed), -2, 2);
                    }
                }
                else
                {
                    onLadder = false;
                    gravity = defGravity;
                    if (ladderClimbSource.isPlaying)
                    {
                        ladderClimbSource.Stop();
                    }
                }
            }
            else
            {
                onLadder = false;
                gravity = defGravity;
                if (ladderClimbSource.isPlaying)
                {
                    ladderClimbSource.Stop();
                }
            }
        }
    }

    // Store point that we're in contact with for use in FixedUpdate if needed
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        contactPoint = hit.point;
    }

    // If falling damage occured, this is where you process that damage
    // have HP and remove it based on the distance fallen, add sound effects, death screen, etc.
    void FallingDamageAlert(float fallDistance)
    {
        print("Ouch! Fell " + fallDistance + " units!");
    }

    //Setter function for isWorking bool, for pausing movement, i.e, note inspection, etc.
    public void SetWorking(bool workingState)
    {
        isWorking = workingState;
        //if player is running and simultaneously reads a page, the footsteps and headbob wont loop
        if (!isWorking)
        {
            headbobAnim.Stop();
            footstepSource.Stop();
        }
    }
}
