using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCombatReactions : MonoBehaviour
{

    public int health;
    public Image enemyHealthBar;

    private float maxHealth;
    private GameObject dev;
    private Animator enemyAnim;

    public void Init()
    {
        enemyAnim = this.gameObject.GetComponent<Animator>();
        dev = DevMain.Player;
        maxHealth = health;
    }

    public void FrameUpdate()
    {
        if (health <= 0)
            enemyAnim.SetBool("Dead", true);
        updateHealthBar();
    }

    private void updateHealthBar()
    {
        float ratio = health / maxHealth;
        if (ratio < 0f)
            ratio = 0f;
        enemyHealthBar.rectTransform.localScale = new Vector3(Mathf.MoveTowards(enemyHealthBar.rectTransform.localScale.x, ratio, 0.005f), 1f, 1f);
    }

    private float Clamp(float f)
    {
        if (f > 180f)
            return f - 360f;
        if (f < -180f)
            return f + 360f;
        return f;
    }

    public bool rotationAllowsBlock()
    {
        float myAngle = transform.eulerAngles.y; Clamp(myAngle);
        float devAngle = dev.transform.eulerAngles.y; Clamp(devAngle);
        float rotDifference;
        if (myAngle > devAngle)
            rotDifference = Mathf.Abs(myAngle - devAngle);
        else
            rotDifference = Mathf.Abs(devAngle - devAngle);


        return rotDifference > 110f && rotDifference < 250f;
    }

    public bool isBlocking()
    {
        AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo(0);
        return info.IsName("sword_and_shield_block_idle") || info.IsName("standing_block_react_large");
    }
}
