using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowScript : MonoBehaviour {

    public Transform arrowCanvas;
    public RectTransform arrowPrefab;
    public GameObject enemiesParent;
    private EnemyAI[] enemies;

	void Start () {
        enemies = enemiesParent.GetComponentsInChildren<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            if(enemy.isActiveAndEnabled)
                enemy.arrow = Instantiate(arrowPrefab, arrowCanvas);
        }
    }
	
	void Update () {
        arrowCanvas.position = transform.position + (Vector3.up * 2f);
        arrowCanvas.rotation = Camera.main.transform.rotation;
        foreach (EnemyAI enemy in enemies)
        {
            if(enemy.isActiveAndEnabled)
                UpdateArrow(enemy.arrow, enemy.transform.position);
        }
    }

    void UpdateArrow(RectTransform arrow, Vector3 targetPos)
    {
        //Vector3 direction = Vector3.Scale((targetPos - transform.position).normalized, EnemyAI.xzMask);
        //arrow.rotation = Quaternion.Euler(new Vector3(0f, Vector3.Angle(arrow.forward, direction), 0f));
        //arrow.localEulerAngles = new Vector3(0f, Vector3.Angle(arrow.forward, direction), 0f);
        targetPos = new Vector3(targetPos.x, arrow.eulerAngles.y, targetPos.z);
        arrow.LookAt(targetPos);
    }
}
