using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEvents : MonoBehaviour
{

    public AudioSource footstep1,
                       footstep2,
                       footstep3,
                       footstep4,
                       land,
                       flipJump;

    [Tooltip("Particle effect for walking. Don't change!")]
    [SerializeField]
    public GameObject footDust;

    [SerializeField]
    public GameObject slashEffect1, slashEffect1Mirrored,
                      slashEffect2, slashEffect2Mirrored,
                      slashEffect3, slashEffect3Mirrored;

    [Tooltip("Feet transforms used for particle effect postioning. Don't change!")]
    [SerializeField]
    public Transform leftFoot, rightFoot;

    [Tooltip("Hand transforms used for particle effect postioning. Don't change!")]
    public Transform leftHand, rightHand;

    private Animator m_Animator;

    private int runCounter;
    private bool applyJumpTrans;
    private AudioSource[] footSteps;


    public void Init()
    {
        m_Animator = GetComponent<Animator>();
        footSteps = new AudioSource[]{footstep1, footstep2, footstep3, footstep4};
    }

    private float rand(float a, float b)
    {
        return UnityEngine.Random.Range(a, b);
    }

    #region sounds
    public void runningSound()
    {
        AudioSource chosenSource = footSteps[runCounter];
        chosenSource.pitch = rand(0.7f, 0.9f);
        chosenSource.Play();

        ++runCounter;
        if (runCounter == 4)
            runCounter = 0;
    }

    public void horizRunningSound()
    {
        if (!Mathf.Approximately(m_Animator.GetFloat("VSpeed"), 0f))
            return;

        AudioSource chosenSource = footSteps[runCounter];
        chosenSource.pitch = rand(0.7f, 0.9f);
        chosenSource.Play();

        ++runCounter;
        if (runCounter == 4)
            runCounter = 0;
    }

    public void flipTakeOffSound()
    {
        flipJump.Play();
    }

    public void landingSound()
    {
        land.Play();
    }

    public void stopFootstepSound()
    {
        Debug.Log("Running sound");
        if (footstep1.isPlaying)
            footstep1.Stop();
        if (footstep2.isPlaying)
            footstep2.Stop();
        if (footstep3.isPlaying)
            footstep3.Stop();
        if (footstep4.isPlaying)
            footstep4.Stop();
    }

    #endregion

    #region methods called by animation events
    public void playSlashEffect()
    {
        int attackIndex = m_Animator.GetInteger("quickAttack");
        bool mirrored = m_Animator.GetFloat("Mirrored") > 0f;
        Vector3 pos = Vector3.zero;
        Quaternion rot = transform.rotation;
        GameObject slashEffectClone = null;

        if (attackIndex == 1)
        {
            if (mirrored)
            {
                pos = transform.position + (-0.5f * transform.right) + (1.5f * transform.up);
                slashEffectClone = Instantiate(slashEffect1Mirrored, pos, rot, transform);
            }
            else
            {
                pos = transform.position + (0.25f * transform.right) + (1.5f * transform.up);
                slashEffectClone = Instantiate(slashEffect1, pos, rot, transform);
            }
        }
        else if (attackIndex == 2)
        {
            if (mirrored)
            {
                pos = transform.position + (-0.13f * transform.right) + (1.6f * transform.up);
                slashEffectClone = Instantiate(slashEffect2Mirrored, pos, rot, transform);
            }
            else
            {
                pos = transform.position + (0f * transform.right) + (1.75f * transform.up) + (-0.5f * transform.forward);
                slashEffectClone = Instantiate(slashEffect2, pos, rot, transform);
            }
        }
        else if (attackIndex == 3)
        {
            if (mirrored)
            {
                pos = transform.position + (-0.6f * transform.right) + (0.6f * transform.up) + (0.2f * transform.forward);
                slashEffectClone = Instantiate(slashEffect3Mirrored, pos, rot, transform);
            }
            else
            {
                pos = transform.position + (0.35f * transform.right) + (0.6f * transform.up) + (0.2f * transform.forward);
                slashEffectClone = Instantiate(slashEffect3, pos, rot, transform);
            }
        }

        slashEffectClone.GetComponent<ParticleSystem>().Play();
        Destroy(slashEffectClone, 1.0f);
    }

    public void spawnFootDust(int doLeftFoot)
    {
        GameObject footDustClone = null;
        Vector3 dustPos = Vector3.zero;

        if (doLeftFoot == 0)
            dustPos = leftFoot.position - (0.15f * leftFoot.right) + (0f * transform.forward) - (0f * transform.up);
        else
            dustPos = rightFoot.position + (0.1f * rightFoot.right) + (0f * transform.forward) - (0f * transform.up);

        //if (doLeftFoot == 0)
        //    dustPos = leftFoot.position - (0.25f * leftFoot.right) - (0.2f * transform.forward) - (0.1f * transform.up);
        //else
        //    dustPos = rightFoot.position + (0.2f * rightFoot.right) - (0.25f * transform.forward) - (0.3f * transform.up);


        footDustClone = Instantiate(footDust, dustPos, transform.rotation);
        footDustClone.GetComponent<ParticleSystem>().Play();

        Destroy(footDustClone, 2.0f);
    }

    public void onApplyTrans()
    {
        applyJumpTrans = true;
    }

    public void offApplyTrans()
    {
        applyJumpTrans = false;
    }

    public void stopJumping()
    {
        m_Animator.SetBool("Jumping", false);
        applyJumpTrans = false;
    }

    public void stopFrontFlip()
    {
        m_Animator.SetBool("shouldFrontFlip", false);
        applyJumpTrans = false;
    }
    #endregion


}
