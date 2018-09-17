using System.Collections;
using UnityEngine;

// Ugly script, just used for demo/testing purposes!
public class TargetMatching : MonoBehaviour
{
    CharacterController characterController;
    DevCombat devCombat;
    Animator animator;
    Rigidbody rb;

    Vector3 desiredPos;
    Quaternion correctRot;
    int attackIndex;
    bool shouldMatchTarget;
    bool recoveringFromHit;

    //private float[] matchEndTimes = {
    //    0.66f,
    //    0.8f,
    //    0.5f
    //};

    private float[] matchEndTimes = {
        0.45f,
        0.45f,
        0.472f
    };

    private float[] attackDistances = {
        0.806f,
        0.951f,
        1.067f
    };

    public float[] desiredDistances = {
        1.5f,
        1.5f,
        2f
    };

    public float[] margins = {
        4f,
        6f,
        6f
    };

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        devCombat = GetComponent<DevCombat>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        shouldMatchTarget = false;
    }

    public void MatchTargetUpdate()
    {
        if (recoveringFromHit) return;
        if (!shouldMatchTarget) return;
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("attacking") || !animator.GetBool("doAttack") || animator.GetBool("Dodge"))
        {
            Debug.LogWarning("NOT");
            return;
        }

        Debug.LogWarning("DOING");


        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > matchEndTimes[attackIndex])
            return;

        animator.MatchTarget(desiredPos, correctRot, AvatarTarget.Root,
            new MatchTargetWeightMask(new Vector3(1, 1, 1), 0),
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime /*0.2f*/,
            matchEndTimes[attackIndex]
            );
    }

    public void SetUpMatchTarget()
    {
        if (recoveringFromHit)
        {
            shouldMatchTarget = false;
            return;
        }

        Vector3 enemyPos = devCombat.CurrentEnemy.transform.position;
        Vector3 curPos = transform.position;
        Vector3 dir = enemyPos - curPos;
        dir.Normalize();

        attackIndex = animator.GetInteger("quickAttack") - 1;
        correctRot = Quaternion.LookRotation(characterController.currentEnemyLookDirection());
        desiredPos = enemyPos - (desiredDistances[attackIndex] * dir);
        shouldMatchTarget = InAttackingRange(curPos, desiredPos, attackIndex);

        if (!shouldMatchTarget)
        {
            shouldMatchTarget = true;
            desiredPos = curPos + (margins[attackIndex] * dir);
            //desiredPos = curPos + (0.5f * Vector3.Distance(desiredPos, curPos) * dir);
            //desiredPos = enemyPos - ((margins[attackIndex] + desiredDistances[attackIndex]) * dir);
        }

        Debug.LogWarning("SET UP MT: " + (shouldMatchTarget ? "GOOD" : "BAD"));
    }

    private bool InAttackingRange(Vector3 curPos, Vector3 desiredPos, int attackIndex)
    {
        float distance = Vector3.Distance(desiredPos, curPos);
        return distance - margins[attackIndex] < attackDistances[attackIndex];
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag.Equals("DefectorShield"))
        {
            Debug.Log("hit defector");
            shouldMatchTarget = false;
            animator.InterruptMatchTarget(false);
            StartCoroutine(animatorSpeedChanges());
        }
    }

    IEnumerator animatorSpeedChanges()
    {
        animator.speed = 0f;
        while (animator.speed > 0f)
        {
            animator.speed -= 0.05f;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.05f);

        StartCoroutine(translateFall());

        while (animator.speed < 1f)
        {
            animator.speed += 0.05f;
            yield return null;
        }
    }

    public void FinishRecoveringFromHit()
    {
        recoveringFromHit = false;
    }


    IEnumerator translateFall()
    {
        //animator.applyRootMotion = false;
        recoveringFromHit = true;

        if (Random.Range(0f, 1f) > 0.5f)
            animator.SetFloat("Mirrored", 1.0f);
        else
            animator.SetFloat("Mirrored", 0.0f);
        animator.Play("Fall Back");
        animator.SetBool("doAttack", false);

        float tt = 0f;
        float multiplier = 0.025f;
        float decrement = multiplier / 100f;

        float angle = Random.Range(-30f, -70f);
        //if (devCombat.mirroredAttack()) angle *= -1f;
        Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * -transform.forward.normalized;

        while (tt < 70f)
        {
            Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + (30f * direction), Color.magenta);

            if (animator.speed < 0.6f)
                transform.Translate(direction * (multiplier + 0.25f), Space.World);
            else
                transform.Translate(direction * multiplier, Space.World);

            tt += 1f;
            multiplier = Mathf.Max(multiplier - decrement, 0.01f);
            yield return null;
        }

        recoveringFromHit = false;
        //animator.applyRootMotion = true;
    }
}