using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class CharacterController : MonoBehaviour
{
    public enum JumpState
    {
        waitingToStart,
        waitingToRise,
        waitingToFall,
        waitingToLand,
        waitingToIdle,
        notJumping
    }

    #region all imports
    #region imports (ignore these)
    [HideInInspector]
    public bool startJumping;
    [HideInInspector]
    public bool isGrounded = true;
    [HideInInspector]
    public float forwardAmount;
    [HideInInspector]
    public float m_SideAmount;
    [HideInInspector]
    public float turnAmount;
    [HideInInspector]
    public Animator m_Animator;


    InputController InputController;
    CharacterEvents CharacterEvents;
    DevCombat DevCombat;

    Rigidbody rb;
    Vector3 m_GroundNormal;
    CapsuleCollider m_Capsule;
    Vector3 m_CapsuleCenter;
    float m_CapsuleHeight;
    float lastGroundedTime;
    #endregion

    #region things to tweak in inspector
    //[Tooltip("Targets on player's body with weights for stealth detection raycasts.")]
    //public List<WeightedDetectionTarget> detectionTargets = new List<WeightedDetectionTarget>();

    public GameObject spine, spine1, spine2, hips, leftShoulder, rightShoulder, leftLeg, rightLeg;
    public Transform rollingHelper;

    [Tooltip("How much upwards force when jumping?")]
    [Range(7f, 15f)]
    public float m_JumpPower;
    #endregion 

    #region things to tweak only in code
    float m_MovingTurnSpeed = 180f;
    float m_CombatMoveSpeedMultiplier = 1.4f;
    float m_MoveSpeedMultiplier = 8f;
    float m_RollingSpeedMax = 8f;
    float m_RollingSpeedMin = 6f;
    float m_RollingSpeedMultiplier = 6f;
    float m_WallJumpCheckDistance = 0.5f;
    float m_GroundCheckDistance = 0.3f;
    int lerpFrames = 60;
    float lerpSmoothing = 16f;

    JumpState jumpState = JumpState.notJumping;
    JumpState prevJumpState = JumpState.notJumping;

    float jumpAmountGoal = -0.1f;
    float jumpLandTime = 0f;
    float jumpStartTime = 0f;

    public bool jumpEnabled = false;
    #endregion
    #endregion

    #region important methods
    private void Awake()
    {
        //detectionTargets.Sort();
        //detectionTargets.Reverse();
    }

    public void Init()
    {
        DevCombat = GetComponent<DevCombat>();
        m_Animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        startJumping = false;
        isGrounded = true;
        lastGroundedTime = Time.time;
        InputController = GetComponent<InputController>();
        CharacterEvents = GetComponent<CharacterEvents>();
        CharacterEvents.Init();
    }

    void LateUpdate()
    {
        bool rotationsEnabled = false;

        if (rotationsEnabled)
            if (inCombatMode() && !rolling())
            {
                spine.transform.Rotate(10f * transform.up);
                spine1.transform.Rotate(10f * transform.up);
                spine2.transform.Rotate(10f * transform.up);
                rightShoulder.transform.Rotate(20f * transform.up);
            }
    }

    public void ProcessInputs (Vector3 move, bool rollPressed)
    {
        CalculateForwardMovement(move);
        UpdateAnimator(move, rollPressed);
        RotatePlayer(move);
        TranslatePlayer();
        UpdateSounds();
    }

    void CalculateForwardMovement(Vector3 move)
    {
        AnimatorStateInfo anim = m_Animator.GetCurrentAnimatorStateInfo(0);

        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        turnAmount = Mathf.Atan2(move.x, move.z);

        if (anim.IsTag("equip"))
        {
            forwardAmount = 0f;
            move.z = 0f;
        }
        else if (anim.IsTag("impact"))
        {
            move = Vector3.back * 0.5f;
            forwardAmount = move.z * 1f;
        }
        else if (anim.IsTag("roll"))
            forwardAmount = move.z * 1f;
        else if (jumpEnabled && jumpState == JumpState.waitingToIdle && !checkJumpIntoWall())
            forwardAmount = move.z * 1f;
        else if (jumpEnabled && jumping() && !checkJumpIntoWall())
            forwardAmount = move.z * 2f;
        else if (jumpEnabled && jumping() && checkJumpIntoWall())
        {
            forwardAmount = 0f;
            move.z = 0f;
        }
        else
            forwardAmount = move.z;

        if (inCombatMode() && InputController.IsInputEnabled())
        {
            m_SideAmount = InputController.controlsManager.GetAxis(ControlsManager.ButtonType.Horizontal);
            forwardAmount = InputController.controlsManager.GetAxis(ControlsManager.ButtonType.Vertical);
        }

        if (!inCombatMode() && turnAmount > 0f && forwardAmount < 0.33f)
            forwardAmount = 0.33f;
    }

    void UpdateAnimator(Vector3 move, bool rollPressed)
    {
        CheckGroundStatus();

        if (jumpEnabled)
        {
            if (startJumping)
            {
                prevJumpState = jumpState;
                jumpState = JumpState.waitingToRise;
            }
            updateJumpState();
        }

        if (rollPressed && !rolling() && !inCombatMode() && !jumping())
            m_Animator.SetBool("Dodge", true);

        if (jumpEnabled && jumping() && checkJumpIntoWall())
        {
            m_Animator.SetFloat("Forward", 0f);
            //m_Capsule.material.dynamicFriction = 0f;
            //m_Capsule.material.staticFriction = 0f;
            //m_Capsule.material.bounciness = 0f;
        }
        else
        {
            if (inCombatMode())
            {
                m_Animator.SetFloat("Forward", Mathf.MoveTowards(m_Animator.GetFloat("Forward"), forwardAmount, 3f * Time.fixedDeltaTime));
                m_Animator.SetFloat("HorizSpeed", Mathf.MoveTowards(m_Animator.GetFloat("HorizSpeed"), m_SideAmount, 3f * Time.fixedDeltaTime));
            }
            else
            {
                m_Animator.SetFloat("Forward", Mathf.MoveTowards(m_Animator.GetFloat("Forward"), forwardAmount, 0.8f * Time.fixedDeltaTime));
            }
        }

        bool fallingDown = rb.velocity.y < 0f && !isGrounded && !jumping();
        if (fallingDown) jumpState = JumpState.waitingToFall;
        //if (jumping() || fallingDown) m_Rigidbody.AddForce(Physics.gravity * 1.25f);
    }

    void TranslatePlayer()
    {
        if (InputController.IsInputEnabled())
        {
            if (inCombatMode())
            {
                if (rolling())
                {
                    float animNormTime = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    if (animNormTime > 0.1f && animNormTime < 0.3f)
                        m_RollingSpeedMultiplier = Mathf.MoveTowards(m_RollingSpeedMultiplier, m_RollingSpeedMax, Time.fixedDeltaTime * 50f);
                    else if (animNormTime > 0.8f)
                        m_RollingSpeedMultiplier = Mathf.MoveTowards(m_RollingSpeedMultiplier, m_CombatMoveSpeedMultiplier, Time.fixedDeltaTime * 200f);
                    else
                        m_RollingSpeedMultiplier = Mathf.MoveTowards(m_RollingSpeedMultiplier, m_RollingSpeedMin, Time.fixedDeltaTime * 200f);

                    transform.Translate(Vector3.forward * Time.fixedDeltaTime * m_RollingSpeedMultiplier);
                }
                else
                {
                    bool attackMoveEnabled = false;
                    AnimatorStateInfo anim = m_Animator.GetCurrentAnimatorStateInfo(0);
                    if (attackMoveEnabled && anim.IsTag("attacking"))
                    {
                        transform.Translate(Vector3.forward * m_CombatMoveSpeedMultiplier * 0.5f * Time.fixedDeltaTime);
                    }
                    else
                    {
                        Vector3 fwd = m_Animator.GetFloat("Forward") * Vector3.forward;
                        Vector3 side = m_Animator.GetFloat("HorizSpeed") * Vector3.right;
                        Vector3 total = fwd + side;
                        if (total.magnitude > 1f) total.Normalize();
                        transform.Translate(total * Time.fixedDeltaTime * m_CombatMoveSpeedMultiplier);
                    }
                }
            }
            else
            {
                float fwd = m_Animator.GetFloat("Forward");
                float fwdMultiplier = fwd > 0f ? Mathf.Pow(fwd, 1.5f) : 0f;
                transform.Translate(fwdMultiplier * Vector3.forward *
                                    Time.fixedDeltaTime * m_MoveSpeedMultiplier);
            }
        }
    }

    void UpdateSounds()
    {
        //if (!movingVert && !movingHoriz)
        //    CharacterEvents.stopFootstepSound();
    }

    void RotatePlayer(Vector3 move)
    {
        if (!isGrounded && (isGrounded || jumpState != JumpState.waitingToLand)) //can't rotate unless grounded or on second half of jump
            return;

        float animNormTime = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;


        if (!inCombatMode()) // non combat
        {
            transform.Rotate(0, turnAmount * m_MovingTurnSpeed * Time.fixedDeltaTime, 0);
        }
        else if (inCombatMode() && rolling() && animNormTime < 0.5f) // rolling
        {
            transform.Rotate(0, turnAmount * m_MovingTurnSpeed * 3f * Time.fixedDeltaTime, 0);
            rollingHelper.forward = Vector3.RotateTowards(rollingHelper.forward, currentEnemyLookDirection(), 10f, Time.fixedDeltaTime * 10f);
        }
        else if (inCombatMode() && rolling() && animNormTime >= 0.5f) // rolling
        {
            transform.Rotate(0, turnAmount * m_MovingTurnSpeed * 0.1f * Time.fixedDeltaTime, 0);
            rollingHelper.forward = Vector3.RotateTowards(rollingHelper.forward, currentEnemyLookDirection(), 10f, Time.fixedDeltaTime * 10f);
        }
        else if (inCombatMode() && DevCombat.Locked) // locked
        {
            transform.forward = Vector3.RotateTowards(transform.forward, currentEnemyLookDirection(), 0.15f, Time.fixedDeltaTime * 2f);
            rollingHelper.forward = Vector3.RotateTowards(rollingHelper.forward, currentEnemyLookDirection(), 10f, Time.fixedDeltaTime * 10f);
        }
        else if (inCombatMode() && !DevCombat.Locked) // not locked
        {
            if (Mathf.Abs(m_Animator.GetFloat("Forward")) > 0f || Mathf.Abs(m_Animator.GetFloat("HorizSpeed")) > 0f)
                transform.forward = Vector3.RotateTowards(transform.forward, cameraLookDirection(), 0.1f, Time.fixedDeltaTime * 1f);
        }
    }
    #endregion

    #region helpers and getters

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
        Vector3 yOffset = Vector3.up * 0.1f;
        Vector3 xOffset = Vector3.right * 0.2f;
        Vector3 zOffset = Vector3.forward * 0.15f;
        Vector3 posPlusY = transform.position + yOffset;

        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character

        Debug.DrawLine(posPlusY, posPlusY + (Vector3.down * m_GroundCheckDistance), Color.red);
        Debug.DrawLine(posPlusY + zOffset, posPlusY + zOffset + (Vector3.down * m_GroundCheckDistance), Color.red);
        Debug.DrawLine(posPlusY - zOffset, posPlusY - zOffset + (Vector3.down * m_GroundCheckDistance), Color.red);
        Debug.DrawLine(posPlusY + xOffset, posPlusY + xOffset + (Vector3.down * m_GroundCheckDistance), Color.red);
        Debug.DrawLine(posPlusY - xOffset, posPlusY - xOffset + (Vector3.down * m_GroundCheckDistance), Color.red);


        if (Physics.Raycast(posPlusY, Vector3.down, out hitInfo, m_GroundCheckDistance) ||
            Physics.Raycast(posPlusY + zOffset, Vector3.down, out hitInfo, m_GroundCheckDistance) ||
            Physics.Raycast(posPlusY - zOffset, Vector3.down, out hitInfo, m_GroundCheckDistance) ||
            Physics.Raycast(posPlusY + xOffset, Vector3.down, out hitInfo, m_GroundCheckDistance) ||
            Physics.Raycast(posPlusY - xOffset, Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            m_GroundNormal = hitInfo.normal;
            isGrounded = true;
            lastGroundedTime = Time.time;
        }
        else
        {
            isGrounded = false;
            m_GroundNormal = Vector3.up;
            m_Animator.applyRootMotion = false;
        }
    }

    void updateJumpState()
    {
        switch (jumpState)
        {
            case JumpState.waitingToRise:
                {
                    AnimatorStateInfo animState = m_Animator.GetCurrentAnimatorStateInfo(0);
                    bool canJump = (prevJumpState == JumpState.notJumping);
                    if (canJump)
                    {
                        startJumping = false;
                        isGrounded = false;
                        jumpAmountGoal = 1f / 3f;
                        rb.velocity = transform.up * m_JumpPower;
                        prevJumpState = jumpState;
                        jumpState = JumpState.waitingToFall;
                    }
                    break;
                }
            case JumpState.waitingToFall:
                {
                    bool alreadyLanded = isGrounded;
                    bool falling = rb.velocity.y <= 0f;
                    if (falling || alreadyLanded)
                    {
                        jumpAmountGoal = 2f / 3f;
                        prevJumpState = jumpState;
                        jumpState = JumpState.waitingToIdle;
                    }

                    break;
                }
            case JumpState.waitingToIdle:
                {
                    if (isGrounded)
                    {
                        jumpAmountGoal = -0.1f;
                        m_Animator.SetFloat("JumpAmount", -0.1f);
                        CharacterEvents.spawnFootDust(2);
                        prevJumpState = jumpState;
                        jumpState = JumpState.notJumping;
                    }
                    break;
                }
            case JumpState.notJumping:
                {
                    break;
                }
            default:
                {
                    break;
                }
        }
        m_Animator.SetFloat("JumpAmount", Mathf.MoveTowards(m_Animator.GetFloat("JumpAmount"), jumpAmountGoal, 0.05f));
    }

    //lerp the player each frame to face a wall
    public IEnumerator lerpToFaceWall(Vector3 wallNormal)
    {
        int count = 0;
        while (count <= lerpFrames)
        {
            transform.forward = Vector3.Slerp(transform.forward, wallNormal, Time.fixedDeltaTime * lerpSmoothing);
            ++count;
            yield return null;
        }
    }

    private IEnumerator guaranteeStopJump()
    {
        yield return new WaitForSeconds(2.5f);
        if (!isGrounded)
        {
            if (Time.time - lastGroundedTime > 2.2f)
            {
                isGrounded = true;
                lastGroundedTime = Time.time;
            }
        }
    }

    public Vector3 currentEnemyLookDirection()
    {
        Vector3 enemyPos = DevCombat.TestEnemy.transform.position;
        enemyPos = new Vector3(enemyPos.x, transform.position.y, enemyPos.z);
        Vector3 dir = enemyPos - transform.position;
        dir.Normalize();
        return dir;
    }

    public Vector3 cameraLookDirection()
    {
        Vector3 camTrans = Camera.main.transform.forward;
        return new Vector3(camTrans.x, transform.forward.y, camTrans.z);
    }

    Vector3 getDodgeDirection(Vector3 move)
    {
        return move;
    }

    public bool running()
    {
        return Mathf.Abs(m_Animator.GetFloat("Forward")) > 0.01f;
    }

    public bool rolling()
    {
        AnimatorStateInfo anim = m_Animator.GetCurrentAnimatorStateInfo(0);
        return anim.IsTag("roll");
    }

    bool checkJumpIntoWall()
    {
        RaycastHit hitInfo;
        bool jumpedIntoWall = Physics.Raycast(transform.position, transform.forward, out hitInfo, m_WallJumpCheckDistance);
        bool falling = (jumpState == JumpState.waitingToIdle);
        //return jumpedIntoWall && falling;
        return jumpedIntoWall;
    }

    void ResetDodge()
    {
        m_Animator.SetBool("Dodge", false);
    }

    public bool isClimbing()
    {
        return m_Animator.GetBool("Climb");
    }

    public bool jumping()
    {
        return jumpState != JumpState.notJumping;
    }

    public bool inCombatMode()
    {
        return m_Animator.GetBool("WeaponDrawn");
    }
    #endregion
}
