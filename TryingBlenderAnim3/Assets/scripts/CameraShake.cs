using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private enum ShakeState
    {
        ToShake,
        ToNormal
    }

    private ShakeState state;
    private Vector3 shakeOffset;

    private float magnitude = 0.05f;

    private void Start()
    {
        shakeOffset = Vector3.zero;
        state = ShakeState.ToNormal;
    }

    public Vector3 AddCameraShake(Vector3 normalPosition)
    {
        if (state.Equals(ShakeState.ToShake))
        {
            Vector3 curPos = transform.position;
            curPos = Vector3.MoveTowards(curPos, normalPosition + shakeOffset, Time.fixedDeltaTime * 20f);

            if (Vector3.Distance(transform.position, normalPosition + shakeOffset) < 0.000001f)
                state = ShakeState.ToNormal;

            return curPos;
        }
        else
        {
            return normalPosition;
        }
    }

    public void TriggerCameraShake()
    {
        state = ShakeState.ToShake;
        //shakeOffset = new Vector2(0f, Random.Range(-1f, 1f) * magnitude);
        shakeOffset = Random.insideUnitSphere * magnitude;
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
