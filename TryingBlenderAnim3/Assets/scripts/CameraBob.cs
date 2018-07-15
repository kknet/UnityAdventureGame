using System;
using System.Collections;
using UnityEngine;


public class CameraBob : MonoBehaviour
{
    float maxBobSpeed = 12f;
    float maxBobAmount = 0.03f;
    float timer = Mathf.PI / 2;
    Vector3 camPos;

    public Vector3 AddCameraBob(Vector3 restPosition, float curForwardAmount)
    {
        float bobAmount = maxBobAmount * curForwardAmount;
        float bobSpeed = maxBobSpeed * curForwardAmount;
        camPos = Camera.main.transform.position;

        if (curForwardAmount > 0f) //moving
        {
            timer += bobSpeed * Time.fixedDeltaTime;

            Vector3 newPosition = restPosition + (Camera.main.transform.right * Mathf.Cos(timer) * bobAmount) +
                (Camera.main.transform.up * Mathf.Abs((Mathf.Sin(timer) * bobAmount)));
            camPos = newPosition;
        }
        else
        {
            timer = Mathf.PI / 2; //reinitialize
            camPos = restPosition;
        }

        if (timer > Mathf.PI * 2) //completed a full cycle on the unit circle. Reset to 0 to avoid bloated values.
            timer = 0;

        return camPos;
    }
}