using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    private Vector3 shakeOffset;

    private float maxMagnitude = 0.06f;
    private const float minMagnitude = 0.01f;
    private const float duration = 0.3f;
    
    private float magnitude;
    private float startTime;

    private void Start()
    {
        shakeOffset = Vector3.zero;
        magnitude = maxMagnitude;
    }

    public Vector3 AddCameraShake(Vector3 normalPosition)
    {
        float timeElapsed = Time.fixedTime - startTime;
        if (timeElapsed < duration)
        {
            float timeFraction = Mathf.Max(0f, duration - timeElapsed) / duration;
            timeFraction = Mathf.Clamp(timeFraction, 0f, 1f);
            magnitude = Mathf.Lerp(minMagnitude, maxMagnitude, timeFraction);

            Vector3 shakeOffset = shakeOffset = Random.insideUnitCircle * magnitude;
            return normalPosition + shakeOffset;
        }
        else
        {
            return normalPosition;
        }
    }

    public void TriggerCameraShake(float setMaxMagnitudeTo = 0.06f)
    {
        maxMagnitude = setMaxMagnitudeTo;
        startTime = Time.fixedTime;
    }
    
    //private IEnumerator shakeInternal()
    //{
    //    float startTime = Time.time;
    //    while (Time.time - startTime < duration)
    //    {
    //        Vector3 shakeOffset = new Vector2(Random.Range(-1f, 1f) * magnitude, Random.Range(-1f, 1f) * magnitude);
    //        transform.position += shakeOffset;
    //        yield return null;
    //    }

    //    while (Vector3.Distance(transform.position, normalPosition) > 0.001f)
    //    {
    //        transform.position = Vector3.MoveTowards(transform.position, normalPosition, Time.deltaTime * 10f);
    //        yield return null;
    //    }

    //    transform.position = new Vector3(0, 0, -10);
    //    shaking = false;
    //}

}
