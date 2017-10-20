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
	private AudioSource[] enemyAttackReactionSounds = new AudioSource[4];
	private float[] strongHitCrossFadeTimes = { 0.2f, 0.2f, 0.05f };
	private float spaceBarPressedTime, leftMousePressedTime, FPressedTime;
	private float twoButtonPressTimeMax = 0.1f;
	private float[] quickAttackOffsets = {1.8f, 1.8f, 1.0f};
	private float jumpAttackStartingOffset = 3.7f;
	private string[] quickAttackStateNames = {"quick_1", "quick_2", "quick_3"};
	AttackType currentType;

	float desiredOffset;

	enum AttackType {
		none,
		jump,
		quick
	};

	void Start () {
		myAnimator = GetComponent<Animator>();
		cam = Camera.main;
		lerpSpeedMultiplier = 2.0f;
		currentEnemy = GameObject.Find ("Brute2");
		enemyAttackReactionSounds[0] = quickAttack;
		enemyAttackReactionSounds[1] = quickAttack2;
		enemyAttackReactionSounds[2] = quickAttack3;
		enemyAttackReactionSounds[3] = quickAttack3;
		currentType = AttackType.none;
	}
	void Update () {
		handleAttacking ();
		handleInput ();
	}

	public GameObject getCurrentEnemy(){
		return currentEnemy;
	}

	#region handlingInput helpers
	private void handleLeftMousePressed(){
		if (Time.time - spaceBarPressedTime < twoButtonPressTimeMax) {
			leftMousePressedTime = 0f;
			spaceBarPressedTime = 0f;
			InitiateStepsToAttack (AttackType.jump);
			return;
		}

//		if (Time.time - FPressedTime < (twoButtonPressTimeMax)) {
//			leftMousePressedTime = 0f;
//			FPressedTime = 0f;
//			InitiateStepsToAttack (AttackType.flip);
//			return;
//		}
		leftMousePressedTime = Time.time;
	}

	private void handleSpaceBarPressed(){
		if (Time.time - leftMousePressedTime < twoButtonPressTimeMax) {
			spaceBarPressedTime = 0f;
			leftMousePressedTime = 0f;
			InitiateStepsToAttack (AttackType.jump);
			return;
		}
		spaceBarPressedTime = Time.time;
	}

