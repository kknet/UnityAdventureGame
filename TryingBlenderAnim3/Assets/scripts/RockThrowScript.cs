using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockThrowScript : MonoBehaviour {

    [System.Serializable]
    public class RockTuple
    {
        public GameObject rockPrefab;
        public Transform spawnPos;
        public Transform floatPos;
        public float rightMultiplier;
    }

    private enum State
    {
        Idle,
        Rising,
        Floating,
        Striking
    }

    [HideInInspector] public bool startThrow;

    public List<RockTuple> rockTuples;

    Transform playerTransform;
    GameObject[] rocks;
    State[] rockStates;

    private bool testingThrow = true;

	void Start () {
        playerTransform = DevMain.Player.transform;
        rockStates = new State[rockTuples.Count];
        rocks = new GameObject[rockTuples.Count];

        int idx = 0;
        foreach (RockTuple tuple in rockTuples)
        {
            rockStates[idx++] = State.Idle;
            tuple.spawnPos.localPosition = tuple.spawnPos.position - transform.position;
            tuple.floatPos.localPosition = tuple.floatPos.position - transform.position;
        }

        startThrow = true;
	}

    bool allIdle()
    {
        foreach(State curState in rockStates)
            if (!curState.Equals(State.Idle))
                return false;
        return true;
    }

    void Update()
    {
        if (startThrow && allIdle())
        {
            startThrow = false;
            StartCoroutine(handleNewThrow());
        }
    }

    IEnumerator handleNewThrow()
    {
        for (int rockIdx = 0; rockIdx < rockTuples.Count; ++rockIdx)
        {
            StartCoroutine(RockThrow(rockIdx));

            for(int waitFrames = 0; waitFrames < 5; ++waitFrames)
                yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator RockThrow(int rockIdx)
    {
        if (!rockStates[rockIdx].Equals(State.Idle))
        {
            Debug.LogError("Already throwing rocks rn!");
            yield break;
        }

        yield return StartCoroutine(Rise(rockIdx));
        yield return StartCoroutine(Float(rockIdx));
        yield return StartCoroutine(Strike(rockIdx));
    }

    IEnumerator Rise(int rockIdx)
    {
        rockStates[rockIdx] = State.Rising;

        RockTuple tuple = rockTuples[rockIdx];
        GameObject newRock = GameObject.Instantiate(tuple.rockPrefab, transform.position, Quaternion.LookRotation(transform.forward), transform);
        newRock.transform.localPosition = tuple.spawnPos.localPosition;
        rocks[rockIdx] = newRock;

        Vector3 goalPos = tuple.floatPos.localPosition;
        Transform curRock = rocks[rockIdx].transform;
        float distance = float.MaxValue;
        while (curRock.gameObject && distance > 0.001f)
        {
            curRock.localPosition = Vector3.MoveTowards(curRock.localPosition, goalPos, Time.fixedDeltaTime * 0.5f);
            distance = Vector3.Distance(curRock.localPosition, goalPos);

            yield return new WaitForFixedUpdate();
        }

        //Debug.Log("Done Rising: " + rockIdx);
    }

    IEnumerator Float(int rockIdx)
    {
        rockStates[rockIdx] = State.Floating;

        int numFloatIterations = 2;
        float offsetMagnitude = 0.2f;
        float maxSpeed = 0.3f;
        for (int floatIteration = 0; floatIteration < numFloatIterations; ++floatIteration)
        {
            int negativeMultiplier = (floatIteration % 2 == 0) ? -1 : 1;
            Vector3 direction = Vector3.up.normalized * negativeMultiplier;
            Vector3 posOffset = offsetMagnitude * direction;

            Transform curRock = rocks[rockIdx].transform;
            Vector3 curPos = curRock.localPosition;
            Vector3 goalPos = curPos + posOffset;
            float speed = 0f;
            float distance = float.MaxValue;
            while (curRock.gameObject && distance > 0.001f)
            {
                float t = (1f - Mathf.Abs((distance - (offsetMagnitude / 2f)) / (offsetMagnitude / 2f)));
                speed = Mathf.Lerp(maxSpeed / 2f, maxSpeed, t);

                curRock.localPosition = Vector3.MoveTowards(curRock.localPosition, goalPos, Time.fixedDeltaTime * speed);
                distance = Vector3.Distance(curRock.localPosition, goalPos);

                yield return new WaitForFixedUpdate();
            }
        }

        //Debug.Log("Done Floating: " + rockIdx);
    }

    IEnumerator Strike(int rockIdx)
    {
        rockStates[rockIdx] = State.Striking;
        Vector3 playerPos = playerTransform.position;
        Vector3 middlePos = playerPos + (7f * playerTransform.forward) + (5f * playerTransform.up);
        Transform curRock = rocks[rockIdx].transform;
        Rigidbody rb = curRock.gameObject.GetComponent<Rigidbody>();
        curRock.parent = null;

        float speed = 20f;
        float goalSpeed = 100f;
        float turnSpeed = 8f;
        float goalTurnSpeed = 30f;
        float distance = float.MaxValue;
        bool shouldSpeedUp = Vector3.Distance(transform.position, playerPos) > 7f;

        if (!shouldSpeedUp)
        {
            speed = 15f;
            turnSpeed = 30f;
        }

        //curRock.transform.forward = Vector3.up;
        curRock.transform.forward = (Vector3.up + 
            (rockTuples[rockIdx].rightMultiplier * transform.right) ).normalized;

        while (curRock!= null && curRock.gameObject != null && distance > 1.3f/* && curRock.position.y > 0.3f*/)
        {
            Vector3 direction = (playerPos - rb.position).normalized;
            Vector3 rotateAmount = Vector3.Cross(direction, curRock.transform.forward);
            rb.angularVelocity = -rotateAmount * turnSpeed;
            rb.velocity = curRock.transform.forward * speed;

            if (shouldSpeedUp)
            {
                speed = Mathf.MoveTowards(speed, goalSpeed, Time.fixedDeltaTime * 50f);
                turnSpeed = Mathf.MoveTowards(turnSpeed, goalTurnSpeed, Time.fixedDeltaTime * 50f);
            }

            distance = Vector3.Distance(curRock.position, playerPos);
            //distance = Mathf.Abs(curRock.position.x - playerPos.x);
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForFixedUpdate();
        if(curRock != null)
            Destroy(curRock.gameObject);
        rockStates[rockIdx] = State.Idle;
        if (testingThrow)
            startThrow = true;
    }
}
