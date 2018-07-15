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

        controlsManager.Init();
        characterController.Init();
        cameraController.Init();
    }

    public void FrameUpdate()
    {
        if (!inputEnabled) return;

        controlsManager.updateRecentInputDevice();

        if(characterController.jumpEnabled)
            if (controlsManager.GetButtonDown(ControlsManager.ButtonType.Jump))
                StartCoroutine(BufferedJump()); //allows you to press jump slightly (10 frames) before you hit the ground.
    }

    public void PhysicsUpdate()
    {
        if (characterController == null)
            return;

        if (!inputEnabled)
        {
            characterController.m_ForwardAmount = 0f;
            GetComponent<Animator>().SetFloat("Forward", 0f);
            characterController.Move(Vector3.zero);
            if (cameraEnabled) cameraController.PhysicsUpdate();
            return;
        }

        float h = controlsManager.GetAxis(ControlsManager.ButtonType.Horizontal);
        float v = controlsManager.GetAxis(ControlsManager.ButtonType.Vertical);
        bool interact = controlsManager.GetButtonDown(ControlsManager.ButtonType.Interact);
        bool walk = controlsManager.GetButton(ControlsManager.ButtonType.Walk);

        if (m_Cam != null)
        {
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = defaultMoveCalculation(h, v); //calculate camera relative direction to move
        }
        else
        {
            m_Move = v * Vector3.forward + h * Vector3.right; // we use world-relative directions in the case of no main camera
        }

        bool walking = walk && !characterController.jumping() && !characterController.rolling();
        if (walking) m_Move *= 0.66f;


        // pass all parameters to the character control script
        characterController.Move(m_Move);
        cameraController.PhysicsUpdate();
    }

    IEnumerator BufferedJump()
    {
        int count = 0;
        int limit = 10;
        while (count < limit)
        {
            if (characterController.m_grounded && !characterController.m_jump && !characterController.jumping())
            {
                characterController.m_jump = true;
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
}
