using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCombat : MonoBehaviour {
	public AudioSource quickAttack, quickAttack2, quickAttack3;
	public AudioSource strongHit;

	private Animator myAnimator;
	private Camera cam;
	private float lerpT, lerpSpeedMultiplier;
	private bool needToAttack, doneLerping;
	private GameObject currentEnemy;
	private AudioSource[] enemyQuickAttackSounds = new AudioSource[3];
	private float[] strongHitCrossFadeTimes = { 0.2f, 0.2f, 0.05f };
	private float spaceBarPressedTime, leftMousePressedTime, FPressedTime;

	private float twoButtonPressTimeMax = 0.1f;

	void Start () {
		myAnimator = GetComponent<Animator>();
		cam = Camera.main;
		lerpSpeedMultiplier = 2.0f;
		currentEnemy = GameObject.Find ("Brute2");
		enemyQuickAttackSounds[0] = quickAttack;
		enemyQuickAttackSounds[1] = quickAttack2;
		enemyQuickAttackSounds[2] = quickAttack3;
	}
	void Update () {
		handleAttacking ();
		handleInput ();
	}

	private void handleLeftMousePressed(){
		if (Time.time - spaceBarPressedTime < twoButtonPressTimeMax) {
			leftMousePressedTime = 0f;
			spaceBarPressedTime = 0f;
			myAnimator.SetBool ("doJumpAttack", true);
			return;
		}

		if (Time.time - FPressedTime < (twoButtonPressTimeMax)) {
			leftMousePressedTime = 0f;
			FPressedTime = 0f;
			myAnimator.SetBool ("doFlipAttack", true);
			return;
		}
		leftMousePressedTime = Time.time;
	}

	private void handleSpaceBarPressed(){
		if (Time.time - leftMousePressedTime < twoButtonPressTimeMax) {
			spaceBarPressedTime = 0f;
			leftMousePressedTime = 0f;
			myAnimator.SetBool ("doJumpAttack", true);
			return;
		}
		spaceBarPressedTime = Time.time;
	}

	private void handleFPressed(){
		if (Time.time - leftMousePressedTime < (twoButtonPressTimeMax)) {
			FPressedTime = 0f;
			leftMousePressedTime = 0f;
			myAnimator.SetBool ("doFlipAttack", true);
			return;
		}
		leftMousePressedTime = Time.time;
	}

	private void handleInput(){
		bool leftMousePressed = Input.GetKeyDown (KeyCode.Mouse0);
		bool leftMouseHeld = Input.GetKey (KeyCode.Mouse0);

		bool spaceBarPressed = Input.GetKeyDown (KeyCode.Space);
		bool spaceBarHeld = Input.GetKey (KeyCode.Space);

		bool rightMouseHeld = Input.GetKey (KeyCode.Mouse1);
		bool rightMouseReleased = Input.GetKeyUp (KeyCode.Mouse1);

		bool FPressed = Input.GetKeyDown (KeyCode.F);
		bool FHeld = Input.GetKeyDown (KeyCode.F);

		if (rightMouseReleased) {
			myAnimator.SetBool ("isBlocking", false);
		}

		if (leftMousePressedTime > 0f && (Time.time - leftMousePressedTime > twoButtonPressTimeMax)) {
			leftMousePressedTime = 0f;
			if (notInCombatMove () && closeEnoughToAttack ()) {
				startGettingIntoPosition ();
				return;
			}
			stopAttack ();
		} else if (spaceBarPressedTime > 0f && (Time.time - spaceBarPressedTime > twoButtonPressTimeMax)) {
			spaceBarPressedTime = 0f;
			if (myAnimator.GetBool ("WeaponDrawn")) {
				myAnimator.SetBool ("roll", true);
				Invoke ("stopRolling", 1.0f);
			}
		} else if (FPressedTime > 0f && (Time.time - FPressedTime > twoButtonPressTimeMax)) {
			FPressedTime = 0f;
		}



		if (rightMouseHeld) {
			stopAttack ();
			myAnimator.SetBool ("isBlocking", true);
		} else if (leftMousePressed && spaceBarPressed) {
			myAnimator.SetBool ("doJumpAttack", true);
		} else if (leftMousePressed && FPressed) {
			myAnimator.SetBool ("doFlipAttack", true);
		} else if (leftMousePressed) {
			handleLeftMousePressed ();
		} else if (spaceBarPressed) {
			handleSpaceBarPressed ();
		} else if (FPressed) {
			handleFPressed ();
		}
	}

	void stopRolling(){
		myAnimator.SetBool ("roll", false);
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
//		Invoke ("switchAttack", 0.5f);
	}

	void makeEnemyReact(){
//		cam.GetComponent<MouseMovement> ().getClosestEnemyObject().GetComponent<EnemyCombatAI> ().playReactAnimation (myAnimator.GetInteger("quickAttack"));
		currentEnemy.GetComponent<EnemyCombatAI> ().playReactAnimation (myAnimator.GetInteger("quickAttack"));
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

	public void setHitStrong(){
		if (currentEnemy.GetComponent<EnemyCombatAI> ().isBlocking ()) {
//			myAnimator.SetBool ("hitStrong", true);
			myAnimator.CrossFade("sword_and_shield_impact_1", strongHitCrossFadeTimes[myAnimator.GetInteger("quickAttack")]);
		}
	}

	void startGettingIntoPosition(){
		needToAttack = true;
		lerpT = 0f;
		triggerAttack ();
	}

	Vector3 getEnemyPos(){
		return currentEnemy.transform.position;
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

	#region sounds

	public void playQuickAttackSound(){
		if (strongHit.isPlaying)
			strongHit.Stop ();
		if(quickAttack2.isPlaying)
			quickAttack2.Stop();
		if(quickAttack.isPlaying)
			quickAttack.Stop();
		if (quickAttack3.isPlaying)
			quickAttack3.Stop ();


		if (currentEnemy.GetComponent<EnemyCombatAI> ().isBlocking ()) {
			strongHit.Play ();
		} else {
			enemyQuickAttackSounds [myAnimator.GetInteger ("quickAttack") - 1].Play ();
		}
	}
		
//	public void playQuickAttackSound(){
//		if(quickAttack2.isPlaying)
//			quickAttack2.Stop();
//		if(quickAttack.isPlaying)
//			quickAttack.Stop();
//		if (quickAttack3.isPlaying)
//			quickAttack3.Stop ();
//
//		quickAttack.Play ();
//	}
//
//	public void playQuickAttackSound2(){
//		if(quickAttack2.isPlaying)
//			quickAttack2.Stop();
//		if(quickAttack.isPlaying)
//			quickAttack.Stop();
//		if (quickAttack3.isPlaying)
//			quickAttack3.Stop ();
//
//		quickAttack2.Play ();
//	}
//
//	public void playQuickAttackSound3(){
//		if(quickAttack2.isPlaying)
//			quickAttack2.Stop();
//		if(quickAttack.isPlaying)
//			quickAttack.Stop();
//		if (quickAttack3.isPlaying)
//			quickAttack3.Stop ();
//
//		quickAttack3.Play ();
//	}


	#endregion
}
