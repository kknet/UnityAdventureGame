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

    #region imports (ignore these)
    [HideInInspector]
    public bool m_jump;
    [HideInInspector]
    public bool m_grounded = true;
    [HideInInspector]
    public float m_ForwardAmount;
    [HideInInspector]
    public float m_SideAmount;
    [HideInInspector]
    public float m_TurnAmount;
    [HideInInspector]
    public Animator m_Animator;


    InputController InputController;
    CharacterEvents CharacterEvents;
    DevCombat DevCombat;

    Rigidbody m_Rigidbody;
    Vector3 m_GroundNormal;
    CapsuleCollider m_Capsule;
    Vector3 m_CapsuleCenter;
    float m_CapsuleHeight;
    float lastGroundedTime;
    #endregion

    #region things to tweak in inspector
    //[Tooltip("Targets on player's body with weights for stealth detection raycasts.")]
    //public List<WeightedDetectionTarget> detectionTargets = new List<WeightedDetectionTarget>();

    [Tooltip("How much upwards force when jumping?")]
    [Range(7f, 15f)]
    public float m_JumpPower;
    #endregion 

    #region things to tweak only in code
    float m_MovingTurnSpeed = 180f;
    float m_CombatMoveSpeedMultiplier = 4f;
    float m_MoveSpeedMultiplier = 8f;
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

    private void Awake()
    {
        //detectionTargets.Sort();
        //detectionTargets.Reverse();
    }

    public void Init()
    {
        DevCombat = GetComponent<DevCombat>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;

        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_jump = false;
        m_grounded = true;
        lastGroundedTime = Time.time;
        InputController = GetComponent<InputController>();
        CharacterEvents = GetComponent<CharacterEvents>();
        CharacterEvents.Init();
    }

    public void Move(Vector3 move)
    {
        AnimatorStateInfo anim = m_Animator.GetCurrentAnimatorStateInfo(0);

        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);

        if (anim.IsTag("equip"))
        {
            m_ForwardAmount = 0f;
            move.z = 0f;
        }
        else if (anim.IsTag("impact"))
        {
            move = Vector3.back * 0.5f;
            m_ForwardAmount = move.z * 1f;
        }
        else if (anim.IsTag("roll"))
            m_ForwardAmount = move.z * 1f;
        else if (jumpEnabled && jumpState == JumpState.waitingToIdle && !checkJumpIntoWall())
            m_ForwardAmount = move.z * 1f;
        else if (jumpEnabled && jumping() && !checkJumpIntoWall())
            m_ForwardAmount = move.z * 2f;
        else if (jumpEnabled && jumping() && checkJumpIntoWall())
        {
            m_ForwardAmount = 0f;
            move.z = 0f;
        }
        else
            m_ForwardAmount = move.z;

        if (inCombatMode())
            m_SideAmount = move.x;


        if (m_TurnAmount > 0f && m_ForwardAmount < 0.33f)
            m_ForwardAmount = 0.33f;

        if (m_grounded || (!m_grounded && jumpState == JumpState.waitingToLand))
            RotatePlayer();

        bool fallingDown = m_Rigidbody.velocity.y < 0f;
        //if (jumping() || fallingDown)
        //    m_Rigidbody.AddForce(Physics.gravity * 1.25f);

        UpdateAnimator(move);

        //bool movingVert = !Mathf.Approximately(m_Animator.GetFloat("Forward"), 0f);
        //bool movingHoriz = !Mathf.Approximately(m_Animator.GetFloat("Horizontal"), 0f);
        //if (!movingVert && !movingHoriz)
        //    CharacterEvents.stopFootstepSound();
    }

    void UpdateAnimator(Vector3 move)
    {
        CheckGroundStatus();

        if (jumpEnabled) updateJumpState();
        m_Rigidbody.useGravity = true;

        if (jumpEnabled && jumping() && checkJumpIntoWall())
        {
            m_Animator.SetFloat("Forward", 0f);
            //m_Capsule.material.dynamicFriction = 0f;
            //m_Capsule.material.staticFriction = 0f;
            //m_Capsule.material.bounciness = 0f;
        }
        else
        {
            m_Animator.SetFloat("Forward", Mathf.MoveTowards(m_Animator.GetFloat("Forward"), m_ForwardAmount, 0.8f * Time.fixedDeltaTime));
            m_Animator.SetFloat("HorizSpeed", Mathf.MoveTowards(m_Animator.GetFloat("HorizSpeed"), m_SideAmount, 0.8f * Time.fixedDeltaTime));
        }

        if (m_jump)
        {
            prevJumpState = jumpState;
            jumpState = JumpState.waitingToRise;
        }
        else
        {
            if (inCombatMode())
            {
                float fwd = m_Animator.GetFloat("Forward");
                float side = m_Animator.GetFloat("HorizSpeed");

                transform.Translate(Vector3.forward * fwd * Time.fixedDeltaTime * m_CombatMoveSpeedMultiplier);
                transform.Translate(Vector3.right * side * Time.fixedDeltaTime * m_CombatMoveSpeedMultiplier);
            }
            else
            {
                float fwd = m_Animator.GetFloat("Forward");
                float fwdMultiplier = fwd > 0f ? Mathf.Pow(fwd, 1.5f) : 0f;
                transform.Translate(fwdMultiplier * Vector3.forward *
                                    Time.fixedDeltaTime * m_MoveSpeedMultiplier);
            }
        }

        bool fallingDown = m_Rigidbody.velocity.y < 0f && !m_grounded && !jumping();
        if (fallingDown) jumpState = JumpState.waitingToFall;
    }

    void RotatePlayer()
    {
        if (inCombatMode() && DevCombat.Locked)
        {
            Vector3 pos = DevCombat.TestEnemy.transform.position;
            pos = new Vector3(pos.x, transform.position.y, pos.z);
            transform.LookAt(pos, transform.up);
        }
        else
        {
            if (Mathf.Abs(m_Animator.GetFloat("Forward")) > 0f)
                transform.Rotate(0, m_TurnAmount * m_MovingTurnSpeed * Time.fixedDeltaTime, 0);
        }
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

    private IEnumerator guaranteeStopJump()
    {
        yield return new WaitForSeconds(2.5f);
        if (!m_grounded)
        {
            if (Time.time - lastGroundedTime > 2.2f)
            {
                m_grounded = true;
                lastGroundedTime = Time.time;
            }
        }
    }

    bool checkJumpIntoWall()
    {
        RaycastHit hitInfo;
        bool jumpedIntoWall = Physics.Raycast(transform.position, transform.forward, out hitInfo, m_WallJumpCheckDistance);
        bool falling = (jumpState == JumpState.waitingToIdle);
        //return jumpedIntoWall && falling;
        return jumpedIntoWall;
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
                        m_jump = false;
                        m_grounded = false;
                        jumpAmountGoal = 1f / 3f;
                        m_Rigidbody.velocity = transform.up * m_JumpPower;
                        prevJumpState = jumpState;
                        jumpState = JumpState.waitingToFall;
                    }
                    break;
                }
            case JumpState.waitingToFall:
                {
                    bool alreadyLanded = m_grounded;
                    bool falling = m_Rigidbody.velocity.y <= 0f;
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
                    if (m_grounded)
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
            m_grounded = true;
            lastGroundedTime = Time.time;
        }
        else
        {
            m_grounded = false;
            m_GroundNormal = Vector3.up;
            m_Animator.applyRootMotion = false;
        }
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
}
