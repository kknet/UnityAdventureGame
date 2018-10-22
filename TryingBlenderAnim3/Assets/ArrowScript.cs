using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowScript : MonoBehaviour {

    public Transform arrowCanvas;
    public RectTransform arrowPrefab;
    public GameObject enemiesParent;
    private EnemyAI[] enemies;

    private const float maxSeeAngle = 60f;

	void Start () {
        enemies = enemiesParent.GetComponentsInChildren<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            if(enemy.isActiveAndEnabled)
                enemy.arrow = Instantiate(arrowPrefab, arrowCanvas);
        }
    }
	
	void Update () {
        arrowCanvas.position = transform.position + (Vector3.up * 1f);
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
        Vector3 target = targetPos;
        target.y = arrow.position.y;
        arrow.LookAt(target);
        arrow.eulerAngles = arrow.eulerAngles + new Vector3(90f, 180f, 0f);
        arrow.localPosition = Vector3.zero;
        arrow.GetChild(0).localPosition = (arrow.forward.normalized * 0.6f);

        if (needToDisplayArrow(targetPos))
            arrow.gameObject.SetActive(true);
        else
            arrow.gameObject.SetActive(false);
    }

    bool needToDisplayArrow(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        float angle = Vector3.Angle(dir, Camera.main.transform.forward);
        return angle > maxSeeAngle;
    }
}
