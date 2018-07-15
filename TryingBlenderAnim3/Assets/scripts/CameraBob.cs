using System;
using System.Collections;
using UnityEngine;


public class CameraBob : MonoBehaviour
{
    float transitionSpeed = 2f; //smooths out the transition from moving to not moving.
    float bobSpeed = 12f; //how quickly the player's head bobs.
    float maxBobAmount = 0.03f; //how dramatic the bob is. Increasing this in conjunction with bobSpeed gives a nice effect for sprinting.
    float timer = Mathf.PI / 2; //initialized as this value because this is where sin = 1. So, this will make the camera always start at the crest of the sin wave, simulating someone picking up their foot and starting to walk--you experience a bob upwards when you start walking as your foot pushes off the ground, the left and right bobs come as you walk.
    Vector3 camPos;

    public Vector3 AddCameraBob(Vector3 restPosition, float m_ForwardAmount)
    {
        float bobAmount = maxBobAmount * m_ForwardAmount;
        camPos = Camera.main.transform.position;

        if (m_ForwardAmount > 0f) //moving
        {
            timer += bobSpeed * Time.fixedDeltaTime;

            //use the timer value to set the position
            Vector3 newPosition = new Vector3(restPosition.x + Mathf.Cos(timer) * bobAmount, restPosition.y +  Mathf.Abs((Mathf.Sin(timer) * bobAmount)), restPosition.z); //abs val of y for a parabolic path
            camPos = newPosition;
        }
        else
        {
            timer = Mathf.PI / 2; //reinitialize

            //Vector3 newPosition = new Vector3(Mathf.Lerp(camPos.x, restPosition.x, transitionSpeed * Time.deltaTime), 
            //    Mathf.Lerp(camPos.y, restPosition.y, transitionSpeed * Time.deltaTime), 
            //    Mathf.Lerp(camPos.z, restPosition.z, transitionSpeed * Time.deltaTime)); //transition smoothly from walking to stopping.
            //camPos = newPosition;

            camPos = restPosition;
        }

        if (timer > Mathf.PI * 2) //completed a full cycle on the unit circle. Reset to 0 to avoid bloated values.
            timer = 0;

        return camPos;
    }
}