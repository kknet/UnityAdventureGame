﻿using System.Collections;
using UnityEngine;

// Ugly script, just used for demo/testing purposes!
public class TargetMatching : MonoBehaviour
{
    CharacterController characterController;
    DevCombat devCombat;
    Animator animator;
    Rigidbody rb;
    Collider hurtCollider;
    CheckHitDeflectorShield deflectorHit;

    Vector3 desiredPos;
    Quaternion correctRot;
    int attackIndex;

    [HideInInspector] public bool shouldMatchTarget;
    [HideInInspector] public bool recoveringFromHit;

    //private float[] matchEndTimes = {
    //    0.66f,
    //    0.8f,
    //    0.5f
    //};

    private float[] matchEndTimes = {
        0.5f,
        0.45f,
        0.29f,
        0.35f,
        0.35f
    };

    private float[] attackDistances = {
        0.806f,
        0.951f,
        1.067f,
        0.05f,
        0.05f
    };

    private float[] desiredDistances = {
        0.5f,
        0.5f,
        2f,
        0.7f,
        0.7f
    };

    private float[] margins = {
        3f,
        6f,
        4f,
        2f,
        2f
    };

    private float[] totalDistances = {
        3.5f,
        6.5f,
        6f,
        2.7f,
        2.7f
    };

    private int[] attacksByDistance =  {
        4,
        5,
        1,
        3,
        2
    };

    public float[] TotalDistances
    {
        get
        {
            return totalDistances;
        }
    }

    public int[] AttacksByDistance
    {
        get
        {
            return attacksByDistance;
        }
    }

    private void Awake()
    {
        deflectorHit = GetComponent<CheckHitDeflectorShield>();
        characterController = GetComponent<CharacterController>();
        devCombat = GetComponent<DevCombat>();
        animator = GetComponent<Animator>();
        hurtCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        shouldMatchTarget = false;
    }

    public float[] Margins
    {
        get
        {
            return margins;
        }
    }

    public float[] DesiredDistances
    {
        get
        {
            return desiredDistances;
        }
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

        float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        if (normalizedTime > matchEndTimes[attackIndex])
            return;

        MatchTargetWeightMask mask = new MatchTargetWeightMask(new Vector3(1, 1, 1), 0);
        float startTime = normalizedTime;
        float endTime = matchEndTimes[attackIndex];

        if (attackIndex == 4 || attackIndex == 5) startTime = Mathf.Max(normalizedTime, 0.15f);

        animator.MatchTarget(desiredPos, correctRot, AvatarTarget.Root, mask, startTime, endTime);
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

        float sphereRadius = 0.5f;
        if (deflectorHit.deflectingEnabled)
            enemyPos = enemyPos - (dir * sphereRadius);

        attackIndex = animator.GetInteger("quickAttack") - 1;
        correctRot = Quaternion.LookRotation(characterController.currentEnemyLookDirection());
        desiredPos = enemyPos - (desiredDistances[attackIndex] * dir);
        shouldMatchTarget = InAttackingRange(curPos, desiredPos, attackIndex);

        if (!shouldMatchTarget)
        {
            shouldMatchTarget = true;
            desiredPos = curPos + (margins[attackIndex] * dir);
        }

        Debug.LogWarning("SET UP MT: " + (shouldMatchTarget ? "GOOD" : "BAD"));
    }

    private bool InAttackingRange(Vector3 curPos, Vector3 desiredPos, int attackIndex)
    {
        float distance = Vector3.Distance(desiredPos, curPos);
        return distance - margins[attackIndex] < attackDistances[attackIndex];
    }

}