using UnityEngine;
using System;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{

    private CharacterController characterController; // A reference to the RobotCharacter on the object
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private static Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    private CameraController cameraController;
    private DevCombat devCombat;
    private Animator animator;

    private bool prevDisabledInput;
    private float reenabledInputStartTime;
    private float reenableDelay = 0.5f;
    private bool inputEnabled;
    private bool cameraEnabled = true;
    private GameObject blockCameraPanel;

    public static ControlsManager controlsManager;
    [HideInInspector] public bool combatEnabled;

    public void Init()
    {
        combatEnabled = false;
        inputEnabled = true;
        cameraEnabled = true;
        prevDisabledInput = false;

        m_Cam = Camera.main.transform;
        controlsManager = new ControlsManager();
        characterController = GetComponent<CharacterController>();
        cameraController = m_Cam.GetComponent<CameraController>();
        devCombat = GetComponent<DevCombat>();
        animator = GetComponent<Animator>();

        controlsManager.Init();
        characterController.Init();
        cameraController.Init();
    }


    public void PhysicsUpdate()
    {
        HandleInputs();
    }

    public void FrameUpdate()
    {
        if (!inputEnabled) return;

        controlsManager.updateRecentInputDevice();

        if(characterController.jumpEnabled)
            if (controlsManager.GetButtonDown(ControlsManager.ButtonType.Jump))
                StartCoroutine(BufferedJump()); //allows you to press jump slightly (10 frames) before you hit the ground.
    }

    void HandleInputs()
    {
        if (characterController == null)
            return;

        if (!inputEnabled)
        {
            characterController.forwardAmount = 0f;
            GetComponent<Animator>().SetFloat("Forward", 0f);
            characterController.ProcessInputs(Vector3.zero, false);
            if (cameraEnabled) cameraController.PhysicsUpdate();
            return;
        }

        float h = controlsManager.GetAxis(ControlsManager.ButtonType.Horizontal);
        float v = controlsManager.GetAxis(ControlsManager.ButtonType.Vertical);
        bool interact = controlsManager.GetButtonDown(ControlsManager.ButtonType.Interact);
        bool walk = controlsManager.GetButton(ControlsManager.ButtonType.Walk);

        bool leftMousePressed = InputController.controlsManager.GetButtonDown(ControlsManager.ButtonType.Attack);
        bool rightMouseHeld = Input.GetKey(KeyCode.Mouse1);
        bool rightMouseReleased = Input.GetKeyUp(KeyCode.Mouse1);
        bool spaceBarPressed = InputController.controlsManager.GetButtonDown(ControlsManager.ButtonType.Jump);

        SendInputs(h, v, interact, walk, leftMousePressed, rightMouseHeld, rightMouseReleased, spaceBarPressed);
    }

    void SendInputs(float h, float v, bool interact, bool walk, bool leftMousePressed, bool rightMouseHeld, bool rightMouseReleased, bool spaceBarPressed)
    {
        bool walking = walk && !characterController.jumping() && !characterController.rolling();
        bool rolling = animator.GetBool("Dodge");

        if (m_Cam != null)
        {
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            if (rolling)
                m_Move = rollingMoveCalculation(h, v);
            else
                m_Move = defaultMoveCalculation(h, v); //calculate camera relative direction to move
        }
        else
        {
            m_Move = v * Vector3.forward + h * Vector3.right; // we use world-relative directions in the case of no main camera
        }

        if (walking) m_Move *= 0.66f;

        // pass all parameters to character scripts to process and translate inputs into character actions
        characterController.ProcessInputs(m_Move, spaceBarPressed);
        cameraController.PhysicsUpdate();
        if(combatEnabled)
            devCombat.ProcessInputs(interact, leftMousePressed, rightMouseHeld, rightMouseReleased, spaceBarPressed);
    }

    IEnumerator BufferedJump()
    {
        int count = 0;
        int limit = 10;
        while (count < limit)
        {
            if (characterController.isGrounded && !characterController.startJumping && !characterController.jumping())
            {
                characterController.startJumping = true;
                Debug.LogWarning("Jumped");
                break;
            }
            else
            {
                ++count;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    #region getters and helpers
    public bool IsInputEnabled()
    {
        return inputEnabled;
    }

    public void EnableInput()
    {
        inputEnabled = true;
    }

    public void DisableInput()
    {
        inputEnabled = false;
    }

    public void DisableCamera()
    {
        cameraEnabled = false;
    }

    private Vector3 defaultMoveCalculation(float h, float v)
    {
        return v * m_CamForward + h * m_Cam.right;
    }

    private Vector3 rollingMoveCalculation(float h, float v)
    {
        Vector3 vFactor = v * m_CamForward;
        Vector3 hFactor = h * m_Cam.right;
        hFactor += Mathf.Abs(Mathf.Clamp(Mathf.Pow(Vector3.Distance(transform.position, devCombat.CurrentEnemy.transform.position), -0.5f), 0.1f, 1f) * 
            (h * 0.5f)) * m_CamForward;
        return vFactor + hFactor;
    }
    #endregion
}
