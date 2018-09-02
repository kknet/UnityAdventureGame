using UnityEngine;

// Ugly script, just used for demo/testing purposes!
public class TargetMatching : MonoBehaviour
{
    CharacterController characterController;
    DevCombat devCombat;
    Animator animator;

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

    private float margin = 1f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        devCombat = GetComponent<DevCombat>();
        animator = GetComponent<Animator>();
    }

    public void MatchTargetIfPossible()
    {
        Quaternion correctRot = Quaternion.LookRotation(characterController.CurrentEnemyLookDirection());

        Vector3 enemyPos = devCombat.CurrentEnemy.transform.position;
        Vector3 curPos = transform.position;
        Vector3 dir = enemyPos - curPos;
        dir.Normalize();
        Vector3 desiredPos = enemyPos - (1.5f * dir);
        int attackIndex = animator.GetInteger("quickAttack") - 1;

        if (InAttackingRange(curPos, desiredPos, attackIndex))
        {
            animator.MatchTarget(desiredPos, correctRot, AvatarTarget.Root,
                new MatchTargetWeightMask(new Vector3(1, 1, 1), 0), // zero weight for the position, so it doesn't get adjusted at all.  one for rotation = 100% adjustment
                                                                    /*animator.GetCurrentAnimatorStateInfo(0).normalizedTime*/ 0.2f, // we start adjustments at the current time, "right now"
                matchEndTimes[attackIndex]); //so I want the adjustments to complete at this point in the animation (100%)
        }
    }

    private bool InAttackingRange(Vector3 curPos, Vector3 desiredPos, int attackIndex)
    {
        float distance = Vector3.Distance(desiredPos, curPos);
        return distance - margin < attackDistances[attackIndex];
    }
}