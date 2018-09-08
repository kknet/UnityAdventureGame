using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class EnemyCheckHit : MonoBehaviour
{
    public enum Direction
    {
        Forward,
        Backward,
        Left,
        Right
    }

    private string[] reactAnimations = {
        "Fall Forward",
        "Fall Back",
        "Fall Right",
        "Fall Left"
    };

    public Collider hurtCollider;
    DevCombat devCombat;
    Animator animator;
    Rigidbody rb;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        devCombat = DevMain.Player.GetComponent<DevCombat>();
    }

    void Update()
    {
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);
        if (animState.IsTag("enemyRun") && devCombat.canHit)
        {
            if (CheckHit())
            {
                Direction enemyFallDirection = DevToEnemyHitDirection();
                HurtReaction(enemyFallDirection);
                StartCoroutine(animatorSpeedChanges(enemyFallDirection));
            }
        }
    }

    //The direction that the enemy goes after being hit
    public Direction DevToEnemyHitDirection()
    {
        Vector3 enemyForward = devCombat.TestEnemy.transform.forward;
        Vector3 devForward = DevMain.Player.transform.forward;
        float angle = Vector3.SignedAngle(enemyForward, devForward, Vector3.up);
        float absAngle = Mathf.Abs(angle);

        //if (absAngle > 135f) return Direction.Backward;
        //if (absAngle < 45f) return Direction.Forward;
        //if (angle > 45f) return Direction.Right;
        //else return Direction.Left;

        if (absAngle > 90f) return Direction.Backward;
        else return Direction.Forward;
    }

    void HurtReaction (Direction enemyFallDirection)
    {
        //switch (enemyFallDirection)
        //{
        //    case Direction.Forward:
        //        animator.Play("Fall Forward");
        //        break;
        //    case Direction.Backward:
        //        animator.Play("Fall Back");
        //        break;
        //    case Direction.Left:
        //        animator.Play("Fall Left");
        //        break;
        //    default:
        //        animator.Play("Fall Right");
        //        break;
        //}

        if(devCombat.mirroredAttack())
            animator.Play("Fall Back Mirrored");
        else
            animator.Play("Fall Back");
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

    IEnumerator translateEnemyFall(Direction enemyFallDirection)
    {
        float tt = 0f;
        float multiplier = 0.025f;
        float decrement = multiplier / 100f;
        while (tt < 100f)
        {
            transform.Translate(transform.forward.normalized * multiplier);
            tt += 1f;
            multiplier = Mathf.Max(multiplier - decrement, 0.01f);
            yield return null;
        }
    }

    IEnumerator animatorSpeedChanges(Direction enemyFallDirection)
    {
        Animator devAnimator = DevMain.Player.GetComponent<Animator>();
        animator.speed = 0f;
        devAnimator.speed = 0f;
        while (animator.speed > 0f)
        {
            animator.speed -= 0.05f;
            devAnimator.speed -= 0.05f;
            yield return null;
        }


        yield return new WaitForSecondsRealtime(0.05f);

        StartCoroutine(translateEnemyFall(enemyFallDirection));

        while (animator.speed < 1f)
        {
            animator.speed += 0.05f;
            devAnimator.speed += 0.05f;

            transform.Translate(transform.forward.normalized * 0.15f);

            yield return null;
        }

    }

}
