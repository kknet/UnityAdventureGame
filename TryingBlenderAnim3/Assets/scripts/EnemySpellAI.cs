using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpellAI : MonoBehaviour {

    GameObject deflectionSphere;
    Animator devAnimator;
    Animator enemyAnimator;

    bool deflectingEnabledForTesting = false;
    bool spellEnabledForTesting = false;

	// Use this for initialization
	void Start () {
        enemyAnimator = GetComponent<Animator>();
        devAnimator = DevMain.Player.GetComponent<Animator>();
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

            if (deflectionSphere.activeInHierarchy)
                deflectionSphere.SetActive(false);
            else
            {
                while (!canPutShieldUp())
                    yield return new WaitForEndOfFrame();

                deflectionSphere.SetActive(true);
            }
        }
    }

    bool canPutShieldUp()
    {
        AnimatorStateInfo devInfo = devAnimator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo enemyInfo = enemyAnimator.GetCurrentAnimatorStateInfo(0);

        bool enemyInDefensiveAnimation = enemyInfo.IsTag("enemyRun");
        bool devCanAvoidHittingShield = !devInfo.IsTag("attacking") || 
            (devInfo.IsTag("attacking") && devInfo.normalizedTime < 0.3f);


        return devCanAvoidHittingShield && enemyInDefensiveAnimation;
    }

    public bool DeflectingEnabled()
    {
        return deflectingEnabledForTesting && deflectionSphere.activeInHierarchy;
    }
}
