using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeflectShieldController : MonoBehaviour {

    GameObject deflectionSphere;

    bool deflectingEnabledForTesting = true;

	// Use this for initialization
	void Start () {
        deflectionSphere = transform.GetChild(0).gameObject;
        if (deflectingEnabledForTesting)
            StartCoroutine(alternateEveryFewSeconds());
        else
            deflectionSphere.SetActive(false);
    }

    IEnumerator alternateEveryFewSeconds()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5f);
            deflectionSphere.SetActive(!deflectionSphere.activeInHierarchy);
        }
    }

    public bool DeflectingEnabled()
    {
        return deflectingEnabledForTesting && deflectionSphere.activeInHierarchy;
    }
}