//	private void handleFPressed(){
//		if (Time.time - leftMousePressedTime < (twoButtonPressTimeMax)) {
//			FPressedTime = 0f;
//			leftMousePressedTime = 0f;
//			InitiateStepsToAttack (AttackType.flip);
//			return;
//		}
//		leftMousePressedTime = Time.time;
//	}
	#endregion

	private void handleInput(){
		bool leftMousePressed = Input.GetKeyDown (KeyCode.Mouse0);
		bool leftMouseHeld = Input.GetKey (KeyCode.Mouse0);

		bool spaceBarPressed = Input.GetKeyDown (KeyCode.Space);
		bool spaceBarHeld = Input.GetKey (KeyCode.Space);

		bool rightMouseHeld = Input.GetKey (KeyCode.Mouse1);
		bool rightMouseReleased = Input.GetKeyUp (KeyCode.Mouse1);

//		bool FPressed = Input.GetKeyDown (KeyCode.F);
//		bool FHeld = Input.GetKeyDown (KeyCode.F);

		if (rightMouseReleased) {
			myAnimator.SetBool ("isBlocking", false);
		}

		if (leftMousePressedTime > 0f && (Time.time - leftMousePressedTime > twoButtonPressTimeMax)) {
			leftMousePressedTime = 0f;
			InitiateStepsToAttack (AttackType.quick);
		} else if (spaceBarPressedTime > 0f && (Time.time - spaceBarPressedTime > twoButtonPressTimeMax)) {
			spaceBarPressedTime = 0f;
			if (myAnimator.GetBool ("WeaponDrawn")) {
				myAnimator.SetBool ("roll", true);
				Invoke ("stopRolling", 1.0f);
			}
		}
//		else if (FPressedTime > 0f && (Time.time - FPressedTime > twoButtonPressTimeMax)) {
//			FPressedTime = 0f;
//		}


		if (rightMouseHeld) {
			stopAttack ();
			myAnimator.SetBool ("isBlocking", true);
		} else if (leftMousePressed && spaceBarPressed) {
			InitiateStepsToAttack (AttackType.jump);
		} 
//		else if (leftMousePressed && FPressed) {
//			InitiateStepsToAttack (AttackType.flip);
//		} 
		else if (leftMousePressed) {
			handleLeftMousePressed ();
		} else if (spaceBarPressed) {
			handleSpaceBarPressed ();
		} 
//		else if (FPressed) {
//			handleFPressed ();
//		}
	}


	private IEnumerator triggerJumpAttack(){
		while (needToAttack == true) {
			yield return new WaitForSeconds (0.01f);
		}
//		myAnimator.SetBool ("doJumpAttack", true);
		myAnimator.CrossFade("jump attack", 0.01f);
	}

	private IEnumerator triggerQuickAttack(){
		while (needToAttack == true) {
			yield return new WaitForSeconds (0.01f);
		}
		myAnimator.CrossFade(quickAttackStateNames[myAnimator.GetInteger("quickAttack")-1], 0.01f);
	}

	private void InitiateStepsToAttack(AttackType type){
		currentType = type;
		if (notInCombatMove () && closeEnoughToAttack ()) {
			if (type == AttackType.quick) {
				cam.GetComponent<MouseMovement> ().doCombatRotationOffset (true);
				startGettingIntoPosition ();
				desiredOffset = quickAttackOffsets[myAnimator.GetInteger("quickAttack")-1];
//				StartCoroutine (triggerQuickAttack());
				myAnimator.SetBool("doAttack", true);
			} else if (type == AttackType.jump) {
				cam.GetComponent<MouseMovement> ().doCombatRotationOffset (false);
				startGettingIntoPosition ();
				desiredOffset = jumpAttackStartingOffset;
				StartCoroutine (triggerJumpAttack());
			}

		}
	}

	private void handleAttacking(){
		if (needToAttack) {
			if (doneLerping) {
				needToAttack = false;
				myAnimator.SetFloat ("VSpeed", 0f); 
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

	void stopRolling(){
		myAnimator.SetBool ("roll", false);
	}

	#region helper methods to prepare for attack
	public void stopAttack(){
		myAnimator.SetBool ("doAttack", false);
		myAnimator.SetBool ("doFlipAttack", false);
		myAnimator.SetBool ("doJumpAttack", false);
		currentType = AttackType.none;
		needToAttack = false;
		lerpT = 0f;
		doneLerping = false;
	}
		
	void makeEnemyReact(int index){
//		cam.GetComponent<MouseMovement> ().getClosestEnemyObject().GetComponent<EnemyCombatAI> ().playReactAnimation (myAnimator.GetInteger("quickAttack"));
		currentEnemy.GetComponent<EnemyCombatAI> ().playReactAnimation (index);
	}
		
	bool closeEnoughToAttack(){
//		Vector3 totalVectorOffset = cam.GetComponent<MouseMovement> ().getClosestEnemyObject().transform.position - transform.position;
		Vector3 totalVectorOffset = getEnemyPos() - transform.position;
		totalVectorOffset = new Vector3 (totalVectorOffset.x, 0f, totalVectorOffset.z);
		float totalOffset = totalVectorOffset.magnitude;
		if (totalOffset > 10f)
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
	}

	Vector3 getEnemyPos(){
		return currentEnemy.transform.position;
	}

	void getIntoPositionForQuickAttack(){
		//		Vector3 totalVectorOffset = cam.GetComponent<MouseMovement> ().getClosestEnemyObject().transform.position - transform.position;
		Vector3 totalVectorOffset = getEnemyPos() - transform.position;
		totalVectorOffset = new Vector3 (totalVectorOffset.x, 0f, totalVectorOffset.z);
		float totalOffset = totalVectorOffset.magnitude;
		float remaining = totalOffset - desiredOffset;
		if (Mathf.Abs (remaining) < 0.01f) {
			doneLerping = true;
		} else {
			Vector3 deltaPos = totalVectorOffset.normalized * remaining;
			transform.position = Vector3.Lerp (transform.position, transform.position + deltaPos, lerpT);	
		}
	}

	void getIntoPositionForJumpAttack(){
			//		Vector3 totalVectorOffset = cam.GetComponent<MouseMovement> ().getClosestEnemyObject().transform.position - transform.position;
			Vector3 totalVectorOffset = getEnemyPos() - transform.position;
			totalVectorOffset = new Vector3 (totalVectorOffset.x, 0f, totalVectorOffset.z);
			float totalOffset = totalVectorOffset.magnitude;
			float remaining = totalOffset - desiredOffset;
		if (Mathf.Abs (remaining) < 0.01f) {
			doneLerping = true;
		} else {
			if (remaining > 0.5f) {
				if (remaining < 1.0f)
					myAnimator.SetFloat ("VSpeed", 0.5f);
				else
					myAnimator.SetFloat ("VSpeed", 1f);
			} else if (remaining < -0.5f) {				
				if (remaining < 1.0f)
					myAnimator.SetFloat ("VSpeed", -0.5f);
				else
					myAnimator.SetFloat ("VSpeed", -1f);
			}
			Vector3 deltaPos = totalVectorOffset.normalized * remaining;
			transform.position = Vector3.Lerp (transform.position, transform.position + deltaPos, lerpT * 0.1f);
		}
	}


	void getIntoPosition(){
		if (currentType == AttackType.quick)
			getIntoPositionForQuickAttack ();
		else if (currentType == AttackType.jump)
			getIntoPositionForJumpAttack ();
		else
			Debug.LogAssertion ("Bad current attack type detected in getIntoPosition()");
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
		return info.IsName ("quick_1") || info.IsName ("quick_2") || info.IsName ("quick_3") || info.IsName("jump attack") || info.IsName("flip attack");
	}
	#endregion

	#region sounds

	public void playQuickAttackSound(int index){
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
			enemyAttackReactionSounds [index - 1].Play ();
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
