using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */
public class MouseLook : MonoBehaviour
{
    [Tooltip("Place this on Root for MouseX, MainCamera for MouseY")]
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    [Tooltip("Default sensitivity is 3")]
    public float sensitivityX = 3f;
    [Tooltip("Default sensitivity is 3")]
    public float sensitivityY = 3f;
    [Tooltip("Default min is -360")]
    public float minimumX = -360f;
    [Tooltip("Default max is 360")]
    public float maximumX = 360f;
    [Tooltip("Default min is -85")]
    public float minimumY = -85f;
    [Tooltip("Default max is 90")]
    public float maximumY = 90f;
    private float rotationX = 0f;
    private float rotationY = 0f;
    Quaternion originalRotation;
    [Tooltip("locks cursor to center")]
    public bool lockCursor = true;
    [Tooltip("script enabled")]
    public bool working = true;
    [Tooltip("if cutscene mode, player can still look but it'll pull back automatically")]
    public bool cutSceneMode = false;

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

    void Start()
    {
        originalRotation = transform.localRotation;
        if (lockCursor)
        {
            #if UNITY_STANDALONE
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            #endif
            #if UNITY_EDITOR
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            #endif
        }
    }

    void LateUpdate()
    {
        if (lockCursor)
        {
            #if UNITY_STANDALONE
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            #endif
            #if UNITY_EDITOR
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            #endif
        }

        if (working)
        {
            if (cutSceneMode)
            {
                rotationX += Input.GetAxis("Mouse X") * sensitivityX * .5f;
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY * .5f;
                rotationX = Mathf.Lerp(rotationX, transform.root.localRotation.x, Time.deltaTime * 3);
                rotationY = Mathf.Lerp(rotationY, transform.root.localRotation.y, Time.deltaTime * 3);

                rotationX = ClampAngle(rotationY, minimumX * .8f, maximumX * .8f);
                rotationY = ClampAngle(rotationY, minimumY * .8f, maximumY * .8f);
                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
                transform.localRotation = originalRotation * xQuaternion * yQuaternion;
            }
            else if (!cutSceneMode)
            {
                if (axes == RotationAxes.MouseXAndY)
                {
                    // Read the mouse input axis
                    rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                    rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

                    rotationX = ClampAngle(rotationX, minimumX, maximumX);
                    rotationY = ClampAngle(rotationY, minimumY, maximumY);
                    Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                    Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
                    transform.localRotation = originalRotation * xQuaternion * yQuaternion;
                }
                else if (axes == RotationAxes.MouseX)
                {
                    rotationX += Input.GetAxis("Mouse X") * sensitivityX;

                    rotationX = ClampAngle(rotationX, minimumX, maximumX);
                    Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                    transform.localRotation = originalRotation * xQuaternion;
                }
                else
                {
                    rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

                    rotationY = ClampAngle(rotationY, minimumY, maximumY);
                    Quaternion yQuaternion = Quaternion.AngleAxis(-rotationY, Vector3.right);
                    transform.localRotation = originalRotation * yQuaternion;
                }
            }
        }
    }
}