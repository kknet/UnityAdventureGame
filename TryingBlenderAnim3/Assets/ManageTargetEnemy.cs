using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageTargetEnemy : MonoBehaviour {

    DevCombat devCombat;
    GameObject[] enemies;
    List<TargetAnglePair> tapList;
    TargetAnglePair chosenTap;

    float lastChangeTime = 0f;
    float pollingLatency = 0.5f;
    float threshold = 1f;

    private void Start()
    {
        devCombat = GetComponent<DevCombat>();
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        tapList = new List<TargetAnglePair>();

        KeepListOfTargets();
        chosenTap = tapList[0];
        devCombat.CurrentEnemy = chosenTap.enemy;
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
        devCombat.CurrentEnemy = chosenTap.enemy;
    }

    private void KeepListOfTargets()
    {
        tapList.Clear();

        Vector3 playerFwd = transform.forward;
        foreach (GameObject enemy in enemies)
        {
            float angle = EnemyDevForwardAngleBetween(playerFwd, enemy);
            tapList.Add(new TargetAnglePair(enemy, angle));
        }

        tapList.Sort((x, y) => x.angle.CompareTo(y.angle));
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
        Vector3 enemyForward = transform.position - enemy.transform.position;
        return Vector3.SignedAngle(playerFwd, enemyForward, Vector3.up);
    }

}

class TargetAnglePair
{
    public GameObject enemy;
    public float angle;

    public TargetAnglePair(GameObject enemy, float angle)
    {
        this.enemy = enemy;
        this.angle = angle;
    }
}
