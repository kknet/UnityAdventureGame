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
    public BoxCollider attackCollider;
    public AudioSource quickAttack, quickAttack2, quickAttack3;
    public AudioSource strongHit;
    public bool canHit;

    [HideInInspector] public bool startRolling;
    [HideInInspector] public bool startAttacking;
    private CharacterController characterController;
    private Animator myAnimator;
    private Camera cam;
    private GameObject currentEnemy;
    private AudioSource[] enemyAttackReactionSounds;
    private DevCombatReactions devCombatReactionsScript;
    private TargetMatching targetMatching;

    private bool blockingEnabled = false;
    private bool alwaysLocked = false;

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
        currentType = AttackType.none;
        enemyAttackReactionSounds = new AudioSource[] { quickAttack, quickAttack2, quickAttack3, quickAttack3 };
    }

    public void ProcessInputs(bool interact, bool rightMouseHeld, bool rightMouseReleased, bool stealthAttack)
    {
        if (stealthAttack)
        {
            myAnimator.SetBool("Stealth Attack", true);
            return;
        }

        if (blockingEnabled && rightMouseReleased) //unblock 
            myAnimator.SetBool("isBlocking", false);

        if (alwaysLocked)
            Locked = true;
        else if (interact) //locking
            Locked = !Locked;

        if (startAttacking) //quick attack
        {
            startAttacking = false;
            switchAttackInternal();
            triggerQuickAttack();
        }

        if (startRolling) //roll
        {
            startRolling = false;
            myAnimator.SetBool("Dodge", true);
            stopAttack();
            DisableHits();
            myAnimator.InterruptMatchTarget(false);
        }
        else if (blockingEnabled && rightMouseHeld && !characterController.rolling()) //block
        {
            stopAttack();
            myAnimator.SetBool("isBlocking", true);
        }

        if (myAnimator.GetCurrentAnimatorStateInfo(0).IsTag("attacking"))
            myAnimator.applyRootMotion = true;
        else
            myAnimator.applyRootMotion = false;


        CheckHit();
        targetMatching.MatchTargetUpdate();
    }

    public void stopStealthAttack()
    {
        myAnimator.SetBool("Stealth Attack", false);
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
    }

    private int pickRandom(int a, int b)
    {
        return Random.Range(0f, 1f) > 0.5f ? a : b;
    }

    private int pickAttackByDistance(int curAttack)
    {
        float dist = Vector3.Distance(transform.position, CurrentEnemy.transform.position);
        Debug.Log(dist);

        if (targetMatching.TotalDistances[targetMatching.AttacksByDistance[0] - 1] > dist)
        {
            if (curAttack == targetMatching.AttacksByDistance[0]) return targetMatching.AttacksByDistance[1];
            else if (curAttack == targetMatching.AttacksByDistance[1]) return targetMatching.AttacksByDistance[0];
            else return pickRandom(targetMatching.AttacksByDistance[0], targetMatching.AttacksByDistance[1]);
        }
        else if (targetMatching.TotalDistances[targetMatching.AttacksByDistance[2] - 1] > dist)
        {
            if (curAttack == targetMatching.AttacksByDistance[2]) return pickRandom(targetMatching.AttacksByDistance[2], targetMatching.AttacksByDistance[1]);
            else return targetMatching.AttacksByDistance[2];
        }
        else
        {
            if (curAttack == targetMatching.AttacksByDistance[3]) return targetMatching.AttacksByDistance[4];
            else if (curAttack == targetMatching.AttacksByDistance[4]) return targetMatching.AttacksByDistance[3];
            else return pickRandom(targetMatching.AttacksByDistance[3], targetMatching.AttacksByDistance[4]);
        }
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
    }

    public void switchAttack()
    {
        //StartCoroutine(switchAttackOnceDoneAttacking());
    }

    private void switchAttackInternal()
    {
        StartCoroutine(switchAttackOnceDoneAttacking());
    }

    IEnumerator switchAttackOnceDoneAttacking()
    {
        bool testing1 = false;
        if (testing1)
        {
            myAnimator.SetInteger("quickAttack", 4);
            myAnimator.SetFloat("Mirrored", mirroredAttack() ? 0f : 1f);
            yield break;
        }

        int newQA = pickAttackByDistance(myAnimator.GetInteger("quickAttack"));

        //float dist = Vector3.Distance(transform.position, CurrentEnemy.transform.position);
        //float firstAttackTravelDist = targetMatching.Margins[0] + targetMatching.DesiredDistances[0];
        //int newQA = 0;
        //switch (myAnimator.GetInteger("quickAttack"))
        //{
        //    case 1:
        //        newQA = pickAttackByDistance();
        //        if (newQA == 1) newQA = Random.Range(0f, 1f) < 0.5f ? 4 : 5;
        //        break;
        //    case 2:
        //        newQA = pickAttackByDistance();
        //        if (newQA == 2) newQA = Random.Range(0f, 1f) < 0.5f ? 3 : 1;
        //        break;
        //    case 3:
        //        newQA = pickAttackByDistance();
        //        if (newQA == 3) newQA = Random.Range(0f, 1f) < 0.5f ? 2 : 1;
        //        break;
        //    case 4:
        //        newQA = Random.Range(0f, 1f) < 0.8f ? pickAttackByDistance() : 1;
        //        if (newQA == 4) newQA = 5;
        //        break;
        //    case 5:
        //        newQA = Random.Range(0f, 1f) < 0.8f ? pickAttackByDistance() : 1;
        //        if (newQA == 5) newQA = 4;
        //        break;
        //    default:
        //        Debug.LogAssertion("quickAttack is not set to 1-3, look at DevCombat.cs script");
        //        break;
        //}

        myAnimator.SetInteger("quickAttack", newQA);
        if (Random.Range(0f, 1f) < 0.7f)
            myAnimator.SetFloat("Mirrored", mirroredAttack() ? 0f : 1f);
        if (newQA == 4)
            myAnimator.SetFloat("Mirrored", 0f);
        else if (newQA == 5)
            myAnimator.SetFloat("Mirrored", 1f);
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
        set
        {
            currentEnemy = value;
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
        return info.IsTag("attacking");
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

    public void makeEnemyReact() {}
    public void setHitStrong() {}
    #endregion
}
