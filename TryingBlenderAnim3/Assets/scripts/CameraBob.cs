using System;
using System.Collections;
using UnityEngine;


public class CameraBob : MonoBehaviour
{
    float maxBobSpeed = 12f;
    float maxBobAmount = 0.03f;
    float timer = Mathf.PI / 2;
    float bobAmount, bobSpeed; 
    Vector3 camPos;

    public Vector3 AddCameraBob(Vector3 restPosition, float curForwardAmount, bool cameraShouldBob)
    {
        camPos = Camera.main.transform.position;
        updateBobAmountAndSpeed(curForwardAmount, cameraShouldBob);

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

    void updateBobAmountAndSpeed(float curForwardAmount, bool cameraShouldBob)
    {
        if (cameraShouldBob)
        {
            bobAmount = Mathf.Lerp(bobAmount, maxBobAmount * curForwardAmount, 2f * Time.fixedDeltaTime);
            bobSpeed = Mathf.Lerp(bobSpeed, maxBobSpeed * curForwardAmount, 2f * Time.fixedDeltaTime);
        }
        else
        {
            bobAmount = Mathf.Lerp(bobAmount, 0f, 2f * Time.fixedDeltaTime);
            bobSpeed = Mathf.Lerp(bobSpeed, 0f, 2f * Time.fixedDeltaTime);
        }
    }
}