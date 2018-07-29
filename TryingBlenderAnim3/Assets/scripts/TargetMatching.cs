using UnityEngine;

// Ugly script, just used for demo/testing purposes!
public class TargetMatching : MonoBehaviour
{
    public bool UseTargetMatch = true;

    CharacterController characterController;
    DevCombat devCombat;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        devCombat = GetComponent<DevCombat>();
    }

    void Update()
    {
        Animator animator = GetComponent<Animator>();

        // We are turning...
        // Wait for our turn state to begin (there is a delay because I'm using blending to make things look nice)...
        //if (animator.GetCurrentAnimatorStateInfo(0).IsName("zombie_turn_2"))
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("attacking"))
        {
            // The magic happens here, unless disabled...
            if (UseTargetMatch)
            {
                // Calculate the correct rotation needed for us to face the player/target
                Quaternion correctRot = Quaternion.LookRotation(characterController.CombatLookDirection());

                animator.MatchTarget(devCombat.CurrentEnemy.transform.position, correctRot, AvatarTarget.Root,
                    new MatchTargetWeightMask(new Vector3(1, 1, 1), 0), // zero weight for the position, so it doesn't get adjusted at all.  one for rotation = 100% adjustment
                    /*animator.GetCurrentAnimatorStateInfo(0).normalizedTime*/ 0.2f, // we start adjustments at the current time, "right now"
                    1f); //so I want the adjustments to complete at this point in the animation (100%)
            }
        }
    }
}
