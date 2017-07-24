using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCombat : MonoBehaviour {
	public Animator myAnimator;


	// Use this for initialization
	void Start () {
		myAnimator = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update () {
//		int layerIndex = 0;
//		Debug.Log (myAnimator.GetCurrentAnimatorStateInfo (0).fullPathHash);

//		Debug.Log ("in combat: " + !notInCombatMove());

		//if attack button is pressed while an attack is already ongoing, ignore the button press

//		if(!myAnimator.GetBool("WeaponDrawn")){
//			myAnimator.SetBool ("doAttack", false);
//			myAnimator.SetBool ("isBlocking", false);
//			return;	
//		} 

		//if holding RMB, block
		if (Input.GetKey (KeyCode.Mouse1)) {
			myAnimator.SetBool ("doAttack", false);
			myAnimator.SetBool ("isBlocking", true);
		} else {
			myAnimator.SetBool ("isBlocking", false);		

			//otherwise, if clicked LMB, attack
			if (Input.GetKeyDown (KeyCode.Mouse0)) {
				myAnimator.SetBool ("doAttack", true);
			} else {
				myAnimator.SetBool ("doAttack", false);
			}
		}
	}

	public bool notInCombatMove() {
		return !isAttacking() && !myAnimator.GetBool ("isBlocking");
	}

	public bool isAttacking() {
		AnimatorStateInfo info = myAnimator.GetCurrentAnimatorStateInfo (0);
		return info.IsName ("quick_1") || info.IsName ("quick_2") || info.IsName ("quick_3");
	}

	public void stopAttack(){
		myAnimator.SetBool ("doAttack", false);
		switch (myAnimator.GetInteger ("quickAttack")) {
			case 1:
				myAnimator.SetInteger ("quickAttack", 2);
				break;
			case 2:
				myAnimator.SetInteger ("quickAttack", 3);
				break;
			case 3:
				myAnimator.SetInteger ("quickAttack", 1);
				break;
			default:
				Debug.LogAssertion ("quickAttack is not set to 1-3, look at DevCombat.cs script");
				break;
		}
	}
}
