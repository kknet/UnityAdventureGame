using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatAI : MonoBehaviour
{
    #region variables

    public bool setLookAtDev;
    public AudioSource quickAttack1, quickAttack2, quickAttack3, battleCry;
    public AudioSource strongHit;

    private GameObject dev;
    private Animator enemyAnim;
    private EnemyCheckHit enemyCheckHit;

    private AudioSource[] devAttackReactionSounds;

    #endregion

    public void Init()
    {
        enemyAnim = this.gameObject.GetComponent<Animator>();
        enemyCheckHit = GetComponent<EnemyCheckHit>();
        dev = DevMain.Player;

        devAttackReactionSounds = new AudioSource[] { quickAttack1, quickAttack3, quickAttack2, quickAttack3, quickAttack2 };
    }

    public void FrameUpdate()
    {
        if (setLookAtDev && !enemyCheckHit.recoveringFromHit &&
            enemyAnim.GetCurrentAnimatorStateInfo(0).IsTag("enemyRun"))
            lookAtDev();
    }

    private void lookAtDev()
    {
        Vector3 lookDirection = (dev.transform.position - transform.position).normalized;
        transform.forward = Vector3.RotateTowards(transform.forward, lookDirection, 20f * Time.deltaTime, 0.0f);
    }

    public void stopCastingSpell()
    {
        enemyAnim.SetBool("castingSpell", false);
    }

    public void toggleSuccess()
    {
        enemyAnim.SetBool("success", true);
    }

    #region sounds
    public void playQuickAttackSound(int index)
    {
        if (strongHit.isPlaying)
            strongHit.Stop();
        if (quickAttack2.isPlaying)
            quickAttack2.Stop();
        if (quickAttack1.isPlaying)
            quickAttack1.Stop();
        if (quickAttack3.isPlaying)
            quickAttack3.Stop();


        if (dev.GetComponent<DevCombatReactions>().isBlocking() && dev.GetComponent<DevCombatReactions>().rotationAllowsBlock())
        {
            strongHit.Play();
        }
        else
        {
            devAttackReactionSounds[index - 1].Play();
        }
    }


    public void playBattleCry()
    {
        if (strongHit.isPlaying)
            strongHit.Stop();
        if (quickAttack2.isPlaying)
            quickAttack2.Stop();
        if (quickAttack1.isPlaying)
            quickAttack1.Stop();
        if (quickAttack3.isPlaying)
            quickAttack3.Stop();
        if (battleCry.isPlaying)
            battleCry.Stop();

        battleCry.Play();
    }

    #endregion
}
