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

    private float[] matchEndTimes = {
        0.66f,
        0.8f,
        0.5f
    };

    private float[] attackDistances = {
        0.806f,
        0.951f,
        1.067f
    };

    private float[] desiredDistances = {
        1f,
        1f,
        1.3f
    };

    private float margin = 1f;

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
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("attacking"))
        {
            Debug.LogWarning("NOT DOING MT: " + animator.tag);
            return;
        }

        Debug.LogWarning("DOING MT");


        animator.MatchTarget(desiredPos, correctRot, AvatarTarget.Root,
            new MatchTargetWeightMask(new Vector3(1, 1, 1), 0),
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime /*0.2f*/,
            Mathf.Max(matchEndTimes[attackIndex], animator.GetCurrentAnimatorStateInfo(0).normalizedTime)
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

        Debug.LogWarning("SET UP MT: " + (shouldMatchTarget ? "GOOD" : "BAD"));
    }

    private bool InAttackingRange(Vector3 curPos, Vector3 desiredPos, int attackIndex)
    {
        float distance = Vector3.Distance(desiredPos, curPos);
        return distance - margin < attackDistances[attackIndex];
    }
}