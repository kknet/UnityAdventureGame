using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum DashState
    {
        InDash,
        NotInDash
    }

    private enum States
    {
        patrol,
        scan,
        investigate,
        runToPlayer,
        attack,
    }

    public static bool scanLocked;
    public bool useWaypoints;
    [SerializeField] public Transform[] waypoints;
    [SerializeField] public int enemyID;

    private Animator enemyAnim;
    private AStar aStar;
    private StealthDetection detection;
    private Transform Player;

    private Vector3 moveDirection, targetPos;
    private float screenMaxZ, screenMinZ, screenMaxX, screenMinX;
    private bool targetIsPlayer;
    private States state;
    private DashState dashAttackState;
    private float timeSinceLastAttack, timeSinceLastDash;
    private float health;
    private float moveSpeed;
    private bool dashDoneSignal;
    private int curWaypoint;

    private const float rotSpeed = 20f;
    private const float startingHealth = 100f;
    private const float defaultMoveSpeed = 1f;
    private const float dashMoveSpeed = 5f;
    private const float runToPlayerSpeed = 2f;
    private const float attackDistance = 10f;
    private const float dashFrequency = 5f;
    private const float attackFrequency = 5f;
    private const float dashStopDistance = 0.5f;


    public void Start()
    {
        enemyAnim = GetComponent<Animator>();
        Player = DevRef.Player.transform;
        aStar = Player.GetComponent<AStar>();
        detection = GetComponent<StealthDetection>();

        moveDirection = Vector3.zero;
        targetPos = transform.position;
        targetIsPlayer = false;
        moveSpeed = defaultMoveSpeed;
        health = startingHealth;
        timeSinceLastAttack = 0f;
        timeSinceLastDash = 0f;
        dashDoneSignal = false;
        state = States.patrol;
        dashAttackState = DashState.NotInDash;
        curWaypoint = 0;

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
            moveSpeed = defaultMoveSpeed;
            targetIsPlayer = false;
            if (useWaypoints)
            {
                if (Vector3.Distance(transform.position, waypoints[curWaypoint].position) < 1f)
                {
                    if (curWaypoint < waypoints.Length - 1)
                    {
                        curWaypoint += 1;
                    }
                    else
                    {
                        curWaypoint = 0;
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
            targetIsPlayer = false;
            moveSpeed = defaultMoveSpeed;
            targetPos = detection.LastHeardPos;
            move();
        }

        if (state == States.runToPlayer)
        {
            moveSpeed = runToPlayerSpeed;
            alert();
            targetIsPlayer = false;
            targetPos = detection.LastSeenPlayerPos;
            move();

            detection.DisplayLastSeenPosition();
        }

        if (state == States.attack)
        {
            targetIsPlayer = false;
            moveSpeed = runToPlayerSpeed;
            if (timeSinceLastAttack > attackFrequency)
            {
                timeSinceLastAttack = 0f;
                Attack();
            }
        }

        Debug.Log(state);
    }

    private States stateDecision()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        bool seePlayer = detection.seePlayer();
        bool hearSomething = detection.hearSomething();
        detection.acknowledgeSound();
        bool atLastSeenPos = Vector3.Distance(detection.LastSeenPlayerPos, transform.position) < 2f;
        bool atLastHeardPos = Vector3.Distance(detection.LastHeardPos, transform.position) < 3f;

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
            if (dashAttackState.Equals(DashState.InDash))
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
        float enemySpeedGoal = 0f;
        if (moveDirection != Vector3.zero)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, moveDirection, rotSpeed * Time.deltaTime, 0.0f);
            Vector3 tempPos = transform.position + (1f * moveDirection);
            transform.position = Vector3.MoveTowards(transform.position, tempPos, moveSpeed * Time.deltaTime);
            enemySpeedGoal = moveSpeed / dashMoveSpeed * 4f;
        }
        enemyAnim.SetFloat("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat("enemySpeed"), enemySpeedGoal, 5f * Time.deltaTime));
    }

    public void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        timeSinceLastDash += Time.deltaTime;

        if (health <= 0)
            Destroy(gameObject);

        state = stateDecision();

        ExecuteStateActions();
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
        moveDirection = dir.normalized;
        targetPos = clampLoc(targetPos);
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
            if (Vector3.Distance(transform.position, obj.transform.position) < range)
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
        float curDistance = Vector3.Distance(detection.LastSeenPlayerPos, Player.position);
        float newDistance = Vector3.Distance(lastSeenPos, Player.position);
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
            while (Vector3.Distance(targetPos, transform.position) > 1f &&
                Time.time - startTime < maxDuration)
            {
                move();
                yield return null;
            }

            minRadius *= 1.5f;
            SetIdleAnimation();
        }

        targetPos = transform.position;
        moveSpeed = defaultMoveSpeed;
        state = States.patrol;
        detection.DestroyLastSeenGraphic();
    }

    private void Attack()
    {
        meleeAttack();
    }

    private void meleeAttack()
    {
        float distance = Vector3.Distance(transform.position, Player.position);
        if (timeSinceLastDash >= dashFrequency && dashAttackState.Equals(DashState.NotInDash))
            StartCoroutine(dash());
    }

    IEnumerator dash()
    {
        dashAttackState = DashState.InDash;
        timeSinceLastDash = 0f;
        dashDoneSignal = false;

        yield return new WaitForSeconds(0.3f);

        moveSpeed = dashMoveSpeed;
        enemyAnim.SetFloat("Speed", dashMoveSpeed/defaultMoveSpeed);

        Vector3 dir = (Player.position - transform.position).normalized;
        Vector3 posBehindPlayer = Player.position + (dir * 5f);
        targetPos = posBehindPlayer;

        yield return new WaitForSeconds(0.4f);

        float dashStart = Time.time;
        while (Vector3.Distance(transform.position, targetPos) > dashStopDistance)
        {
            if (dashDoneSignal) //true if we collide with player
                break;

            move();
            yield return null;
        }

        dashAttackState = DashState.NotInDash;
        moveSpeed = defaultMoveSpeed;
        enemyAnim.SetFloat("Speed", 1f);
    }

    float clampAngle(float orig)
    {
        while (orig > 180f)
            orig -= 360f;
        while (orig < -180f)
            orig += 360f;
        return orig;
    }
}