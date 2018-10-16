using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageTargetEnemyByMouse : MonoBehaviour {

    DevCombat devCombat;
    GameObject[] enemies;
    List<TargetAnglePair> tapList;
    TargetAnglePair chosenTap;

    float lastChangeTime = 0f;
    float pollingLatency = 0.5f;
    float threshold = 0.3f;

    private void Start()
    {
        devCombat = GetComponent<DevCombat>();
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        tapList = new List<TargetAnglePair>();

        KeepListOfTargets();
        chosenTap = tapList[0];
        devCombat.CurrentEnemy = chosenTap.Enemy;
        lastChangeTime = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (Time.realtimeSinceStartup - lastChangeTime < pollingLatency)
            return;

        float mouseX = InputController.controlsManager.GetAxis(ControlsManager.ButtonType.MouseX);
        if (Mathf.Abs(mouseX) < threshold)
            return;

        //Debug.Log("Mouse X: " + mouseX);

        //KeepListOfTargets();
        chosenTap = PickTarget(mouseX);
        devCombat.CurrentEnemy = chosenTap.Enemy;
    }

    private void KeepListOfTargets()
    {
        tapList.Clear();

        Vector3 playerFwd = transform.forward.normalized;
        foreach (GameObject enemy in enemies)
        {
            float angle = EnemyDevForwardAngleBetween(playerFwd, enemy);
            tapList.Add(new TargetAnglePair(enemy, angle));
        }

        tapList.Sort((x, y) => x.Angle.CompareTo(y.Angle));
    }

    

    private TargetAnglePair PickTarget(float mouseX)
    {
        int chosenIndex = tapList.IndexOf(chosenTap);
        if (mouseX > threshold)
            chosenIndex += 1;
        else if (mouseX < threshold)
            chosenIndex -= 1;

        //chosenIndex = Mathf.Clamp(chosenIndex, 0, enemies.Length - 1);

        chosenIndex = chosenIndex % enemies.Length;
        if (chosenIndex < 0) chosenIndex += enemies.Length;

        if(!tapList[chosenIndex].Equals(chosenTap))
            lastChangeTime = Time.realtimeSinceStartup;

        return tapList[chosenIndex];
    }

    private float EnemyDevForwardAngleBetween(Vector3 playerFwd, GameObject enemy)
    {
        Vector3 enemyForward = (transform.position - enemy.transform.position).normalized;
        return Vector3.SignedAngle(playerFwd, enemyForward, Vector3.up);
    }

}

public class TargetAnglePair
{
    private GameObject enemy;
    private float angle;

    public TargetAnglePair(GameObject enemy, float angle)
    {
        this.enemy = enemy;
        this.angle = angle;
    }

    public GameObject Enemy
    {
        get
        {
            return enemy;
        }
        set
        {
            enemy = value;
        }
    }

    public float Angle
    {
        get
        {
            return angle;
        }
        set
        {
            angle = value;
        }
    }
}
