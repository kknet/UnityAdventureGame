using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))] 
public class DevCombat : MonoBehaviour
{
    enum AttackType
    {
        none,
        jump,
        quick
    };

    #region globals
    public GameObject TestEnemy;
    public BoxCollider attackCollider;
    public AudioSource quickAttack, quickAttack2, quickAttack3;
    public AudioSource strongHit;
    public bool canHit;

    [HideInInspector] public Quaternion rollRotation;

    private CharacterController characterController;
    private GameObject[] enemies;
    private Animator myAnimator;
    private Camera cam;
    private GameObject currentEnemy;
    private AudioSource[] enemyAttackReactionSounds;
    private DevCombatReactions devCombatReactionsScript;
    private TargetMatching targetMatching;

    private float[] strongHitCrossFadeTimes, quickAttackOffsets;
    private string[] quickAttackStateNames;

    private float lerpT, lerpSpeedMultiplier, desiredOffset,
                         spaceBarPressedTime, leftMousePressedTime,
                         FPressedTime, twoButtonPressTimeMax,
                         jumpAttackStartingOffset;

    private bool needToAttack, doneLerping, needsRunningAnimation;

    [HideInInspector] public bool Locked;

    AttackType currentType;
    #endregion

    #region high level methods

    public void Init()
    {
        targetMatching = GetComponent<TargetMatching>();
        characterController = GetComponent<CharacterController>();
        devCombatReactionsScript = GetComponent<DevCombatReactions>();
        myAnimator = GetComponent<Animator>();
        cam = Camera.main;
        currentEnemy = GameObject.Find("Brute2");
        currentType = AttackType.none;
        quickAttackStateNames = new string[] { "quick_1", "quick_2", "quick_3" };
        enemyAttackReactionSounds = new AudioSource[] { quickAttack, quickAttack2, quickAttack3, quickAttack3 };
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        /*variables to tweak*/
        strongHitCrossFadeTimes = new float[] { 0.2f, 0.2f, 0.05f };
        quickAttackOffsets = new float[] { 1.8f, 1.8f, 1.0f };
        twoButtonPressTimeMax = 0.1f;
        jumpAttackStartingOffset = 3.7f;
    }

    public void ProcessInputs(bool interact, bool leftMousePressed, bool rightMouseHeld, bool rightMouseReleased, bool spaceBarPressed)
    {
        if (rightMouseReleased) //unblock 
            myAnimator.SetBool("isBlocking", false);

        if (interact) //locking
            Locked = !Locked;

        if (leftMousePressedTime > 0f && (Time.time - leftMousePressedTime > twoButtonPressTimeMax) && !characterController.rolling()) //quick attack
        {
            leftMousePressedTime = 0f;
            triggerQuickAttack();
        }

        if (spaceBarPressed && !myAnimator.GetBool("Dodge") && characterController.inCombatMode()) //roll
        {
            myAnimator.SetBool("Dodge", true);
            stopAttack();
            DisableHits();
            myAnimator.InterruptMatchTarget(false);
        }
        else if (rightMouseHeld && !characterController.rolling()) //block
        {
            stopAttack();
            myAnimator.SetBool("isBlocking", true);
        }
        else if (leftMousePressed && !characterController.rolling()) //quick attack
        {
            handleLeftMousePressed();
        }

        if (myAnimator.GetCurrentAnimatorStateInfo(0).IsTag("attacking"))
            myAnimator.applyRootMotion = true;
        else
            myAnimator.applyRootMotion = false;


        CheckHit();
        targetMatching.MatchTargetUpdate();
    }

    public void EnableHits()
    {
        canHit = true;
    }

    public void DisableHits()
    {
        canHit = false;
    }

    void CheckHit()
    {
        if (!attacking())
            return;

        Collider[] cols = Physics.OverlapBox(attackCollider.bounds.center, attackCollider.bounds.extents,
            attackCollider.transform.rotation, LayerMask.GetMask("HurtBox"));
        foreach (Collider other in cols)
        {
            if (other.Equals(attackCollider))
                continue;

            Debug.Log("Dev was Hurt by: " + other.transform.root.name);
        }

        cols = Physics.OverlapBox(attackCollider.bounds.center, attackCollider.bounds.extents,
            attackCollider.transform.rotation, LayerMask.GetMask("AttackBox"));
        foreach (Collider other in cols)
        {
            if (other.Equals(attackCollider))
                continue;

            Debug.Log("Dev blocked: " + other.transform.root.name);
        }
    }

