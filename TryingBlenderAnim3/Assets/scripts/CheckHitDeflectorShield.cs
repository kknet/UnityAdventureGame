using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckHitDeflectorShield : MonoBehaviour {


    Animator animator;
    Rigidbody rb;
    Collider hurtCollider;
    TargetMatching targetMatching;
    DevCombat devCombat;
    EnemySpellAI enemyDeflect;
    private bool reasonDeflecting;

    [HideInInspector] public bool deflectingEnabled;

    private void Awake()
    {
        devCombat = GetComponent<DevCombat>();
        targetMatching = GetComponent<TargetMatching>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        hurtCollider = GetComponent<Collider>();
        reasonDeflecting = false;
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

    public void EnemyLandHit()
    {
        if (!targetMatching.recoveringFromHit)
        {
            reasonDeflecting = false;
            StartCoroutine(animatorSpeedChanges());
        }
    }

    private void FixedUpdate()
    {
        enemyDeflect = devCombat.CurrentEnemy.GetComponent<EnemySpellAI>();
        deflectingEnabled = enemyDeflect.DeflectingEnabled();

        if (deflectingEnabled && !targetMatching.recoveringFromHit && devCombat.attacking()/* && !animator.GetBool("Dodge")*/)
        {
            if (CheckHit())
            {
                reasonDeflecting = true;
                StartCoroutine(animatorSpeedChanges());
            }
        }
    }

    IEnumerator animatorSpeedChanges()
    {
        Debug.Log("hit defector");
        animator.SetBool("Dodge", false);
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

        Vector3 direction;
        if(reasonDeflecting)
            direction = Quaternion.AngleAxis(angle, transform.up) * -transform.forward.normalized;
        else
            direction = Quaternion.AngleAxis(angle, transform.up) * 
                (transform.position - devCombat.CurrentEnemy.transform.position).normalized; //otherwise it's from an enemy landing

        while (tt < 150f)
        {
            Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + (30f * direction), Color.magenta);
            transform.forward = Vector3.MoveTowards(transform.forward, -direction, Time.fixedDeltaTime * 20f);

            if (animator.speed > 0.1f)
                transform.Translate(direction * multiplier, Space.World);
            tt += 1f;
            multiplier = Mathf.MoveTowards(multiplier, 0f, Time.fixedDeltaTime * multiplier * 1.5f);
            yield return new WaitForFixedUpdate();
        }

        targetMatching.recoveringFromHit = false;
        //animator.applyRootMotion = true;
    }

}
