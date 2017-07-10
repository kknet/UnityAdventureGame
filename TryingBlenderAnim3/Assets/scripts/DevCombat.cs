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
		if (Input.GetKeyDown (KeyCode.Mouse0)) {
//			myAnimator.SetBool ("doAttack", true);
//			Invoke ("stopAttack", 0.3f);
		} else if (Input.GetKey (KeyCode.Mouse1)) {
			myAnimator.SetBool ("isBlocking", true);
		}

		if (Input.GetKeyUp (KeyCode.Mouse1)) {
			myAnimator.SetBool ("isBlocking", false);
		}

	}

	void stopAttack(){
		myAnimator.SetBool ("doAttack", false);
	}
}
