using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCombat : MonoBehaviour {
	public Animator myAnimator;

	private Camera cam;
	private float lerpT = 0f;
	private bool needToAttack = false;
	private bool doneLerping = false;

	// Use this for initialization
	void Start () {
		myAnimator = GetComponent<Animator>();
		cam = Camera.main;
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
		if (needToAttack) {
			if (doneLerping) {
				triggerAttack ();
				needToAttack = false;
			}
			else {
				lerpT += Time.deltaTime * 1;
				getIntoPosition ();
				if (lerpT >= 0.5f) {
					doneLerping = true;
					lerpT = 0f;
				}
			}
		}

		//dev is not rotating, and there is no saved action, so do action right now (normal way)
		if (Input.GetKey (KeyCode.Mouse1)) {
			stopAttack();
			myAnimator.SetBool ("isBlocking", true);
		} else {
			myAnimator.SetBool ("isBlocking", false);		
			//otherwise, if clicked LMB, attack
			if (Input.GetKeyDown (KeyCode.Mouse0) && notInCombatMove()) {
				if (closeEnoughToAttack ()) {
					startGettingIntoPosition ();
					return;
				}
				stopAttack ();
			}
		}
	}

	public void stopAttack(){
		myAnimator.SetBool ("doAttack", false);
		needToAttack = false;
		lerpT = 0f;
		doneLerping = false;
	}

	void triggerAttack(){
		myAnimator.SetBool ("doAttack", true);
		switch (myAnimator.GetInteger ("quickAttack")){
		case 1:
			Invoke ("makeEnemiesReact", 0.38f);
			break;
		case 2:
			Invoke("makeEnemiesReact", 0.3f);
			break;
		case 3:
			Invoke("makeEnemiesReact", 0.45f);
			break;
		default:
			break;
		}

		Invoke ("switchAttack", 0.5f);
	}

	void makeEnemiesReact(){
		GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");
		foreach (GameObject enemy in enemies)
			enemy.GetComponent<EnemyCombatAI> ().playReactAnimation ();
	}

	//animation 1: 1.84
	//animation 2: 1.84
	//animation 3: 1.37
	float offsetByAnimation(){
		switch (myAnimator.GetInteger ("quickAttack")){
		case 1:
			return 2.3f;
//			return 1.84f;
		case 2:
			return 2f;
//			return 1.84f;
		case 3:
			return 1.37f;
		default:
			Debug.LogAssertion ("Quick attack has bad value!");
			return 0;
		}
	}

	bool closeEnoughToAttack(){
		Vector3 totalVectorOffset = cam.GetComponent<MouseMovement> ().closestEnemy - transform.position;
		totalVectorOffset = new Vector3 (totalVectorOffset.x, 0f, totalVectorOffset.z);
		float totalOffset = totalVectorOffset.magnitude;
		if (totalOffset > 10f)
			return false;		
		return true;
	}

	void startGettingIntoPosition(){
		needToAttack = true;
		lerpT = 0f;
		return;
	}

	void getIntoPosition(){
		Vector3 totalVectorOffset = cam.GetComponent<MouseMovement> ().closestEnemy - transform.position;
		totalVectorOffset = new Vector3 (totalVectorOffset.x, 0f, totalVectorOffset.z);
		float totalOffset = totalVectorOffset.magnitude;
		float desiredOffset = offsetByAnimation ();
		float remaining = totalOffset - desiredOffset;
		if (Mathf.Abs (remaining) < 0.01f)
			doneLerping = true;
		else
			transform.position = Vector3.Lerp (transform.position, transform.position + (totalVectorOffset.normalized * remaining), lerpT);
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
}
