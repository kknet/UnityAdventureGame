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
		//if attack button is pressed while an attack is already ongoing, ignore the button press

		if (Input.GetKeyDown (KeyCode.Mouse0) && !myAnimator.GetBool ("doAttack") && !myAnimator.GetBool("isBlocking")) {

			myAnimator.SetBool ("doAttack", true);
			switch (myAnimator.GetInteger ("quickAttack")) {
			case 1:
				Invoke ("stopAttack", 1.14f);
				break;
			case 2:
				Invoke ("stopAttack", 1.1f);
				break;
			case 3:
				Invoke ("stopAttack", 0.97f);
				break;
			default:
				Debug.LogAssertion ("quickAttack is not set to 1-3, look at DevCombat.cs script");
				break;
			}

		} else if (Input.GetKey (KeyCode.Mouse1)) {
			myAnimator.SetBool ("doAttack", false);
			myAnimator.SetBool ("isBlocking", true);
		}

		if (Input.GetKeyUp (KeyCode.Mouse1)) {
			myAnimator.SetBool ("isBlocking", false);
		}

	}

	void stopAttack(){
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
