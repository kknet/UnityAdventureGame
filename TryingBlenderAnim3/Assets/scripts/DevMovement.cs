using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevMovement : MonoBehaviour {

	public GameObject player;
	public Transform CamTransform;
	public Animator myAnimator;
	public int adjustCounter;
	public AudioSource footstep1;
	public AudioSource footstep2;
	public AudioSource footstep3;
	public AudioSource footstep4;
	public AudioSource land;
	public AudioSource flipJump;
	public bool horizRot;

//	private int turnCounter;
//	private float turn;
	private float desiredRot;
	private bool applyJumpTrans;
	private float needToRot;
	private int runCounter;

	// Use this for initialization
	void Start () {
		myAnimator = GetComponent<Animator>();
		needToRot = 0;
		adjustCounter = 0;
//		turnCounter = 0;
		runCounter = 0;
		applyJumpTrans = false;
//		turn = 0f;
		desiredRot = Camera.main.transform.eulerAngles.y;
		horizRot = false;
	}

	public void adjustToCam(float dif, bool firstTimeAdjust)
	{
		if (!firstTimeAdjust && adjustCounter == 0)
			return;
		if(firstTimeAdjust)  {
			if (dif > 180f)
				dif = dif - 360f;
			else if (dif < -180f)
				dif = dif + 360f;

			needToRot = dif / 20.0f;
			adjustCounter = 20;
		}
		transform.Rotate (Vector3.up * needToRot);
		--adjustCounter;
	}

	private float clamp(float angle){
		if (angle < -180f) {
			angle += 360f;
			return clamp (angle);
		}
		else if (angle > 180f) {
			angle -= 360f;
			return clamp (angle);
		}

		return angle;
	}

	public bool isIdle()
	{
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);

		if (!anim.IsTag("Jumps") && myAnimator.GetFloat ("VSpeed") == 0 && myAnimator.GetFloat ("HorizSpeed") == 0 && myAnimator.GetBool ("shouldFrontFlip") == false && myAnimator.GetBool ("Jumping") == false)
		{
			return true;
		}
		return false;
	}

	public bool jumping(){
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
		return anim.IsTag ("Jumps");
	}

	public bool rolling(){
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
		return anim.IsTag ("roll");
	}

	public bool turning(){
		return !Mathf.Approximately(desiredRot, transform.eulerAngles.y);
	}

	private bool camRotChanged(){
		return !Mathf.Approximately (Camera.main.transform.eulerAngles.y, desiredRot);
	}


	void Update () {
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);

		if (anim.IsTag ("impact"))
			impactMoveBack ();

		bool inCombat = Camera.main.GetComponent<MouseMovement> ().inCombatZone;
		bool wepIsOut = Camera.main.GetComponent<MouseMovement> ().wepIsOut;

		if (rolling ()) {
			transform.Translate (Vector3.forward * Time.deltaTime * 4f);
		}

		if (inCombat && wepIsOut && !jumping()) {

			bool W = (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow));
			bool A = (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow));
			bool S = (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow));
			bool D = (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow));

			W = W && !S;
			S = S && !W;
			A = A && !D;
			D = D && !A;

			float angle = 0f;

			if (W && A)
				angle = -45f;
			else if (W && D)
				angle = 45f;
			else if (S && A)
				angle = -135f;
			else if (S && D)
				angle = 135f;
			else if (W)
				angle = 0f;
			else if (S)
				angle = 180f;
			else if (A)
				angle = -90f;
			else if (D)
				angle = 90f;

			if (W || A || S || D) {
				angle += CamTransform.eulerAngles.y;

				while (angle > 180f)
					angle -= 360f;
				while (angle < -180f)
					angle += 360f;

				int div = ((int)angle / 45);
				int rem = ((int)angle) % 45;

				if (rem >= 23)
					++div;

				angle = div * 45f;

				int intAngle = div * 45;
				int X = 0;
				int Y = 0;


				switch (intAngle) {
				case 0:
					{
						X = 1;
						Y = 0;
						break;
					}
				case 45:
					{
						X = 1;
						Y = 1;
						break;
					}
				case 90:
					{
						X = 0;
						Y = 1;
						break;
					}
				case 135:
					{
						X = -1;
						Y = 1;
						break;
					}
				case 180:
					{
						X = -1;
						Y = 0;
						break;
					}
				case -180:
					{
						X = -1;
						Y = 0;
						break;
					}

				case -135:
					{
						X = -1;
						Y = -1;
						break;
					}
				case -90:
					{
						X = 0;
						Y = -1;
						break;
					}
				case -45:
					{
						X = 1;
						Y = -1;
						break;
					}
				}

				myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), X * 1f, 4f * Time.deltaTime));
				myAnimator.SetFloat ("HorizSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("HorizSpeed"), Y * 1f, 4f * Time.deltaTime));
				transform.Translate (((Vector3.forward * X) + (Vector3.right * Y)) * Time.deltaTime * 1.5f);
			} else {
				myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), 0f, 4f * Time.deltaTime));
				myAnimator.SetFloat ("HorizSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("HorizSpeed"), 0f, 4f * Time.deltaTime));
			}
		} else {
			if (anim.IsTag ("Running")) {
				if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) {
					myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), Input.GetAxisRaw ("Vertical"), 0.1f)); 
				} else if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {
					myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), -1.0f * Input.GetAxisRaw ("Vertical"), 0.1f)); 
				} else if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
					myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), -1.0f * Input.GetAxisRaw ("Horizontal"), 0.1f));
				} else if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
					myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), Input.GetAxisRaw ("Horizontal"), 0.1f));
				} else {
					if(!horizRot)
						myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), Input.GetAxisRaw ("Vertical"), 0.1f)); 
				}
					transform.Translate (Vector3.forward * Time.deltaTime * 5f * myAnimator.GetFloat ("VSpeed"));
			}
		}
			
		if (Mathf.Approximately(myAnimator.GetFloat("VSpeed"), 0f) && Mathf.Approximately(myAnimator.GetFloat("HorizSpeed"), 0f)) {
			stopFootstepSound ();
		}

		if (applyJumpTrans && anim.IsTag("Jumps")) {
			if(anim.IsName("running_jump")) {
				transform.Translate (Vector3.forward * Time.deltaTime * 8f);
			} else {
				transform.Translate (Vector3.forward * Time.deltaTime * 12f);
			}
		}

		if (myAnimator.GetBool ("WeaponDrawn") && Input.GetButtonDown("Jump")) {
			myAnimator.SetBool ("roll", true);
			Invoke ("stopRolling", 1.0f);
		}

		if (anim.IsName ("quick_roll_to_run")) {
			transform.Translate (Vector3.forward * Time.deltaTime * 2f);
		}

		if(Input.GetButtonDown("Jump") && myAnimator.GetFloat("VSpeed") > 0f && adjustCounter == 0 
			&& player.GetComponent<DevCombat>().notInCombatMove())
		{
			myAnimator.SetBool("Jumping", true);
			Invoke ("stopJumping", 0.8f);
		}
		else if(Input.GetButtonDown("FrontFlip") && myAnimator.GetFloat("VSpeed") > 0f && adjustCounter == 0
			&& player.GetComponent<DevCombat>().notInCombatMove())
		{
			myAnimator.SetBool ("shouldFrontFlip", true);
			Invoke ("stopFrontFlip", 2.1f);
		}
		if(!inCombat && adjustCounter == 0 && (!Mathf.Approximately(myAnimator.GetFloat("VSpeed"), 0f) || !Mathf.Approximately(myAnimator.GetFloat("HorizSpeed"), 0f))) {
			if (!anim.IsTag("Jumps") && !myAnimator.GetBool ("Jumping") && !myAnimator.GetBool ("shouldFrontFlip") && player.GetComponent<DevCombat>().notInCombatMove()) {
				transform.Rotate (Vector3.up * Input.GetAxisRaw("Mouse X") * Time.deltaTime * Camera.main.GetComponent<MouseMovement>().sensitivityX);
			}
		}
	}

	public void impactMoveBack(){
		transform.Translate (Vector3.back * 0.5f * Time.deltaTime);
	}

	void rotateRight(){
		transform.Rotate(new Vector3(0f, 9f, 0f)); 
	}

	void rotateLeft(){
		transform.Rotate(new Vector3(0f, -9f, 0f));  
	}

	void runningSound(){
		if (runCounter == 0)
			footstep1.Play ();
		else if (runCounter == 1)
			footstep2.Play ();
		else if (runCounter == 2)
			footstep3.Play ();
		else if (runCounter == 3)
			footstep4.Play ();
		++runCounter;
		if (runCounter == 4)
			runCounter = 0;
	}

	void horizRunningSound(){
		if (!Mathf.Approximately(myAnimator.GetFloat ("VSpeed"), 0f))
			return;
		if (runCounter == 0)
			footstep1.Play ();
		else if (runCounter == 1)
			footstep2.Play ();
		else if (runCounter == 2)
			footstep3.Play ();
		else if (runCounter == 3)
			footstep4.Play ();
		++runCounter;
		if (runCounter == 4)
			runCounter = 0;
	}

	void onApplyTrans(){
		applyJumpTrans = true;
	}

	void offApplyTrans(){
		applyJumpTrans = false;
	}



	void flipTakeOffSound(){
		flipJump.Play ();
	}

	void landingSound(){
		land.Play ();
	}

	void stopRolling(){
		myAnimator.SetBool ("roll", false);
	}

	void stopJumping()
	{
		myAnimator.SetBool("Jumping", false);
		applyJumpTrans = false;
	}

	void stopFrontFlip()
	{
		myAnimator.SetBool ("shouldFrontFlip", false);
		applyJumpTrans = false;
	}

	void stopFootstepSound(){
		if (footstep1.isPlaying)
			footstep1.Stop ();
		if (footstep2.isPlaying)
			footstep2.Stop ();
		if (footstep3.isPlaying)
			footstep3.Stop ();
		if (footstep4.isPlaying)
			footstep4.Stop ();
	}


	void stopTurnRight(){
		myAnimator.SetBool ("TurnRight", false);
	}

	void stopTurnLeft(){
		myAnimator.SetBool ("TurnLeft", false);
	}
}