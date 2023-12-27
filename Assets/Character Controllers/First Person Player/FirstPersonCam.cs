//from tutorial: https://www.youtube.com/watch?v=f473C43s8nE&ab_channel=Dave%2FGameDevelopment

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCam : MonoBehaviour
{
    [SerializeField] private float xSensitivity = 250, ySensitivity = 250;

    //[SerializeField] private float rotationDeadZone;
    [SerializeField] private Transform orientation;


    private float xRotation, yRotation;

    public float minViewAngle = -45f;
    public float maxViewAngle = 80f;

    private ToggleCursorLock toggleCursor;

    private void Start()
    {
        toggleCursor.ToggleCursorMode();
    }

    private void Update()
    {
        //Get Mouse Input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * xSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * ySensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, minViewAngle, maxViewAngle);

        //Rotate Cam & Orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        //if ((yRotation > 0f && yRotation > rotationDeadZone) || (yRotation < 0f && yRotation < -rotationDeadZone))
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
