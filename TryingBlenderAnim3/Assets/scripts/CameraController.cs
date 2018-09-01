using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class CameraController : MonoBehaviour
{
    #region variables etc
    public static CameraController camScript;
    [HideInInspector] public GameObject Player;
    [HideInInspector] public DevCombat devCombat;

    public CollisionHandler collision;
    public Transform target;
    public Transform rollingHelper;
    public bool cameraBobEnabled;
    public bool cameraCollisionZoom;
    public bool autoAdjustCam;
    public bool drawGizmos;
    //public bool CameraAssist;

    InputController inputController;
    CameraBob cameraBob;
    CharacterController characterScript;
    Camera cam;
    Vector3 screenPoint;

    float lastCamClipTime;
    const float camDistanceResetDelay = 0.5f;

    Vector3 targetPos, camPos, desiredCamPos;
    Vector3 velocityCamSmooth = Vector3.zero;
    Vector3 verticalPosOffset = Vector3.up * 1.5f;
    float horizontalPosOffsetMultiplier = 0.6f;
    float normalDistance = 1.2f;
    //float combatDistance = 1f;
    float combatDistance = 2f;
    float distance;
    float controllerSensitivityMultiplier = 3f;
    [SerializeField] float mouseSensitivityX = 20f;
    [SerializeField] float mouseSensitivityY = 10f;
    float yMinLimit = -30f;
    float yMaxLimit = 80f;
    float smoothTime = 50f;
    float positionSmoothTime;
    float normalDistanceSmoothTime = 1f;
    float climbingDistanceSmoothTime = 3f;
    float rotationYAxis = 0.0f;
    float rotationXAxis = 0.0f;
    float previousGoalY = 0.0f;
    float velocityX = 0.0f;
    float velocityY = 0.0f;
    float moveDirection = -1;

    int noRotationYCount = 0;
    int noRotationXCount = 0;
    int noRotationXThreshold = 5;
    int noRotationYThreshold = 5;
    #endregion

    #region cameraSettings
    public void setMouseSensitivityX(float value)
    {
        mouseSensitivityX = (value * 5f);
    }

    public void setMouseSensitivityY(float value)
    {
        mouseSensitivityY = (value * 1f);
    }

    //public void setCameraAssist(bool value)
    //{
    //    CameraAssist = value;
    //}

    public void setCameraInvertedX(bool value)
    {
        float absMouseX = Mathf.Abs(mouseSensitivityX);
        float mouseInvertedX = value ? -1f : 1f;
        mouseSensitivityX = absMouseX * mouseInvertedX;
    }

    public void setCameraInvertedY(bool value)
    {
        float absMouseY = Mathf.Abs(mouseSensitivityY);
        float mouseInvertedY = value ? -1f : 1f;
        mouseSensitivityY = absMouseY * mouseInvertedY;
    }
    #endregion

    public void Init()
    {
        Player = DevMain.Player;
        inputController = Player.GetComponent<InputController>();
        devCombat = Player.GetComponent<DevCombat>();
        cameraBob = GetComponent<CameraBob>();
        camScript = GetComponent<CameraController>();
        cam = Camera.main;
        cam.nearClipPlane = 0.01f;
        characterScript = Player.GetComponent<CharacterController>();

        distance = initialDistance();
        Vector3 angles = transform.eulerAngles;
        rotationYAxis = (rotationYAxis == 0) ? angles.y : rotationYAxis;
        rotationXAxis = angles.x;

        Quaternion rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        float backPosCoefficient = Mathf.Clamp(-rotationXAxis / ((yMaxLimit - yMinLimit) / 2f), 0f, 1f);
        Vector3 backPosOffset = -transform.forward * 1.3f * backPosCoefficient;
        targetPos = target.position + verticalPosOffset + horizontalPosOffset();
        camPos = targetPos + rotation * negDistance + backPosOffset;

        if (cameraCollisionZoom)
        {
            collision = new CollisionHandler();
            collision.Initialize(Camera.main);
            collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
            collision.UpdateCameraClipPoints(camPos, transform.rotation, ref collision.desiredCameraClipPoints);
        }

        positionSmoothTime = smoothTime * 2f;
    }

    bool cameraShouldBob()
    {
        return characterScript.m_ForwardAmount > 0.9f && !characterScript.inCombatMode();
    }

    Vector3 horizontalPosOffset()
    {
        float multiplierGoal;
        Vector3 direction = transform.right;
        multiplierGoal = characterScript.inCombatMode() ? 0.9f : 0.6f;
        horizontalPosOffsetMultiplier = Mathf.Lerp(horizontalPosOffsetMultiplier, multiplierGoal, 3f * Time.fixedDeltaTime);
        return direction * horizontalPosOffsetMultiplier;
    }
    
    public void PhysicsUpdate()
    {
        moveCamera();

        if (InputController.controlsManager.RecentDevice.Equals(ControlsManager.InputDevice.keyboard))
            Cursor.visible = true;
        else
            Cursor.visible = false;
    }

    void moveCamera()
    {
        if (characterScript.jumping()) smoothTime = 3f;
        else smoothTime = 5f;

        Quaternion rotation = updateRotation();
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        float backPosCoefficient = Mathf.Clamp(-rotationXAxis / ((yMaxLimit - yMinLimit) / 2f), 0f, 1f);
        Vector3 backPosOffset = -transform.forward * 1.3f * backPosCoefficient;
        targetPos = target.position + verticalPosOffset + horizontalPosOffset();
        camPos = targetPos + rotation * negDistance + backPosOffset;
        if(cameraBobEnabled)
            camPos = cameraBob.AddCameraBob(camPos, characterScript.m_Animator.GetFloat("Forward"), cameraShouldBob());

        Vector3 defaultNegDistance = new Vector3(0.0f, 0.0f, -initialDistance());
        desiredCamPos = targetPos + rotation * defaultNegDistance;

        if (cameraCollisionZoom)
        {
            collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
            collision.UpdateCameraClipPoints(desiredCamPos, transform.rotation, ref collision.desiredCameraClipPoints);

            if (drawGizmos)
                for (int i = 0; i < 5; ++i)
                {
                    Debug.DrawLine(target.position, collision.desiredCameraClipPoints[i], Color.red);
                    Debug.DrawLine(target.position, collision.adjustedCameraClipPoints[i], Color.green);
                }

            collision.checkColliding(targetPos);
            float zoomedDistance = collision.colliding ? collision.getAdjustedDistanceWithRay(targetPos) : initialDistance();
            bool colliding = !characterScript.inCombatMode() && collision.colliding && zoomedDistance < initialDistance();
            if (colliding)
            {
                lastCamClipTime = Time.fixedTime;
                if (rotationXAxis < 0f)
                    zoomedDistance *= 0.2f;
                distance = zoomedDistance;
                negDistance = new Vector3(0.0f, 0.0f, -distance);
                backPosCoefficient = Mathf.Clamp(-rotationXAxis / ((yMaxLimit - yMinLimit) / 2f), 0f, 1f);
                backPosOffset = -transform.forward * 1.3f * backPosCoefficient;
                camPos = targetPos + rotation * negDistance + backPosOffset;
            }
            else
            {
                distance = Mathf.MoveTowards(distance, initialDistance(), Time.fixedDeltaTime * distanceSmoothTime());
            }

            if (Mathf.Approximately(distance, initialDistance()))
                positionSmoothTime = Mathf.MoveTowards(positionSmoothTime, smoothTime * 8f, Time.fixedDeltaTime * 2f);
            else
                positionSmoothTime = Mathf.MoveTowards(positionSmoothTime, smoothTime * 2f, Time.fixedDeltaTime * 2f);

            transform.position = Vector3.Lerp(transform.position, camPos, Time.fixedDeltaTime * positionSmoothTime);
        }
        else
        {
            distance = initialDistance();
            transform.position = Vector3.Slerp(transform.position, camPos, Time.fixedDeltaTime * smoothTime);
        }

        transform.LookAt(targetPos);

        velocityX = Mathf.Lerp(velocityX, 0, Time.fixedDeltaTime * smoothTime * 2f);
        velocityY = Mathf.Lerp(velocityY, 0, Time.fixedDeltaTime * smoothTime * 2f);
    }

    private void handleAutoReset()
    {
        bool moving = characterScript.running();
        bool noRotationY = (Mathf.Abs(velocityY) < 0.01);
        if (moving && noRotationY)
            rotationXAxis = Mathf.Lerp(rotationXAxis, 10f, Time.fixedDeltaTime * smoothTime * 0.2f);
    }

    private float initialDistance()
    {
        if (characterScript.inCombatMode())
            return combatDistance;
        else
            return normalDistance;
    }

    private float distanceSmoothTime()
    {
        //if (characterScript.m_climbingWall)
        //    return climbingDistanceSmoothTime;
        //else
        return normalDistanceSmoothTime;
    }

    public float cameraXRotationDif()
    {
        float dif = Mathf.Abs(target.eulerAngles.y - transform.eulerAngles.y);
        if (dif > 180) dif = 360 - dif;
        return dif;
    }

    private bool handleManualReset()
    {
        if (InputController.controlsManager.GetButtonDown(ControlsManager.ButtonType.ResetCam))
        {
            rotationYAxis = target.eulerAngles.y;
            rotationXAxis = 10f;
            return true;
        }
        return false;
    }

    Quaternion updateRotation()
    {
        float controllerMouseX = InputController.controlsManager.controller.GetAxis(ControlsManager.ButtonType.MouseX);
        float controllerMouseY = InputController.controlsManager.controller.GetAxis(ControlsManager.ButtonType.MouseY);
        float mouseXOverall = InputController.controlsManager.GetAxis(ControlsManager.ButtonType.MouseX);
        float mouseYOverall = InputController.controlsManager.GetAxis(ControlsManager.ButtonType.MouseY);
        float h = InputController.controlsManager.GetAxis(ControlsManager.ButtonType.Horizontal);

        bool usingControllerXY = Mathf.Abs(controllerMouseX) > 0.05f || Mathf.Abs(controllerMouseY) > 0.05f;
        if (usingControllerXY)
        {
            velocityX += mouseSensitivityX * controllerSensitivityMultiplier * mouseXOverall * 0.02f;
            velocityY += mouseSensitivityY * controllerSensitivityMultiplier * -mouseYOverall * 0.02f;
        }
        else
        {
            velocityX += mouseSensitivityX * mouseXOverall * 0.02f;
            velocityY += mouseSensitivityY * mouseYOverall * 0.02f;
        }
        if (characterScript.inCombatMode())
        {
            UpdateCombatRotationYAxis();
            rotationXAxis = 12f;
        }
        else
        {
            velocityX += h * 0.2f;
            rotationYAxis += velocityX;
            rotationXAxis -= velocityY;
        }

        //------------restricting angle-----------//
        //if (characterScript.m_climbingWall)
        //{
        //    rotationYAxis = ClampAngle(rotationYAxis, target.eulerAngles.y - 80f, target.eulerAngles.y + 80f);
        //}

        rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);

        bool didManualReset = handleManualReset();
        if (autoAdjustCam && !didManualReset)
            handleAutoReset();

        return Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
    }

    private void UpdateCombatRotationYAxis()
    {
        float goalY = UnwrapEulerAngle(rollingHelper.eulerAngles.y, previousGoalY);
        //goalY = ClampAngle(Player.transform.eulerAngles.y, previousGoalY);
        //Debug.Log("goalY: " + goalY);

        rotationYAxis = Mathf.Lerp(rotationYAxis, goalY, 5f * Time.fixedDeltaTime);
        previousGoalY = goalY;
    }


    public static float UnwrapEulerAngle(float angle, float previous)
    {
        while (angle < -360F)
            angle += 360F;
        while (angle > 360F)
            angle -= 360F;

        while (angle - previous > 100f)
            angle = angle - 360f;
        while (angle - previous < -100f)
            angle = angle + 360f;

        return angle;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        while (angle < -360F)
            angle += 360F;
        while (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}

[System.Serializable]
public class CollisionHandler
{
    [HideInInspector] public LayerMask collisionLayer;
    [HideInInspector] public bool colliding = false;
    [HideInInspector] public Vector3[] adjustedCameraClipPoints;
    [HideInInspector] public Vector3[] desiredCameraClipPoints;

    Camera camera;
    GameObject Player;

    public void Initialize(Camera cam)
    {
        Player = DevMain.Player;
        camera = cam;
        adjustedCameraClipPoints = new Vector3[5];
        desiredCameraClipPoints = new Vector3[5];
    }

    public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
    {
        if (!camera)
        {
            Debug.LogError("No camera!");
            return;
        }

        intoArray = new Vector3[5];
        float z = camera.nearClipPlane;
        float x = Mathf.Tan(camera.fieldOfView / 3.41f) * z;
        float y = x / camera.aspect;

        //top left
        intoArray[0] = (atRotation * new Vector3(-x, y, z) + cameraPosition);

        //top right
        intoArray[1] = (atRotation * new Vector3(x, y, z) + cameraPosition);

        //bottom left
        intoArray[2] = (atRotation * new Vector3(-x, -y, z) + cameraPosition);

        //bottom right
        intoArray[3] = (atRotation * new Vector3(x, -y, z) + cameraPosition);

        //cam pos
        intoArray[4] = cameraPosition - camera.transform.forward;
    }


    bool collisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
    {
        for (int i = 0; i < clipPoints.Length; ++i)
        {
            RaycastHit hit;
            Vector3 origin = fromPosition;
            Vector3 direction = clipPoints[i] - fromPosition;
            //Ray ray = new Ray(origin, direction);
            float distance = Vector3.Distance(clipPoints[i], fromPosition);
            if (Physics.Raycast(origin, direction, out hit, distance))
            {
                if (!hit.collider.transform.root.gameObject.Equals(Player.gameObject))
                    return true;
            }
        }

        return false;
    }

    public float getAdjustedDistanceWithRay(Vector3 from)
    {
        float distance = -1;

        for (int i = 0; i < desiredCameraClipPoints.Length; ++i)
        {
            Ray ray = new Ray(from, desiredCameraClipPoints[i] - from);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {

                if (hit.collider.transform.root.gameObject.Equals(Player.gameObject))
                    continue;
                if (distance == -1)
                    distance = hit.distance;
                else
                {
                    if (hit.distance < distance)
                        distance = hit.distance;
                }
            }
        }

        if (distance == -1)
            return 0;
        else
            return distance;
    }

    public void checkColliding(Vector3 targetPosition)
    {
        colliding = collisionDetectedAtClipPoints(desiredCameraClipPoints, targetPosition);
    }
}