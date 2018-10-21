using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealthDetection : MonoBehaviour
{

    public enum Source
    {
        Self,
        Others
    }

    public GameObject playerOutlinePrefab;
    public Transform visionSource;
    private const bool outlineEnabled = false;
    private const bool sightEnabled = true;

    [Range(10f, 50f)]
    [Tooltip("How far enemies can see")]
    public float maxLookDistance;

    private GameObject currentOutline;
    private GameObject player;
    private Animator animator;
    private EnemyAI enemyScript;
    private GameObject lastSoundSource;
    private Vector3 lastSoundPosition;
    private Vector3 lastSeenPlayerPos;
    private Vector3 lastHeardPos;
    private bool heardSomething;
    private float timeLastSeen;
    private Source sourceOfLastSeen;

    private const float timeSinceSeenThreshold = 0.3f;
    //private const int layerMask = (1 << 16);
    private const float maxLookAngle = 50f; //angle from center that agent can see, so total fov = 2 * angle

    // Use this for initialization
    void Start()
    {
        lastSeenPlayerPos = transform.position;
        player = DevRef.Player;
        enemyScript = GetComponent<EnemyAI>();
        animator = GetComponent<Animator>();
        heardSomething = false;
    }

    public void DestroyLastSeenGraphic()
    {
        if(outlineEnabled)
            if (currentOutline != null)
                Destroy(currentOutline);
    }

    public void DisplayLastSeenPosition()
    {
        if(outlineEnabled)
            if (lostPlayerSight() && SourceOfLastSeen.Equals(Source.Self))
            {
                if (currentOutline != null)
                    Destroy(currentOutline);

                currentOutline = Instantiate(playerOutlinePrefab, lastSeenPlayerPos, Quaternion.identity);
            }
    }

    private void Update()
    {
        lastSeenPlayerPos = seePlayer() ? player.transform.position : lastSeenPlayerPos;

        if (seePlayer() || SourceOfLastSeen.Equals(Source.Others))
        {
            if(outlineEnabled)
                if (currentOutline != null)
                    Destroy(currentOutline);

            timeLastSeen = Time.time;
        }
    }

    public bool lostPlayerSight()
    {
        return timeSinceSeen() > timeSinceSeenThreshold;
    }

    public float timeSinceSeen()
    {
        return Time.time - timeLastSeen;
    }

    public void acknowledgeSound()
    {
        heardSomething = false;
    }

    public void registerSound(GameObject sourceObject, Vector3 pos)
    {
        lastSoundSource = sourceObject;
        lastHeardPos = pos;
        heardSomething = true;
    }

    public bool seePlayer()
    {
        if (sightEnabled)
        {
            Vector3 playerPos = player.transform.position;
            return canSeeThisPosition(playerPos);
        }
        else
        {
            return false;
        }
    }

    public bool hearSomething()
    {
        return heardSomething;
    }

    public bool canSeeThisPosition(Vector3 pos)
    {
        return inConeOfVision(pos) && inClearLineOfSight(pos);
    }

    private bool inConeOfVision(Vector3 pos)
    {
        float distance = Vector3.Distance(transform.position, pos);

        if (distance > maxLookDistance)
            return false;

        Vector3 directionToPos = (pos - transform.position).normalized;
        float angle = Vector3.Angle(visionSource.forward, directionToPos);

        if (angle > maxLookAngle)
            return false;

        return true;
    }

    private bool inClearLineOfSight(Vector3 pos)
    {
        Vector2 direction = (pos - visionSource.position).normalized;
        float distance = Vector2.Distance(transform.position, pos);
        RaycastHit[] allSight = Physics.RaycastAll(visionSource.position, direction, distance/*, layerMask*/);

        foreach (RaycastHit sight in allSight)
        {
            if (sight.collider.transform.gameObject.tag == "Wall")
            {
                return false;
            }
        }
        return true;
    }

    public Vector3 LastSeenPlayerPos
    {
        get
        {
            return lastSeenPlayerPos;
        }
        set
        {
            lastSeenPlayerPos = value;
        }
    }

    public Vector3 LastHeardPos
    {
        get
        {
            return lastHeardPos;
        }
    }

    public Source SourceOfLastSeen
    {
        get
        {
            return sourceOfLastSeen;
        }
        set
        {
            sourceOfLastSeen = value;
        }
    }

    private void OnDrawGizmos()
    {
        DebugExtension.DrawCone(visionSource.position, visionSource.forward, maxLookAngle);
        Gizmos.DrawLine(visionSource.position, visionSource.position + (maxLookDistance * visionSource.forward));
    }

}

