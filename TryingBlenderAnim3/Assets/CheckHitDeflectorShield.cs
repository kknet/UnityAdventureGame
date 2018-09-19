using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckHitDeflectorShield : MonoBehaviour {


    Animator animator;
    Rigidbody rb;
    Collider hurtCollider;
    TargetMatching targetMatching;
    DevCombat devCombat;
    EnemyDeflectShieldController enemyDeflect;

    [HideInInspector] public bool deflectingEnabled;

    private void Awake()
    {
        devCombat = GetComponent<DevCombat>();
        targetMatching = GetComponent<TargetMatching>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        hurtCollider = GetComponent<Collider>();
    }


    private bool CheckHit()
    {
        if (!devCombat.attacking())
            return false;

        //if (animator.GetBool("Dodge"))
        //    return false;

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
        enemyDeflect = devCombat.CurrentEnemy.GetComponent<EnemyDeflectShieldController>();
        deflectingEnabled = enemyDeflect.DeflectingEnabled();

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
        animator.SetBool("doAttack", false);
        targetMatching.shouldMatchTarget = false;
        animator.InterruptMatchTarget(false);
        targetMatching.recoveringFromHit = true;

        animator.speed = 0f;

        yield return new WaitForSecondsRealtime(0.06f);

        animator.CrossFadeInFixedTime("Knocked Out", 0.1f);

        StartCoroutine(translateFall());

        while (animator.speed < 1f)
        {
            animator.speed += 0.05f;
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
        float multiplier = 0.3f;
        float decrement = multiplier / 30f;

        float angle = Random.Range(-1f, -10f);
        if (Random.Range(0f, 1f) > 0.5f) angle *= -1f;
        Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * -transform.forward.normalized;

        while (tt < 150f)
        {
            Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + (30f * direction), Color.magenta);

            float speed = multiplier/* + (0.1f * Mathf.Pow(1f - animator.speed, 2f))*/;

            if(animator.speed > 0.1f)
                transform.Translate(direction * speed, Space.World);

            //if (animator.speed < 0.6f)
            //    transform.Translate(direction * (multiplier + 0.25f), Space.World);
            //else
            //    transform.Translate(direction * multiplier, Space.World);

            tt += 1f;
            multiplier = Mathf.MoveTowards(multiplier, 0f, Time.fixedDeltaTime * multiplier * 1.5f);
            //multiplier = Mathf.Max(multiplier - decrement, 0.01f);
            yield return new WaitForFixedUpdate();
        }

        targetMatching.recoveringFromHit = false;
        //animator.applyRootMotion = true;
    }

}