    #endregion

    #region helpers

    void stopRolling()
    {
        myAnimator.SetBool("roll", false);
        rollRotation = Quaternion.identity;
    }

    private void handleLeftMousePressed()
    {
        leftMousePressedTime = Time.time;
    }

    private int pickAttackByDistance()
    {
        float dist = Vector3.Distance(transform.position, TestEnemy.transform.position);
        Debug.Log(dist);
        for(int idx = 0; idx < targetMatching.margins.Length; ++idx)
        {
            if (targetMatching.margins[idx] + targetMatching.desiredDistances[idx] > dist)
                return idx + 1;
        }
        return targetMatching.margins.Length;
    }

    private void triggerQuickAttack()
    {
        myAnimator.SetBool("doAttack", true);
        targetMatching.SetUpMatchTarget();
    }

    public void stopAttack()
    {
        myAnimator.SetBool("doAttack", false);
        myAnimator.SetBool("doFlipAttack", false);
        myAnimator.SetBool("doJumpAttack", false);
        currentType = AttackType.none;
        needToAttack = false;
        lerpT = 0f;
        doneLerping = false;
    }

    public void switchAttack()
    {
        StartCoroutine(switchAttackOnceDoneAttacking());
    }

    IEnumerator switchAttackOnceDoneAttacking()
    {
        while (myAnimator.GetCurrentAnimatorStateInfo(0).IsTag("attacking"))
            yield return null;

        float dist = Vector3.Distance(transform.position, TestEnemy.transform.position);
        float firstAttackTravelDist = targetMatching.margins[0] + targetMatching.desiredDistances[0];

        switch (myAnimator.GetInteger("quickAttack"))
        {
            case 1:
                myAnimator.SetInteger("quickAttack",
                    (Random.Range(0f, 1f) < 0.7f) ? 3 : 2);
                break;
            case 2:
                myAnimator.SetInteger("quickAttack", firstAttackTravelDist > dist ? 1 : 3);
                break;
            case 3:
                myAnimator.SetInteger("quickAttack", firstAttackTravelDist > dist ? 1 : 2);
                break;
            default:
                Debug.LogAssertion("quickAttack is not set to 1-3, look at DevCombat.cs script");
                break;
        }

        if (Random.Range(0f, 1f) < 0.5f)
            myAnimator.SetFloat("Mirrored", mirroredAttack() ? 0f : 1f);
    }

    #region getters

    public bool mirroredAttack()
    {
        return myAnimator.GetFloat("Mirrored") > 0.1f;
    }

    public GameObject CurrentEnemy
    {
        get
        {
            return currentEnemy;
        }
    }

    private bool movementButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A)
            || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)
            || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow)
            || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow)
            || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A)
            || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)
            || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow)
            || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.DownArrow);

    }

    public bool notInCombatMove()
    {
        return !attacking() && !devCombatReactionsScript.isBlocking();
    }

    public bool blocking()
    {
        return myAnimator.GetBool("isBlocking");
    }

    public bool attacking()
    {
        AnimatorStateInfo info = myAnimator.GetCurrentAnimatorStateInfo(0);
        return info.IsName("quick_1") || info.IsName("quick_2") || info.IsName("quick_3") || info.IsName("jump attack") || info.IsName("flip attack");
    }
    #endregion

    #region sounds

    public void playQuickAttackSound(int index)
    {
        if (strongHit.isPlaying)
            strongHit.Stop();
        if (quickAttack2.isPlaying)
            quickAttack2.Stop();
        if (quickAttack.isPlaying)
            quickAttack.Stop();
        if (quickAttack3.isPlaying)
            quickAttack3.Stop();

        bool rotationAllows = currentEnemy.GetComponent<EnemyCombatReactions>().rotationAllowsBlock();

        if (currentEnemy.GetComponent<EnemyCombatReactions>().isBlocking() && rotationAllows)
        {
            strongHit.Play();
        }
        else if (rotationAllows)
        {
            enemyAttackReactionSounds[index - 1].Play();
        }
        else
        {
            quickAttack.Play();
        }
    }
    #endregion

    //placeholders
    public void makeEnemyReact() {}
    public void setHitStrong() {}
    #endregion
}
