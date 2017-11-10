using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatAI : MonoBehaviour {



	#region variables

	public bool setBlocking, setLookAtDev;
	public AudioSource quickAttack1, quickAttack2, quickAttack3, battleCry;
	public AudioSource strongHit;

	private GameObject dev;
	private Animator enemyAnim;

	private AudioSource[] devAttackReactionSounds;
	private float[] strongHitCrossFadeTimes, 
					quickAttackOffsets;

	private bool needToAttack, doneLerping;
	private float lerpSpeedMultiplier, lerpT, desiredOffset;

	#endregion
	// Use this for initialization
	void Start () {
		enemyAnim = this.gameObject.GetComponent<Animator> ();
		dev = GameObject.Find ("DevDrake");
		enemyAnim.SetBool ("enemyBlock", setBlocking);

		devAttackReactionSounds = new AudioSource[] {quickAttack1, quickAttack3, quickAttack2, quickAttack3, quickAttack2};

		/*variables to tweak*/
		strongHitCrossFadeTimes = new float[]{ 0.05f, 0.05f, 0.05f , 0.05f, 0.05f};
		quickAttackOffsets = new float[]{1.5f, 1.4f, 1.5f, 1.5f, 1.4f};

	}

	// Update is called once per frame
	void Update () {
		handleTestingInput ();

		if(setLookAtDev && !notInCombatMove())
			lookAtDev ();

		handleAttacking ();
	}
		
	public void playBattleCry(){
		if (strongHit.isPlaying)
			strongHit.Stop ();
		if(quickAttack2.isPlaying)
			quickAttack2.Stop();
		if(quickAttack1.isPlaying)
			quickAttack1.Stop();
		if (quickAttack3.isPlaying)
			quickAttack3.Stop ();
		if (battleCry.isPlaying)
			battleCry.Stop ();

		battleCry.Play ();
	}

	public void toggleSuccess(){
		enemyAnim.SetBool ("success", true);
	}

	private void handleAttacking(){
		//		Debug.Log ("lerpT:" + lerpT);
		if (needToAttack) {
			if (doneLerping) {
				needToAttack = false;
				enemyAnim.SetFloat ("enemySpeed", 0f); 
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


	#region handleAttack helpers

	private IEnumerator triggerQuickAttack(){
		while (needToAttack == true && lerpT < 0.8f) {
			yield return new WaitForSeconds (0.01f);
		}

		enemyAnim.SetBool ("enemyAttack", true);
	}

	private void InitiateStepsToAttack(){
		lerpSpeedMultiplier = 0.2f;

		bool canAttack = notInCombatMove () && closeEnoughToAttack ();
		if (!canAttack)
			return;

		startGettingIntoPosition ();
		desiredOffset = quickAttackOffsets[enemyAnim.GetInteger("enemyQuick")-1];
		StartCoroutine (triggerQuickAttack());
	}

	public void stopEnemyAttack(){
		enemyAnim.SetBool ("enemyAttack", false);
		needToAttack = false;
		lerpT = 0f;
		doneLerping = false;
	}

	void makeDevReact(int index){
		dev.GetComponent<DevCombatReactions> ().playReactAnimation (index);
	}

	bool closeEnoughToAttack(){
		//		Vector3 totalVectorOffset = cam.GetComponent<MouseMovement> ().getClosestEnemyObject().transform.position - transform.position;
		Vector3 totalVectorOffset = dev.transform.position - transform.position;
		totalVectorOffset = new Vector3 (totalVectorOffset.x, 0f, totalVectorOffset.z);
		float totalOffset = totalVectorOffset.magnitude;
		if (totalOffset > 10f)
			return false;		
		return true;
	}

	public void setHitStrong(){
		bool isBlocking = dev.GetComponent<DevCombatReactions> ().isBlocking ();
		bool rotationAllows = dev.GetComponent<DevCombatReactions> ().rotationAllowsBlock ();
		if (isBlocking && rotationAllows) {
			enemyAnim.CrossFade ("sword_and_shield_impact_1", strongHitCrossFadeTimes [enemyAnim.GetInteger ("enemyQuick") - 1]);
		} else {
//			Debug.Log (isBlocking + " " + rotationAllows);	
		}
	}

	void startGettingIntoPosition(){
		needToAttack = true;
		lerpT = 0f;
	}


	void getIntoPosition(){
		getIntoPositionForQuickAttack ();
	}

	void getIntoPositionForQuickAttack(){
		//		Vector3 totalVectorOffset = cam.GetComponent<MouseMovement> ().getClosestEnemyObject().transform.position - transform.position;
		Vector3 totalVectorOffset = dev.transform.position - transform.position;
		totalVectorOffset = new Vector3 (totalVectorOffset.x, 0f, totalVectorOffset.z);
		float totalOffset = totalVectorOffset.magnitude;
		float remaining = totalOffset - desiredOffset;
		if (Mathf.Abs (remaining) < 0.01f) {
			doneLerping = true;
		} else {
			if (remaining > 0f) {
				if (remaining > 1f) {
					enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards (enemyAnim.GetFloat ("enemySpeed"), 1f, 0.1f));
				}
				else
					enemyAnim.SetFloat("enemySpeed", remaining);
			} else if (remaining < 0f) {				
				if (remaining < -1f) {
					enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards (enemyAnim.GetFloat ("enemySpeed"), -1f, 0.1f));
				}
				else
					enemyAnim.SetFloat("enemySpeed", remaining);
			}
			Vector3 deltaPos = totalVectorOffset.normalized * remaining;
			transform.position = Vector3.Lerp (transform.position, transform.position + deltaPos, lerpT);	
		}
	}

	private void lookAtDev(){
		Vector3 lookDirection = (dev.transform.position - transform.position).normalized;
		Vector3 offset = (transform.right * 0.4f);
		transform.forward = Vector3.RotateTowards (transform.forward, lookDirection + offset, 20f * Time.deltaTime, 0.0f);
	}

	#endregion

	#region sounds
	public void playQuickAttackSound(int index){
		if (strongHit.isPlaying)
			strongHit.Stop ();
		if(quickAttack2.isPlaying)
			quickAttack2.Stop();
		if(quickAttack1.isPlaying)
			quickAttack1.Stop();
		if (quickAttack3.isPlaying)
			quickAttack3.Stop ();


		if (dev.GetComponent<DevCombatReactions> ().isBlocking () && dev.GetComponent<DevCombatReactions> ().rotationAllowsBlock()) {
			strongHit.Play ();
		} else {
			devAttackReactionSounds [index - 1].Play ();
		}
	}
	#endregion

	#region debugging/testing
	private void handleTestingInput(){
		if (isAttacking ())
			return;

		//call taunt animation on equals sign press
		if (Input.GetKeyDown (KeyCode.Equals)) {
			enemyAnim.SetBool ("doBattlecry", true);
		}

		//call attack animations on number presses
		else if (Input.GetKeyDown (KeyCode.Alpha6)) {
			enemyAnim.SetInteger ("enemyQuick", 1);
			InitiateStepsToAttack ();
		} else if (Input.GetKeyDown (KeyCode.Alpha7)) {
			enemyAnim.SetInteger ("enemyQuick", 2);
			InitiateStepsToAttack ();
		} else if (Input.GetKeyDown (KeyCode.Alpha8)) {
			enemyAnim.SetInteger ("enemyQuick", 3);
			InitiateStepsToAttack ();
		} else if (Input.GetKeyDown (KeyCode.Alpha9)) {
			enemyAnim.SetInteger ("enemyQuick", 4);
			InitiateStepsToAttack ();
		} else if (Input.GetKeyDown (KeyCode.Alpha0)) {
			enemyAnim.SetInteger ("enemyQuick", 5);
			InitiateStepsToAttack ();
		}
	}
	#endregion

	public bool notInCombatMove() {
		return !isAttacking() && !enemyAnim.GetBool ("enemyBlock");
	}

	public bool isAttacking() {
		AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo (0);
		return info.IsName ("QUICK1") || info.IsName ("QUICK2") || info.IsName ("QUICK3") || info.IsName("QUICK4") || info.IsName("QUICK5");
	}
		
}
