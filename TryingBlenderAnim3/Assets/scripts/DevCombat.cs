using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCombat : MonoBehaviour {

	#region globals
	public AudioSource quickAttack, quickAttack2, quickAttack3;
	public AudioSource strongHit;

	private Animator myAnimator;
	private Camera cam;
	private GameObject currentEnemy;
	private AudioSource[] enemyAttackReactionSounds;

	private float[] strongHitCrossFadeTimes, quickAttackOffsets;
	private string[] quickAttackStateNames;

	private float lerpT, lerpSpeedMultiplier, desiredOffset,
						 spaceBarPressedTime, leftMousePressedTime, 
						 FPressedTime, twoButtonPressTimeMax, 
						 jumpAttackStartingOffset;

	private bool needToAttack, doneLerping, needsRunningAnimation;


	AttackType currentType;
	#endregion

	enum AttackType {
		none,
		jump,
		quick
	};

	void Start () {
		myAnimator = GetComponent<Animator>();
		cam = Camera.main;
		currentEnemy = GameObject.Find ("Brute2");
		currentType = AttackType.none;
		quickAttackStateNames = new string[] {"quick_1", "quick_2", "quick_3"};
		enemyAttackReactionSounds = new AudioSource[] {quickAttack, quickAttack2, quickAttack3, quickAttack3};

		/*variables to tweak*/
		strongHitCrossFadeTimes = new float[]{ 0.2f, 0.2f, 0.05f };
		quickAttackOffsets = new float[]{1.8f, 1.8f, 1.0f};
		twoButtonPressTimeMax = 0.1f;
		jumpAttackStartingOffset = 3.7f;
	}
	void Update () {
		handleAttacking ();
		handleInput ();
	}
	void stopRolling(){
		myAnimator.SetBool ("roll", false);
	}

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

	private void handleAttacking(){
//		Debug.Log ("lerpT:" + lerpT);
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

	#region handleAttack helpers

	private void setLerpMultiplierByType(){
		if (currentType == AttackType.quick)
			lerpSpeedMultiplier = 0.3f;
		else if (currentType == AttackType.jump)
			lerpSpeedMultiplier = 0.5f;
		else
			Debug.LogAssertion ("Bad attack type detected in setLerpMultiplierByType()");
	}

	private IEnumerator triggerJumpAttack(){
//		while (needToAttack == true) {
//			yield return new WaitForSeconds (0.01f);
//		}

		while (needToAttack == true && lerpT < 0.6f) {
			yield return new WaitForSeconds (0.01f);
		}

		myAnimator.CrossFade("jump attack", 0.03f);
	}

	private IEnumerator triggerQuickAttack(){
//		while (needToAttack == true) {
//			yield return new WaitForSeconds (0.01f);
//		}

		while (needToAttack == true && lerpT < 0.8f) {
			yield return new WaitForSeconds (0.01f);
		}

		myAnimator.SetBool ("doAttack", true);
//		myAnimator.CrossFade(quickAttackStateNames[myAnimator.GetInteger("quickAttack")-1], 0.2f);
	}

	private void InitiateStepsToAttack(AttackType type){
		currentType = type;
		setLerpMultiplierByType ();

		bool canAttack = notInCombatMove () && closeEnoughToAttack ();
		if (!canAttack)
			return;
		
		if (type == AttackType.quick) {
			cam.GetComponent<MouseMovement> ().doCombatRotationOffset (true);
			startGettingIntoPosition ();
			desiredOffset = quickAttackOffsets[myAnimator.GetInteger("quickAttack")-1];
			StartCoroutine (triggerQuickAttack());
//			myAnimator.SetBool("doAttack", true);
		} else if (type == AttackType.jump) {
			cam.GetComponent<MouseMovement> ().doCombatRotationOffset (false);
			startGettingIntoPosition ();
			desiredOffset = jumpAttackStartingOffset;
			StartCoroutine (triggerJumpAttack());
		}
	}

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
//		cam.GetComponent<MouseMovement> ().getClosestEnemyObject().GetComponent<EnemyCombatReactions> ().playReactAnimation (myAnimator.GetInteger("quickAttack"));
		currentEnemy.GetComponent<EnemyCombatReactions> ().playReactAnimation (index);
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
		if (currentEnemy.GetComponent<EnemyCombatReactions> ().isBlocking () && currentEnemy.GetComponent<EnemyCombatReactions> ().rotationAllowsBlock()) {
//			myAnimator.SetBool ("hitStrong", true);
			myAnimator.CrossFade("sword_and_shield_impact_1", strongHitCrossFadeTimes[myAnimator.GetInteger("quickAttack")]);
		}
	}
	void startGettingIntoPosition(){
		needToAttack = true;
		lerpT = 0f;

	}


	void getIntoPosition(){
		if (currentType == AttackType.quick)
			getIntoPositionForQuickAttack ();
		else if (currentType == AttackType.jump)
			getIntoPositionForJumpAttack ();
		else
			Debug.LogAssertion ("Bad current attack type detected in getIntoPosition()");
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
			if (remaining > 0f) {
				if (remaining > 1f) {
					myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), 1f, 0.1f));
				}
				else
					myAnimator.SetFloat("VSpeed", remaining);
			} else if (remaining < 0f) {				
				if (remaining < -1f) {
					myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), -1f, 0.1f));
				}
				else
					myAnimator.SetFloat("VSpeed", remaining);
			}
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
			if (remaining > 0f) {
				if (remaining > 1f) {
					myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), 1f, 0.3f));
				}
				else
					myAnimator.SetFloat("VSpeed", remaining);
			} else if (remaining < 0f) {				
				if (remaining < -1f) {
					myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), -1f, 0.3f));
				}
				else
					myAnimator.SetFloat("VSpeed", remaining);
			}
			Vector3 deltaPos = totalVectorOffset.normalized * remaining;
			transform.position = Vector3.Lerp (transform.position, transform.position + deltaPos, lerpT);
		}
	}


	#endregion

	#region getters

	public GameObject getCurrentEnemy(){
		return currentEnemy;
	}

	Vector3 getEnemyPos(){
		return currentEnemy.transform.position;
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


		if (currentEnemy.GetComponent<EnemyCombatReactions> ().isBlocking () && currentEnemy.GetComponent<EnemyCombatReactions> ().rotationAllowsBlock()) {
			strongHit.Play ();
		} else {
			enemyAttackReactionSounds [index - 1].Play ();
		}
	}
	#endregion
}
