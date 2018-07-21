using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyCheckHit : MonoBehaviour
{
    private string[] reactAnimations = {
        "standing_react_large_from_right",
        "standing_react_large_from_left",
        "React from Right and Move Back",
        "React from Right and Move Back"
    };

    public Collider hurtCollider;
    Animator animator;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
	}

    void Update()
    {
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);
        if (animState.IsTag("enemyRun"))
        {
            if (CheckHit())
            {
                HurtReaction();
                StartCoroutine(animatorSpeedChanges());
            }
        }
    }

    // Update is called once per frame
    void HurtReaction ()
    {
        int idx = Random.Range(0, 3);
        animator.Play(reactAnimations[idx]);
	}

    bool CheckHit()
    {
        Collider[] cols = Physics.OverlapBox(hurtCollider.bounds.center, hurtCollider.bounds.extents,
            hurtCollider.transform.rotation, LayerMask.GetMask("AttackBox"));
        foreach (Collider other in cols)
        {
            if (other.Equals(hurtCollider))
                continue;

            Debug.Log("Hurt: " + other.transform.root.name);
            return true;
        }
        return false;
    }

    IEnumerator animatorSpeedChanges()
    {
        Animator devAnimator = DevMain.Player.GetComponent<Animator>();
        animator.speed = 0f;
        devAnimator.speed = 0f;
        while (animator.speed > 0f)
        {
            animator.speed -= 0.1f;
            devAnimator.speed -= 0.1f;
            yield return null;
        }


        yield return new WaitForSecondsRealtime(0.2f);
        while (animator.speed < 1f)
        {
            animator.speed += 0.1f;
            devAnimator.speed += 0.1f;
            yield return null;
        }
    }

}
