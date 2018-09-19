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
    public bool recoveringFromHit;
    DevCombat devCombat;
    Animator animator;
    Rigidbody rb;
    CheckHitDeflectorShield deflector;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        devCombat = DevMain.Player.GetComponent<DevCombat>();
        deflector = DevMain.Player.GetComponent<CheckHitDeflectorShield>();

        recoveringFromHit = false;
    }

    void Update()
    {
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);
        if (devCombat.canHit && !recoveringFromHit && !deflector.deflectingEnabled)
        {
            if (CheckHit())
            {
                recoveringFromHit = true;
                Direction enemyFallDirection = DevToEnemyHitDirection();
                HurtReaction(enemyFallDirection);
                StartCoroutine(animatorSpeedChanges());
            }
        }
    }

    //The direction that the enemy goes after being hit
    public Direction DevToEnemyHitDirection()
    {
        Vector3 enemyForward = devCombat.CurrentEnemy.transform.forward;
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

    IEnumerator translateEnemyFall()
    {
        float tt = 0f;
        float multiplier = 0.045f;
        float decrement = multiplier / 100f;

        float angle = Random.Range(-30f, -70f);
        if (devCombat.mirroredAttack()) angle *= -1f;
        //Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * DevMain.Player.transform.forward.normalized;
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
    }

    IEnumerator animatorSpeedChanges()
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

        if (Random.Range(0f, 1f) < 0.25f)
            yield return new WaitForSecondsRealtime(0.2f);
        else
            yield return new WaitForSecondsRealtime(0.07f);

        StartCoroutine(translateEnemyFall());

        while (animator.speed < 1f)
        {
            animator.speed += 0.05f;
            devAnimator.speed += 0.05f;
            yield return null;
        }
    }

    public void FinishRecoveringFromHit()
    {
        recoveringFromHit = false;
    }

}
