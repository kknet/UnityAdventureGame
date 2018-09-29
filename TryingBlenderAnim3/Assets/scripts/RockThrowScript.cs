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
    }

    private enum State
    {
        Idle,
        Rising,
        Floating,
        Traveling
    }

    public List<RockTuple> rockTuples;

    List<GameObject> rocks;
    List<Vector3> goalPositions;
    State rockState;


	void Start () {
        rockState = State.Idle;
        foreach (RockTuple tuple in rockTuples)
        {
            tuple.spawnPos.localPosition = tuple.spawnPos.position - transform.position;
            tuple.floatPos.localPosition = tuple.floatPos.position - transform.position;
        }
	}

    void Update()
    {
        if (rockState.Equals(State.Idle))
            StartCoroutine(RockThrow());
    }

    IEnumerator RockThrow()
    {
        if (!rockState.Equals(State.Idle))
        {
            Debug.LogError("Already throwing rocks rn!");
            yield break;
        }

        yield return StartCoroutine(Rise());
        yield return StartCoroutine(Float());
        yield return StartCoroutine(Travel());
    }

    IEnumerator Rise()
    {
        rockState = State.Rising;
        rocks = new List<GameObject>();
        goalPositions = new List<Vector3>();

        foreach (RockTuple tuple in rockTuples)
        {
            GameObject newRock = GameObject.Instantiate(tuple.rockPrefab, transform.position, Quaternion.LookRotation(Vector3.up), transform);
            newRock.transform.localPosition = tuple.spawnPos.localPosition;
            rocks.Add(newRock);
            goalPositions.Add(tuple.floatPos.localPosition);
        }

        float distance = float.MaxValue;

        while (distance > 0.001f)
        {
            distance = 0f;

            for (int rockIdx = 0; rockIdx < rocks.Count; ++rockIdx)
            {
                Transform curRock = rocks[rockIdx].transform;
                Vector3 goalPos = goalPositions[rockIdx];
                curRock.localPosition = Vector3.MoveTowards(curRock.localPosition, goalPos, Time.fixedDeltaTime * 0.5f);
                distance += Vector3.Distance(curRock.localPosition, goalPos);
            }

            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Done Rising");
    }

    IEnumerator Float()
    {
        rockState = State.Floating;

        int numFloatIterations = 5;
        float offsetMagnitude = 0.2f;
        float maxSpeed = 0.3f;
        for (int floatIteration = 0; floatIteration < 10; ++floatIteration)
        {
            float speed = 0f;
            int negativeMultiplier = (floatIteration % 2 == 0) ?  -1 : 1;
            Vector3 direction = Vector3.up.normalized * negativeMultiplier;
            Vector3 posOffset = offsetMagnitude * direction;

            for (int rockIdx = 0; rockIdx < rocks.Count; ++rockIdx)
            {
                Vector3 curPos = rocks[rockIdx].transform.localPosition;
                goalPositions[rockIdx] = curPos + posOffset;
            }

            float distance = float.MaxValue;
            while (distance > 0.001f)
            {
                
                float t = (1f - Mathf.Abs((distance - (offsetMagnitude / 2f)) / (offsetMagnitude / 2f)));
                speed = Mathf.Lerp(maxSpeed / 2f, maxSpeed, t);

                distance = 0f;
                for (int rockIdx = 0; rockIdx < rocks.Count; ++rockIdx)
                {
                    Transform curRock = rocks[rockIdx].transform;
                    Vector3 goalPos = goalPositions[rockIdx];
                    curRock.localPosition = Vector3.MoveTowards(curRock.localPosition, goalPos, Time.fixedDeltaTime * speed);
                    distance += Vector3.Distance(curRock.localPosition, goalPos);
                }

                yield return new WaitForFixedUpdate();
            }
        }

        Debug.Log("Done Floating");
    }

    IEnumerator Travel()
    {
        yield break;
    }
}
