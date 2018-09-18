using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckHitDeflectorShield : MonoBehaviour {


    Animator animator;
    Rigidbody rb;
    Collider hurtCollider;
    TargetMatching targetMatching;

    bool deflectingEnabled = true;

    private void Awake()
    {
        targetMatching = GetComponent<TargetMatching>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        hurtCollider = GetComponent<Collider>();
    }


    private bool CheckHit()
    {
        Collider[] cols = Physics.OverlapBox(hurtCollider.bounds.center, hurtCollider.bounds.extents,
            hurtCollider.transform.rotation, LayerMask.GetMask("DeflectorShield"));
        foreach (Collider other in cols)
        {
            if (other.Equals(hurtCollider))
                continue;

            Debug.Log("Hurt: " + other.transform.root.name);
            return true;
        }
        return false;
    }

    private void FixedUpdate()
    {
        if (deflectingEnabled && !targetMatching.recoveringFromHit)
        {
            if (CheckHit())
            {
                StartCoroutine(animatorSpeedChanges());
            }
        }
    }

    IEnumerator animatorSpeedChanges()
    {
        //animator.applyRootMotion = false;
        Debug.Log("hit defector");
        targetMatching.shouldMatchTarget = false;
        animator.InterruptMatchTarget(false);
        targetMatching.recoveringFromHit = true;

        animator.speed = 0f;

        if (Random.Range(0f, 1f) > 0.5f)
            animator.SetFloat("Mirrored", 1.0f);
        else
            animator.SetFloat("Mirrored", 0.0f);
        animator.Play("Fall Back");
        animator.SetBool("doAttack", false);

        yield return new WaitForSecondsRealtime(0.05f);

        StartCoroutine(translateFall());

        while (animator.speed < 1f)
        {
            animator.speed += 0.03f;
            yield return new WaitForEndOfFrame();
        }

    }

    public void FinishRecoveringFromHit()
    {
        targetMatching.recoveringFromHit = false;
    }

    IEnumerator translateFall()
    {
        float tt = 0f;
        float multiplier = 0.1f;
        float decrement = multiplier / 100f;

        float angle = Random.Range(-1f, -10f);
        if (Random.Range(0f, 1f) > 0.5f) angle *= -1f;
        Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * -transform.forward.normalized;

        while (tt < 100f)
        {
            Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + (30f * direction), Color.magenta);

            float speed = multiplier + (0.25f * (1f - animator.speed));
            transform.Translate(direction * speed, Space.World);

            //if (animator.speed < 0.6f)
            //    transform.Translate(direction * (multiplier + 0.25f), Space.World);
            //else
            //    transform.Translate(direction * multiplier, Space.World);

            tt += 1f;
            multiplier = Mathf.Max(multiplier - decrement, 0.01f);
            yield return new WaitForFixedUpdate();
        }

        targetMatching.recoveringFromHit = false;
        //animator.applyRootMotion = true;
    }

}
