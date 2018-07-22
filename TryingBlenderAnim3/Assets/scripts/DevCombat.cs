using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))] 
public class DevCombat : MonoBehaviour
{

    #region globals
    public GameObject TestEnemy;
    public BoxCollider attackCollider;
    public AudioSource quickAttack, quickAttack2, quickAttack3;
    public AudioSource strongHit;

    [HideInInspector] public Quaternion rollRotation;

    private CharacterController characterController;
    private GameObject[] enemies;
    private Animator myAnimator;
    private Camera cam;
    private GameObject currentEnemy;
    private AudioSource[] enemyAttackReactionSounds;
    private DevCombatReactions devCombatReactionsScript;

    private float[] strongHitCrossFadeTimes, quickAttackOffsets;
    private string[] quickAttackStateNames;

    private float lerpT, lerpSpeedMultiplier, desiredOffset,
                         spaceBarPressedTime, leftMousePressedTime,
                         FPressedTime, twoButtonPressTimeMax,
                         jumpAttackStartingOffset;

    private bool needToAttack, doneLerping, needsRunningAnimation;

    //[HideInInspector] public bool Locked;

    AttackType currentType;
    #endregion

    enum AttackType
    {
        none,
        jump,
        quick
    };

    public void Init()
    {
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
    public void FrameUpdate()
    {
        handleInput();
        CheckHit();
    }

    private void handleInput()
    {
        //bool EPressed = InputController.controlsManager.GetButtonDown(ControlsManager.ButtonType.Interact);
        bool leftMousePressed = InputController.controlsManager.GetButtonDown(ControlsManager.ButtonType.Attack);
        bool rightMouseHeld = Input.GetKey(KeyCode.Mouse1);
        bool rightMouseReleased = Input.GetKeyUp(KeyCode.Mouse1);
        bool spaceBarPressed = InputController.controlsManager.GetButtonDown(ControlsManager.ButtonType.Jump);

        if (rightMouseReleased)
            myAnimator.SetBool("isBlocking", false);

        //if (EPressed)
        //    Locked = !Locked;

        if (leftMousePressedTime > 0f && (Time.time - leftMousePressedTime > twoButtonPressTimeMax)) //quick attack
        {
            leftMousePressedTime = 0f;
            triggerQuickAttack();
        }

        if (rightMouseHeld) //block
        {
            stopAttack();
            myAnimator.SetBool("isBlocking", true);
        }
        else if (leftMousePressed)
        {
            handleLeftMousePressed();
            Debug.LogWarning("Left Mouse Pressed!");
        }
        else if (spaceBarPressed)
        {
            if (myAnimator.GetBool("WeaponDrawn"))
            {
                myAnimator.SetBool("roll", true);
                Invoke("stopRolling", 1.0f);
            }
        }
    }

    void stopRolling()
    {
        myAnimator.SetBool("roll", false);
        rollRotation = Quaternion.identity;
    }

    private void handleLeftMousePressed()
    {
        leftMousePressedTime = Time.time;
    }

    private void triggerQuickAttack()
    {
        myAnimator.SetBool("doAttack", true);
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
        switch (myAnimator.GetInteger("quickAttack"))
        {
            case 1:
                myAnimator.SetInteger("quickAttack", 2);
                break;
            case 2:
                myAnimator.SetInteger("quickAttack", 3);
                break;
            case 3:
                myAnimator.SetInteger("quickAttack", 1);
                break;
            default:
                Debug.LogAssertion("quickAttack is not set to 1-3, look at DevCombat.cs script");
                break;
        }
    }

    #region getters

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
        return !isAttacking() && !devCombatReactionsScript.isBlocking();
    }

    public bool isAttacking()
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

    void CheckHit()
    {
        if (!isAttacking()) 
            return;

        Collider[] cols = Physics.OverlapBox(attackCollider.bounds.center, attackCollider.bounds.extents, 
            attackCollider.transform.rotation, LayerMask.GetMask("HurtBox"));
        foreach (Collider other in cols) {
            if (other.Equals(attackCollider))
                continue;

            Debug.Log("Hurt: " + other.transform.root.name);
        }

        cols = Physics.OverlapBox(attackCollider.bounds.center, attackCollider.bounds.extents,
            attackCollider.transform.rotation, LayerMask.GetMask("AttackBox"));
        foreach (Collider other in cols)
        {
            if (other.Equals(attackCollider))
                continue;

            Debug.Log("Blocked: " + other.transform.root.name);
        }

    }


    //placeholders

    public void makeEnemyReact() {}
    public void setHitStrong() {}

}
