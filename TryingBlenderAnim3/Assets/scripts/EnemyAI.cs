using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum States
    {
        patrol,
        scan,
        investigate,
        runToPlayer,
        attack,
    }

    public static Vector3 xzMask = new Vector3(1f, 0f, 1f);
    public static bool scanLocked;
    public static bool attackLocked;
    public bool useWaypoints;
    public ParticleSystem landingParticles;
    [SerializeField] public GameObject waypointsParent;
    [SerializeField] public int enemyID;
    [HideInInspector] public RectTransform arrow;

    private CameraShake cameraShake;
    private List<Transform> waypoints;
    private Animator enemyAnim;
    private AStar aStar;
    private StealthDetection detection;
    private Transform Player;
    private Vector3 MatchTargetDesiredPos;

    private Vector3 moveDirection, targetPos;
    private float screenMaxZ, screenMinZ, screenMaxX, screenMinX;
    private bool targetIsPlayer;
    private States state;
    private float timeSinceLastAttack, timeSinceLastDash;
    private float health;
    private float moveSpeed;
    private int curWaypoint;
    private float lookAroundStartTime;

    private const float lookAroundDuration = 8f;
    private const float rotSpeed = 6f;
    private const float startingHealth = 100f;
    private const float defaultMoveSpeed = 1f;
    private const float runToPlayerSpeed = 4f;
    private const float attackDistance = 5f;
    private const float attackFrequency = 10f;

    public void Start()
    {
        enemyAnim = GetComponent<Animator>();
        Player = DevRef.Player.transform;
        aStar = Player.GetComponent<AStar>();
        detection = GetComponent<StealthDetection>();
        cameraShake = Camera.main.GetComponent<CameraShake>();

        moveDirection = Vector3.zero;
        targetPos = transform.position;
        targetIsPlayer = false;
        moveSpeed = defaultMoveSpeed;
        health = startingHealth;
        timeSinceLastAttack = 0f;
        timeSinceLastDash = 0f;
        state = States.patrol;
        curWaypoint = 0;

        waypoints = new List<Transform>();
        int numWayPoints = waypointsParent.GetComponentsInChildren<Transform>().Length;
        for (int i = 1; i <= numWayPoints; ++i)
            waypoints.Add(waypointsParent.transform.Find("" + i));


        aStar.moveDirection(transform.position, gameObject);

        screenMaxX = aStar.backRight.position.x;
        screenMinX = aStar.backLeft.position.x;
        screenMaxZ = aStar.forwardLeft.position.z;
        screenMinZ = aStar.backLeft.position.z;
    }

    void ExecuteStateActions()
    {
        if (state == States.patrol)
        {
            enemyAnim.InterruptMatchTarget(false);
            moveSpeed = defaultMoveSpeed;
            targetIsPlayer = false;
            if (useWaypoints)
            {
                if (xzDist(transform.position, waypoints[curWaypoint].position) < 2f)
                {
                    if (curWaypoint < waypoints.Count - 1)
                    {
                        curWaypoint += 1;
                    }
                    else
                    {
                        curWaypoint = 0;
                    }

                    lookAroundStartTime = Time.time;
                    enemyAnim.SetBool("LookAround", true);
                    if (enemyAnim.GetFloat("Mirrored") > 0.9f)
                        enemyAnim.SetFloat("Mirrored", 0f);
                    else
                        enemyAnim.SetFloat("Mirrored", 1f);
                }
                else
                {
                    if (enemyAnim.GetBool("LookAround"))
                    {
                        if (Time.time - lookAroundStartTime > lookAroundDuration)
                            enemyAnim.SetBool("LookAround", false);
                    }
                }

                targetPos = waypoints[curWaypoint].position;
                move();
            }
            else
                SetIdleAnimation();
        }

        if (state == States.investigate)
        {
            enemyAnim.InterruptMatchTarget(false);
            targetIsPlayer = false;
            moveSpeed = defaultMoveSpeed;
            targetPos = detection.LastHeardPos;
            move();
        }

        if (state == States.runToPlayer)
        {
            enemyAnim.InterruptMatchTarget(false);
            enemyAnim.SetBool("LookAround", false);
            moveSpeed = runToPlayerSpeed;
            alert();
            targetIsPlayer = true;
            targetPos = detection.LastSeenPlayerPos;
            move();

            detection.DisplayLastSeenPosition();
        }

        if (state == States.attack)
        {
            targetIsPlayer = true;
            enemyAnim.SetBool("LookAround", false);

            AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo(0);
            bool notStuck = !info.IsTag("fallback") && !info.IsTag("getup") && !enemyAnim.GetBool("LookAround");
            move();

            if (notStuck && inAttack())
            {
                float normalizedTime = enemyAnim.GetCurrentAnimatorStateInfo(0).normalizedTime;
                float startTime =  0.16f;
                startTime = Mathf.Max(startTime, normalizedTime);
                float endTime = 0.37f;

                if (normalizedTime < startTime || normalizedTime > endTime)
                    enemyAnim.InterruptMatchTarget(false);
                else
                {
                    Vector3 dif = Player.position - transform.position;
                    dif = new Vector3(dif.x, 0f, dif.z).normalized;
                    MatchTargetDesiredPos = clampLoc(Player.position + (-1f * dif));
                    Quaternion correctRot = Quaternion.LookRotation(dif);
                    MatchTargetWeightMask mask = new MatchTargetWeightMask(new Vector3(1, 1, 1), 0);
                    enemyAnim.MatchTarget(MatchTargetDesiredPos, correctRot, AvatarTarget.Root, mask, startTime, endTime);
                }
            }
            else
            {
                enemyAnim.InterruptMatchTarget(false);
                if (notStuck && StartNewAttack())
                {
                    Vector3 dif = Player.position - transform.position;
                    dif = new Vector3(dif.x, 0f, dif.z).normalized;
                    MatchTargetDesiredPos = clampLoc(Player.position + (-1f * dif));
                }
            }
        }

        Debug.Log(state);
    }

    public static float xzDist(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(Vector3.Scale(a, xzMask), Vector3.Scale(b, xzMask));
    }

    private States stateDecision()
    {
        float distanceToPlayer = xzDist(transform.position, Player.position);
        bool seePlayer = detection.seePlayer();
        bool hearSomething = detection.hearSomething();
        detection.acknowledgeSound();
        bool atLastSeenPos = xzDist(detection.LastSeenPlayerPos, transform.position) < 2f;
        bool atLastHeardPos = xzDist(detection.LastHeardPos, transform.position) < 3f;

        if (state.Equals(States.patrol))
        {
            if (seePlayer)
                return States.runToPlayer;

            if (hearSomething)
            {
                return States.investigate;
            }

            return States.patrol;
        }

        if (state.Equals(States.investigate))
        {
            if (seePlayer)
                return States.runToPlayer;

            if (atLastHeardPos)
            {
                StartCoroutine(scan());
                return States.scan;
            }

            if (hearSomething)
            {
                return States.investigate;
            }

            return States.investigate;
        }

        if (state.Equals(States.scan))
        {
            if (seePlayer)
            {
                StopCoroutine(scan());
                return States.runToPlayer;
            }

            if (hearSomething)
            {
                StopCoroutine(scan());
                return States.investigate;
            }
            return States.scan;
        }

        if (state.Equals(States.runToPlayer))
        {
            if (distanceToPlayer <= attackDistance && seePlayer)
            {
                return States.attack;
            }

            if (atLastSeenPos && !seePlayer)
            {
                if (hearSomething)
                {
                    return States.investigate;
                }
                else
                {
                    StartCoroutine(scan());
                    return States.scan;
                }
            }

            return States.runToPlayer;
        }

        if (state.Equals(States.attack))
        {
            if (inAttack())
                return States.attack;
            else if (seePlayer && distanceToPlayer <= attackDistance)
                return States.attack;
            else
                return States.runToPlayer;
        }

        return state;
    }

    private void move()
    {
        AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo(0);
        bool canMove = !info.IsTag("fallback") && !info.IsTag("getup") && !enemyAnim.GetBool("LookAround");
        float dist = xzDist(transform.position, targetPos);
        bool run = (state.Equals(States.runToPlayer));
        bool walk = !run && (state.Equals(States.investigate) || state.Equals(States.scan) || state.Equals(States.patrol));
        if (!canMove)
        {
            run = false;
            walk = false;
        }
        float enemyAnimSpeedGoal = run ? 1f : (walk ? 0.5f : 0f);
        float smooth = (enemyAnimSpeedGoal > enemyAnim.GetFloat("enemySpeed")) ? 3f : 1f;
        if (run && !targetIsPlayer && dist < attackDistance + 3f)
        {
            smooth = 2f;
            enemyAnimSpeedGoal = 0.5f + (0.5f * (Mathf.Abs(attackDistance + 3f - dist)/(attackDistance + 3f)));
        }
        else if (run && targetIsPlayer && dist < 3f)
        {
            smooth = 2f;
            enemyAnimSpeedGoal = 0.5f + (0.5f * (Mathf.Abs(3f - dist) / 3f));
        }

        enemyAnim.SetFloat("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat("enemySpeed"), enemyAnimSpeedGoal, smooth * Time.deltaTime));

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDirection.normalized),
                rotSpeed * Mathf.Min(0.5f, enemyAnim.GetFloat("enemySpeed")) * Time.deltaTime);
        }

        if (enemyAnim.GetFloat("enemySpeed") > 0.75f)
            transform.Translate(Vector3.forward * moveSpeed * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime);
    }

    public void Update()
    {
        timeSinceLastAttack += Time.deltaTime;

        if (health <= 0)
            Destroy(gameObject);

        state = stateDecision();

        ExecuteStateActions();
    }

    public void landingEffects()
    {
        ParticleSystem newParticles = Instantiate(landingParticles, transform.position, Quaternion.identity);
        newParticles.Play();
        Destroy(newParticles, 5f);

        float dist = xzDist(Player.position, transform.position);
        if (dist < 10f)
        {
            cameraShake.TriggerCameraShake(0.12f, 0.3f);
            if (dist < 2.5f)
                Player.GetComponent<CheckHitDeflectorShield>().EnemyLandHit();
        }
    }

    public bool TargetIsPlayer
    {
        get
        {
            return targetIsPlayer;
        }
    }

    public void setMoveDirection(Vector3 dir)
    {
        if (!inAttack())
        {
            moveDirection = dir.normalized;
            targetPos = clampLoc(targetPos);
        }

        aStar.moveDirection(targetPos, gameObject);
    }

    private Vector3 clampLoc(Vector3 loc)
    {
        if (loc.x > screenMaxX)
            loc.x = screenMaxX - 1f;
        if (loc.x < screenMinX)
            loc.x = screenMinX + 1f;
        if (loc.z > screenMaxZ)
            loc.z = screenMaxZ - 1f;
        if (loc.z < screenMinZ)
            loc.z = screenMinZ + 1f;
        return loc;
    }

    private void alert()
    {
        float range = 100f;
        detection.SourceOfLastSeen = StealthDetection.Source.Self;
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject obj in objects)
        {
            if (xzDist(transform.position, obj.transform.position) < range)
            {
                EnemyAI enemy = obj.GetComponent<EnemyAI>();
                if (enemy != null)
                    enemy.recieveAlert(detection.LastSeenPlayerPos);
            }
        }
    }

    public void recieveAlert(Vector3 lastSeenPos)
    {
        if (state != States.attack && state != States.scan)
            state = States.runToPlayer;
        float curDistance = xzDist(detection.LastSeenPlayerPos, Player.position);
        float newDistance = xzDist(lastSeenPos, Player.position);
        if (newDistance < curDistance)
        {
            detection.LastSeenPlayerPos = lastSeenPos;
            detection.SourceOfLastSeen = StealthDetection.Source.Others;
        }
        else
        {
            detection.SourceOfLastSeen = StealthDetection.Source.Self;
        }
    }

    void SetIdleAnimation()
    {
        enemyAnim.SetFloat("enemySpeed", 0f);
    }

    IEnumerator scan()
    {
        while (!state.Equals(States.scan))
            yield return null;

        Debug.Log("Scan started");

        yield return new WaitForSeconds(1f);

        float minRadius = 5f;

        //for (int i = 0; i < 3; ++i){
        while (state.Equals(States.scan))
        {
            enemyAnim.SetBool("LookAround", false);
            moveSpeed = defaultMoveSpeed;
            SetIdleAnimation();

            while (scanLocked) //true means locked, false means open
                yield return null;

            scanLocked = true; //set to true to reserve

            int counter = 0;
            Vector3 center = detection.LastSeenPlayerPos;
            if (minRadius > 20)
                center = Vector3.zero;

            Vector3 randomPos = center + (Vector3)(Random.insideUnitCircle.normalized * minRadius);
            randomPos = clampLoc(randomPos);
            while (!aStar.legalPositionInGrid(randomPos, enemyID))
            {
                ++counter;
                if (counter > 100)
                    Debug.LogError("Doesn't work!");

                randomPos = center + (Vector3)(Random.insideUnitCircle.normalized * minRadius);
                randomPos = clampLoc(randomPos);
            }
            targetPos = randomPos;

            scanLocked = false;

            const float maxDuration = 10f;
            float startTime = Time.time;
            while (xzDist(targetPos, transform.position) > 1f &&
                Time.time - startTime < maxDuration)
            {
                move();
                yield return null;
            }

            minRadius *= 1.5f;

            enemyAnim.SetBool("LookAround", true);
            if (enemyAnim.GetFloat("Mirrored") > 0.9f)
                enemyAnim.SetFloat("Mirrored", 0f);
            else
                enemyAnim.SetFloat("Mirrored", 1f);

            yield return new WaitForSeconds(lookAroundDuration);
        }

        targetPos = transform.position;
        moveSpeed = defaultMoveSpeed;
        state = States.patrol;
        detection.DestroyLastSeenGraphic();
    }

    public bool inAttack()
    {
        return enemyAnim.GetCurrentAnimatorStateInfo(0).IsTag("attack") || enemyAnim.GetBool("enemyAttack");
    }

    private bool StartNewAttack()
    {
        float distance = xzDist(transform.position, Player.position);
        if (timeSinceLastAttack >= attackFrequency && 
            !enemyAnim.GetBool("enemyAttack")
            && !attackLocked)
        {
            timeSinceLastAttack = 0f;
            attackLocked = true;
            enemyAnim.SetBool("enemyAttack", true);
            return true;
        }
        return false;
    }

    public void stopAttack()
    {
        enemyAnim.SetBool("enemyAttack", false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            enemyAnim.InterruptMatchTarget(false);
            enemyAnim.SetBool("enemyAttack", false);
        }
    }
}