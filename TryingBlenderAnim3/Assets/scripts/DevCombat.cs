using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCombat : MonoBehaviour {
	private Animator myAnimator;
	private Camera cam;
	private float lerpT, lerpSpeedMultiplier;
	private bool needToAttack, doneLerping;
	private GameObject brute1;

	void Start () {
		myAnimator = GetComponent<Animator>();
		cam = Camera.main;
		lerpSpeedMultiplier = 2.0f;
		brute1 = GameObject.Find ("Brute2");
	}
	void Update () {
		handleAttacking ();
		handleInput ();
	}

	private void handleInput(){
		if (Input.GetKey (KeyCode.Mouse1)) {
			stopAttack ();
			myAnimator.SetBool ("isBlocking", true);
		} else {
			myAnimator.SetBool ("isBlocking", false);		
			//otherwise, if clicked LMB, attack
			if (Input.GetKeyDown (KeyCode.Mouse0) && notInCombatMove ()) {
				if (closeEnoughToAttack ()) {
					startGettingIntoPosition ();
					return;
				}
				stopAttack ();
			}
		}
	}

	private void handleAttacking(){
		if (needToAttack) {
			if (doneLerping) {
				needToAttack = false;
			}
			else {
				lerpT += Time.deltaTime * lerpSpeedMultiplier;
				getIntoPosition ();
				if (lerpT >= 1.0f) {
					doneLerping = true;
					lerpT = 0f;
				}
			}
		}	
	}

	#region helper methods to prepare for attack
	public void stopAttack(){
		myAnimator.SetBool ("doAttack", false);
		needToAttack = false;
		lerpT = 0f;
		doneLerping = false;
	}

	void triggerAttack(){
		myAnimator.SetBool ("doAttack", true);
		Invoke ("switchAttack", 0.5f);
	}

	void makeEnemyReact(){
//		cam.GetComponent<MouseMovement> ().getClosestEnemyObject().GetComponent<EnemyCombatAI> ().playReactAnimation (myAnimator.GetInteger("quickAttack"));
		brute1.GetComponent<EnemyCombatAI> ().playReactAnimation (myAnimator.GetInteger("quickAttack"));
	}

	float offsetByAnimation(){
		//animation 1: 1.84
		//animation 2: 1.84
		//animation 3: 1.37
		switch (myAnimator.GetInteger ("quickAttack")){
		case 1:
			return 1.8f;
		case 2:
			return 1.8f;
		case 3:
			return 1.0f;
		default:
			Debug.LogAssertion ("Quick attack has bad value!");
			return 0;
		}
	}

	bool closeEnoughToAttack(){
//		Vector3 totalVectorOffset = cam.GetComponent<MouseMovement> ().getClosestEnemyObject().transform.position - transform.position;
		Vector3 totalVectorOffset = getEnemyPos() - transform.position;
		totalVectorOffset = new Vector3 (totalVectorOffset.x, 0f, totalVectorOffset.z);
		float totalOffset = totalVectorOffset.magnitude;
		if (totalOffset > 5f)
			return false;		
		return true;
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

	void startGettingIntoPosition(){
		needToAttack = true;
		lerpT = 0f;
		triggerAttack ();
	}

	Vector3 getEnemyPos(){
		return brute1.transform.position;
	}

	void getIntoPosition(){
//		Vector3 totalVectorOffset = cam.GetComponent<MouseMovement> ().getClosestEnemyObject().transform.position - transform.position;
		Vector3 totalVectorOffset = getEnemyPos() - transform.position;
		totalVectorOffset = new Vector3 (totalVectorOffset.x, 0f, totalVectorOffset.z);
		float totalOffset = totalVectorOffset.magnitude;
		float desiredOffset = offsetByAnimation ();
		float remaining = totalOffset - desiredOffset;
		if (Mathf.Abs (remaining) < 0.01f) {
			doneLerping = true;
		}
		else {
			Vector3 deltaPos = totalVectorOffset.normalized * remaining;
			transform.position = Vector3.Lerp (transform.position, transform.position + deltaPos, lerpT);
//			myAnimator.SetFloat ("VSpeed", remaining); 
		}
	}
	#endregion

	#region getters
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

	public bool notInCombatMove() {
		return !isAttacking() && !myAnimator.GetBool ("isBlocking");
	}

	public bool isAttacking() {
		AnimatorStateInfo info = myAnimator.GetCurrentAnimatorStateInfo (0);
		return info.IsName ("quick_1") || info.IsName ("quick_2") || info.IsName ("quick_3");
	}
	#endregion
}
