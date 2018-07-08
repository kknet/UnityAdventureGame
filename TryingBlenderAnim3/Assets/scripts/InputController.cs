using UnityEngine;
using System;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{

    private CharacterController m_Character; // A reference to the RobotCharacter on the object
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private static Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    private CameraController camScript;

    private bool prevDisabledInput;
    private float reenabledInputStartTime;
    private float reenableDelay = 0.5f;
    private bool inputEnabled;
    private bool cameraEnabled = true;
    private GameObject blockCameraPanel;

    public static ControlsManager controlsManager;
    public bool combatEnabled;

    public void Init()
    {
        combatEnabled = false;
        inputEnabled = true;
        cameraEnabled = true;
        controlsManager = new ControlsManager();
        controlsManager.Init();
        prevDisabledInput = false;

        m_Character = GetComponent<CharacterController>();
        m_Cam = Camera.main.transform;
        camScript = Camera.main.GetComponent<CameraController>();
    }

    public void FrameUpdate()
    {
        if (!inputEnabled) return;

        controlsManager.updateRecentInputDevice();

        if (controlsManager.GetButtonDown(ControlsManager.ButtonType.Jump))
            StartCoroutine(BufferedJump()); //allows you to press jump slightly (10 frames) before you hit the ground.
    }

    public void PhysicsUpdate()
    {
        if (m_Character == null)
            return;

        if (!inputEnabled)
        {
            m_Character.m_ForwardAmount = 0f;
            GetComponent<Animator>().SetFloat("Forward", 0f);
            m_Character.Move(Vector3.zero, false, false);
            if (cameraEnabled) camScript.PhysicsUpdate();
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

        bool walking = walk && !m_Character.jumping() && !m_Character.rolling();
        if (walking) m_Move *= 0.5f;


        // pass all parameters to the character control script
        m_Character.Move(m_Move, false, false);
        camScript.PhysicsUpdate();
    }

    IEnumerator BufferedJump()
    {
        int count = 0;
        int limit = 10;
        while (count < limit)
        {
            if (m_Character.m_grounded && !m_Character.m_jump && !m_Character.jumping())
            {
                m_Character.m_jump = true;
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
