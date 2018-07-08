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
    public float m_TurnAmount;

    InputController InputController;
    CharacterEvents CharacterEvents;

    [HideInInspector] public CapsuleCollider m_Capsule;
    float lastGroundedTime;
    Rigidbody m_Rigidbody;
    Animator m_Animator;
    Vector3 m_GroundNormal;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    #endregion

    #region things to tweak in inspector

    [Tooltip("Particle effect for walking. Don't change!")]
    [SerializeField]
    public GameObject footDust;

    [Tooltip("Feet transforms used for particle effect postioning. Don't change!")]
    [SerializeField]
    public Transform leftFoot, rightFoot;

    [Tooltip("Hand transforms used for particle effect postioning. Don't change!")]
    public Transform leftHand, rightHand;

    //[Tooltip("Targets on player's body with weights for stealth detection raycasts.")]
    //public List<WeightedDetectionTarget> detectionTargets = new List<WeightedDetectionTarget>();

    [Tooltip("How much upwards force when jumping?")]
    [Range(7f, 15f)]
    public float m_JumpPower;
    #endregion 

    #region things to tweak only in code
    float m_MovingTurnSpeed = 980f;
    float m_GravityMultiplier = 2f;
    float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    float m_MoveSpeedMultiplier = 4.5f;
    float m_AnimSpeedMultiplier = 5f;
    float m_WallJumpCheckDistance = 0.5f;
    float m_GroundCheckDistance = 0.2f;
    int lerpFrames = 60;
    float lerpSmoothing = 16f;

    JumpState jumpState = JumpState.notJumping;
    JumpState prevJumpState = JumpState.notJumping;

    float jumpAmountGoal = -0.1f;
    float jumpLandTime = 0f;
    float jumpStartTime = 0f;
    #endregion

    private void Awake()
    {
        //detectionTargets.Sort();
        //detectionTargets.Reverse();
    }

    void Start()
    {
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
    }

    public void Move(Vector3 move, bool crouch, bool jump)
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
        else if (jumpState == JumpState.waitingToIdle && !checkJumpIntoWall())
            m_ForwardAmount = move.z * 1f;
        else if ((jumping() && !checkJumpIntoWall()))
            m_ForwardAmount = move.z * 2f;
        else if (jumping() && checkJumpIntoWall())
        {
            m_ForwardAmount = 0f;
            move.z = 0f;
        }
        else
            m_ForwardAmount = move.z;

        if (m_grounded || (!m_grounded && jumpState == JumpState.waitingToLand))
            RotatePlayer();

        bool fallingDown = m_Rigidbody.velocity.y < 0f;
        if (jumping() || fallingDown)
            m_Rigidbody.AddForce(Physics.gravity * 1.25f);

        UpdateAnimator(move);

        bool movingVert = !Mathf.Approximately(m_Animator.GetFloat("Vertical"), 0f);
        bool movingHoriz = !Mathf.Approximately(m_Animator.GetFloat("Horizontal"), 0f);
        if (!movingVert && !movingHoriz)
            CharacterEvents.stopFootstepSound();
    }

    void UpdateAnimator(Vector3 move)
    {
        CheckGroundStatus();
        updateJumpState();
        m_Rigidbody.useGravity = true;

        if (jumping() && checkJumpIntoWall())
        {
            m_Animator.SetFloat("Forward", 0f);
            m_Capsule.material.dynamicFriction = 0f;
            m_Capsule.material.staticFriction = 0f;
            m_Capsule.material.bounciness = 0f;
        }
        else
            m_Animator.SetFloat("Forward", Mathf.MoveTowards(m_Animator.GetFloat("Forward"), m_ForwardAmount, 0.05f));

        if (m_jump)
        {
            prevJumpState = jumpState;
            jumpState = JumpState.waitingToRise;
        }
        else
            transform.Translate(m_Animator.GetFloat("Forward") * Vector3.forward * Time.fixedDeltaTime * m_MoveSpeedMultiplier);

        bool fallingDown = m_Rigidbody.velocity.y < 0f && !m_grounded && !jumping();
        if (fallingDown) jumpState = JumpState.waitingToFall;
    }

    void RotatePlayer()
    {
        transform.Rotate(0, m_TurnAmount * m_MovingTurnSpeed * Time.fixedDeltaTime, 0);
    }

    public bool running()
    {
        return Mathf.Abs(m_Animator.GetFloat("Forward")) > 0.01f;
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
                        spawnFootDust(2);
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

    void spawnFootDust(int doLeftFoot)
    {
        GameObject footDustClone = null;
        Vector3 dustPos = Vector3.zero;

        if (doLeftFoot == 3) //glide particles
        {
            float fwd = (0.7f * Mathf.Min(m_ForwardAmount, 1f) - 0.1f);
            dustPos = leftFoot.position;
            footDustClone = Instantiate(footDust, dustPos, transform.rotation);
            footDustClone.GetComponent<ParticleSystem>().Play();
            Destroy(footDustClone, 2.0f);

            dustPos = rightFoot.position;
            footDustClone = Instantiate(footDust, dustPos, transform.rotation);
            footDustClone.GetComponent<ParticleSystem>().Play();
            Destroy(footDustClone, 2.0f);

            dustPos = leftHand.position;
            footDustClone = Instantiate(footDust, dustPos, transform.rotation);
            footDustClone.GetComponent<ParticleSystem>().Play();
            Destroy(footDustClone, 2.0f);

            dustPos = rightHand.position;
            footDustClone = Instantiate(footDust, dustPos, transform.rotation);
            footDustClone.GetComponent<ParticleSystem>().Play();
            Destroy(footDustClone, 2.0f);

        }

        if (doLeftFoot == 2) //jump land
        {
            float fwd = (0.7f * Mathf.Min(m_ForwardAmount, 1f) - 0.1f);
            dustPos = leftFoot.position + (0.01f * leftFoot.right) + (fwd * transform.forward) - (0.2f * transform.up);
            footDustClone = Instantiate(footDust, dustPos, transform.rotation);
            footDustClone.GetComponent<ParticleSystem>().Play();
            Destroy(footDustClone, 2.0f);

            dustPos = rightFoot.position + (-0.01f * rightFoot.right) + (fwd * transform.forward) - (0.2f * transform.up);
            footDustClone = Instantiate(footDust, dustPos, transform.rotation);
            footDustClone.GetComponent<ParticleSystem>().Play();
            Destroy(footDustClone, 2.0f);
        }


        bool walking = m_ForwardAmount < 0.9f || !m_grounded;
        if (walking)
            return;

        if (doLeftFoot == 0)
        {
            dustPos = leftFoot.position + (0.01f * leftFoot.right) - (0.0f * transform.forward) + (0f * transform.up);
        }
        else if (doLeftFoot == 1)
        {
            dustPos = rightFoot.position + (-0.01f * rightFoot.right) - (0.0f * transform.forward) + (0f * transform.up);
        }
        footDustClone = Instantiate(footDust, dustPos, transform.rotation);
        footDustClone.GetComponent<ParticleSystem>().Play();

        Destroy(footDustClone, 2.0f);
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
