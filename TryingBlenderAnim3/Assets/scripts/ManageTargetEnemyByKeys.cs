using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageTargetEnemyByKeys : MonoBehaviour {

    DevCombat devCombat;
    GameObject[] enemies;
    Dictionary<GameObject, float> angles;
    Dictionary<GameObject, float> distances;

    private const float halfAngle = 70f;
    private const float priorityDistance = 7f;

    private void Start()
    {
        devCombat = GetComponent<DevCombat>();
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        InitDicts();
        devCombat.CurrentEnemy = PickTarget(transform.forward.normalized);
        ToggleOutlines(true);
    }

    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + (transform.forward * priorityDistance), Color.green);
    }

    public void UpdateTargetEnemy(Vector3 keyDir, bool lockedOnToATarget)
    {
        if (lockedOnToATarget)
        {
            //keep the target you already have
            return;
        }
        else
        {
            UpdateDicts(keyDir);

            if (devCombat.CurrentEnemy != null)
                ToggleOutlines(false);
            devCombat.CurrentEnemy = PickTarget(keyDir);
            ToggleOutlines(true);
        }
    }

    private void ToggleOutlines(bool on)
    {
        Outline[] allOutlines = devCombat.CurrentEnemy.GetComponentsInChildren<Outline>();
        foreach (Outline o in allOutlines)
            o.enabled = on;
    }

    private void InitDicts()
    {
        angles = new Dictionary<GameObject, float>();
        distances = new Dictionary<GameObject, float>();
        foreach (GameObject enemy in enemies)
        {
            angles.Add(enemy, AngleToEnemy(transform.forward, enemy));
            distances.Add(enemy, Vector3.Distance(transform.position, enemy.transform.position));
        }
    }

    private void UpdateDicts(Vector3 keyDir)
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;

            angles[enemy] = AngleToEnemy(keyDir, enemy);
            distances[enemy] = Vector3.Distance(transform.position, enemy.transform.position);
        }
    }

    private float AngleToEnemy(Vector3 fwd, GameObject enemy)
    {
        Vector3 dirToEnemy = enemy.transform.position - transform.position;
        dirToEnemy.Normalize();

        return Vector3.Angle(fwd, dirToEnemy);
    }

    private GameObject PickTarget(Vector3 dirFromPlayer)
    {
        GameObject chosen = null;
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (angles[enemy] <= halfAngle) //within angle
            {
                if (chosen != null)
                {
                    if (distances[enemy] < distances[chosen]) //for those within angle, decide by distance
                        chosen = enemy;
                }
                else if(distances[enemy] < priorityDistance)
                {
                    chosen = enemy;
                }
            }
        }

        if (chosen == null)
        {
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null)
                    continue;

                if (chosen != null)
                {
                    if (angles[enemy] < angles[chosen])
                        chosen = enemy;
                }
                else
                {
                    chosen = enemy;
                }
            }
        }
        if (chosen == null)
        {
            chosen = devCombat.CurrentEnemy;
            Debug.Log("Chosen Nothing");
        }

        return chosen;
    }
}