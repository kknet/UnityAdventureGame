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
    CameraShake cameraShake;
    Animator animator;
    Rigidbody rb;
    CheckHitDeflectorShield deflector;
    Transform player;

    AudioSource hitSound;

    private int hitCounter = 0;
    private const int fallBackCountThreshold = 6;

	// Use this for initialization
	void Start () {
        player = DevRef.Player.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        cameraShake = Camera.main.GetComponent<CameraShake>();
        devCombat = DevRef.Player.GetComponent<DevCombat>();
        deflector = DevRef.Player.GetComponent<CheckHitDeflectorShield>();

        hitCounter = 0;
        recoveringFromHit = false;
        hitSound = GetComponent<AudioSource>();
    }

    public bool fallBackNext()
    {
        return hitCounter + 1 >= fallBackCountThreshold;
    }

    public bool fallBackRightNow()
    {
        return hitCounter == 0;
    }

    void Update()
    {
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);
        if (devCombat.canHit && !recoveringFromHit && !deflector.deflectingEnabled)
        {
            Debug.Log("can hit");
            if (CheckHit())
            {
                recoveringFromHit = true;
                ++hitCounter;

                bool fallBack = hitCounter >= fallBackCountThreshold;
                hitCounter = fallBack ? 0 : hitCounter;

                animator.InterruptMatchTarget(false);
                animator.SetBool("enemyAttack", false);

                Direction enemyFallDirection = DevToEnemyHitDirection();
                string anim = HurtReaction(enemyFallDirection, fallBack);
                StartCoroutine(animatorSpeedChanges(fallBack, anim));
            }
        }
    }

    //The direction that the enemy goes after being hit
    public Direction DevToEnemyHitDirection()
    {
        Vector3 enemyForward = devCombat.CurrentEnemy.transform.forward;
        Vector3 devForward = DevRef.Player.transform.forward;
        float angle = Vector3.SignedAngle(enemyForward, devForward, Vector3.up);
        float absAngle = Mathf.Abs(angle);

        //if (absAngle > 135f) return Direction.Backward;
        //if (absAngle < 45f) return Direction.Forward;
        //if (angle > 45f) return Direction.Right;
        //else return Direction.Left;

        if (absAngle > 120f) return Direction.Backward;
        else return Direction.Forward;
    }

    string HurtReaction (Direction enemyFallDirection, bool fallBack)
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

        string anim = "";
        if (fallBack)
        {
            //if (enemyFallDirection.Equals(Direction.Forward))
            //{
            //    anim = "Fall Forward";
            //}
            //else
            //{
            //    if (devCombat.mirroredAttack())
            //        anim = "Fall Back Mirrored";
            //    else
            //        anim = "Fall Back";
            //}

            if (devCombat.mirroredAttack())
                anim = "Fall Back Mirrored";
            else
                anim = "Fall Back";
        }
        else
        {
            if (devCombat.mirroredAttack())
                anim = "React Right";
            else
                anim = "React Left";
        }

        animator.Play(anim);
        return anim;
    }

    bool CheckHit()
    {
        if (!devCombat.attacking())
            return false;
        
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

    IEnumerator translateEnemyFall(bool fallBack, string anim)
    {
        if (fallBack)
        {
            float tt = 0f;
            float multiplier = 0.015f;
            float initialBoost = 1f;
            float decrement = multiplier / 70f;

            float angle = Random.Range(-30f, -70f);
            if (devCombat.mirroredAttack()) angle *= -1f;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * DevRef.Player.transform.forward.normalized;
            //Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * -transform.forward.normalized;

            Vector3 lookDirection = (player.position - transform.position).normalized;
            lookDirection = Quaternion.AngleAxis(angle, Vector3.up) * lookDirection;
            transform.forward = Vector3.RotateTowards(transform.forward, lookDirection, 100f * Time.deltaTime, 0.0f);

            while (tt < 70f)
            {
                Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + (30f * direction), Color.magenta);

                if (animator.speed < 0.2f)
                    transform.Translate(direction * (multiplier + initialBoost), Space.World);
                else
                    transform.Translate(direction * multiplier, Space.World);


                tt += 1f;
                multiplier = Mathf.Max(multiplier - decrement, 0.01f);
                yield return null;
            }
        }
        else
        {
            float magnitude = 0.2f;
            float angle = Random.Range(-20f, -40f);

            if (devCombat.mirroredAttack()) angle *= -1f;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * DevRef.Player.transform.forward.normalized;
            //Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * -transform.forward.normalized;

            Vector3 lookDirection = (player.position - transform.position).normalized;
            lookDirection = Quaternion.AngleAxis(angle, Vector3.up) * lookDirection;
            transform.forward = Vector3.RotateTowards(transform.forward, lookDirection, 100f * Time.deltaTime, 0.0f);

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            while (info.normalizedTime < 0.1f && info.IsName(anim))
            {
                transform.Translate(direction * magnitude, Space.World);
                info = animator.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }
        }

        recoveringFromHit = false;
    }

    IEnumerator animatorSpeedChanges(bool fallBack, string anim)
    {

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        while (!info.IsName(anim))
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        hitSound.pitch = Random.Range(0.9f, 1.1f);
        hitSound.Play();

        Animator devAnimator = DevRef.Player.GetComponent<Animator>();
        animator.speed = 0f;
        devAnimator.speed = 0f;

        if (fallBack)
        {

            yield return new WaitForSecondsRealtime(0.3f);

            StartCoroutine(translateEnemyFall(fallBack, anim));

            while (animator.speed < 1f)
            {
                animator.speed += 0.05f;
                devAnimator.speed += 0.05f;
                yield return null;
            }
        }
        else
        {
            cameraShake.TriggerCameraShake();

            yield return new WaitForSecondsRealtime(0.05f);

            StartCoroutine(translateEnemyFall(fallBack, anim));

            while (animator.speed < 1f)
            {
                animator.speed += 0.2f;
                devAnimator.speed += 0.2f;
                yield return null;
            }
        }
    }

    public void FinishRecoveringFromHit()
    {
        recoveringFromHit = false;
    }

}
