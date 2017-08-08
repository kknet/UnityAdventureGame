using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCombat : MonoBehaviour {
	public Animator myAnimator;

	public string savedAction;
	// Use this for initialization
	void Start () {
		myAnimator = GetComponent<Animator>();
		savedAction = "";
	}

	private bool movementButtonPressed(){
		return Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.A) 
			|| Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.D)
			|| Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.LeftArrow) 
			|| Input.GetKeyDown (KeyCode.RightArrow) || Input.GetKeyDown (KeyCode.DownArrow) 
			|| Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.A) 
			|| Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.D)
			|| Input.GetKey (KeyCode.UpArrow) || Input.GetKey (KeyCode.LeftArrow) 
			|| Input.GetKey (KeyCode.RightArrow) || Input.GetKey (KeyCode.DownArrow);
		
	}

	// Update is called once per frame
	void Update () {
		//if holding RMB, block

		//action was tried earlier, but dev was rotating, now is no longer rotating, so do action
		if (savedAction != "" && GetComponent<DevMovement> ().adjustCounter == 0) {
			myAnimator.SetBool (savedAction, true);
			if (savedAction == "doAttack") {
				Invoke ("switchAttack", 0.5f);
			}
			savedAction = "";
			return;
		}


		//still rotating character, so save action, and do action once rotation is over
		if (savedAction == "" && (GetComponent<DevMovement> ().adjustCounter != 0 || movementButtonPressed())) {
			if (Input.GetKey (KeyCode.Mouse1)) {
				myAnimator.SetBool ("doAttack", false);
				savedAction = "isBlocking";
			} else {
				myAnimator.SetBool ("isBlocking", false);		

				//otherwise, if clicked LMB, attack
				if (Input.GetKeyDown (KeyCode.Mouse0)) {
					savedAction = "doAttack";
				} else {
					myAnimator.SetBool ("doAttack", false);
				}
			}
			if(savedAction!="")
				return;
		}

		//dev is not rotating, and there is no saved action, so do action right now (normal way)
		if (savedAction == "") {
			if (Input.GetKey (KeyCode.Mouse1)) {
				myAnimator.SetBool ("doAttack", false);
				myAnimator.SetBool ("isBlocking", true);
			} else {
				myAnimator.SetBool ("isBlocking", false);		

				//otherwise, if clicked LMB, attack
				if (Input.GetKeyDown (KeyCode.Mouse0)) {
					myAnimator.SetBool ("doAttack", true);
					Invoke ("switchAttack", 0.5f);
				} else {
					myAnimator.SetBool ("doAttack", false);
				}
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

	public void switchAttack(){
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

	public void stopAttack(){
		myAnimator.SetBool ("doAttack", false);
	}
		
}
