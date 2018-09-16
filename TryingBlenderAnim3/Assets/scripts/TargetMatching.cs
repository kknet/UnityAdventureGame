using UnityEngine;

// Ugly script, just used for demo/testing purposes!
public class TargetMatching : MonoBehaviour
{
    CharacterController characterController;
    DevCombat devCombat;
    Animator animator;


    Vector3 desiredPos;
    Quaternion correctRot;
    int attackIndex;
    bool shouldMatchTarget;

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
        shouldMatchTarget = false;
    }

    public void MatchTargetUpdate()
    {
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
}