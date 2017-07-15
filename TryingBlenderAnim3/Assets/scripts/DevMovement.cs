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

	private bool applyJumpTrans;
	private float needToRot;
	private int runCounter;


	// Use this for initialization
	void Start () {
		myAnimator = GetComponent<Animator>();
		needToRot = 0;
		adjustCounter = 0;
		runCounter = 0;
		applyJumpTrans = false;
	}

	public void adjustToCam(float dif, bool firstTimeAdjust)
	{
		if (!firstTimeAdjust && adjustCounter == 0)
			return;
		if (Mathf.Approximately (dif, 0f)) {
			adjustCounter = 0;
			return;
		}
		if(firstTimeAdjust)  {
			if (dif > 180f)
				dif = dif - 360f;
			else if (dif < -180f)
				dif = dif + 360f;
			needToRot = dif / 15.0f;
			adjustCounter = 15;
		}
		transform.Rotate (Vector3.up * needToRot);
		--adjustCounter;
	}

	public bool isIdle()
	{
		if (myAnimator.GetFloat ("VSpeed") == 0 && myAnimator.GetFloat ("HorizSpeed") == 0 && myAnimator.GetBool ("shouldFrontFlip") == false && myAnimator.GetBool ("Jumping") == false)
		{
			return true;
		}
		return false;
	}


	// Update is called once per frame
	void Update () {

		myAnimator.SetFloat ("VSpeed", Input.GetAxis ("Vertical"));

//		myAnimator.SetFloat("HorizSpeed", Input.GetAxis("Horizontal"));

//		if(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))
//		{
//			transform.Translate(Vector3.left * Time.deltaTime * 5);
//		}
//
//		else if(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow))
//		{
//			transform.Translate(Vector3.right * Time.deltaTime * 5);
//		}

		if ((Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) 
			&& !myAnimator.GetBool("Jumping") && !myAnimator.GetBool("shouldFrontFlip") && 
			player.GetComponent<DevCombat>().notInCombatMove()) {
				Debug.Log ("Running!");
				transform.Translate (Vector3.forward * Time.deltaTime * 5f);
		} else {
			stopFootstepSound ();
		}

		if (applyJumpTrans && player.GetComponent<DevCombat>().notInCombatMove()) {
			transform.Translate (Vector3.forward * Time.deltaTime * 8f);
		}

		if(Input.GetButtonDown("Jump") && myAnimator.GetFloat("VSpeed") > 0 && adjustCounter == 0 
			&& player.GetComponent<DevCombat>().notInCombatMove())
		{
			myAnimator.SetBool("Jumping", true);
			Invoke ("stopJumping", 0.8f);
		}
		else if(Input.GetButtonDown("FrontFlip") && myAnimator.GetFloat("VSpeed") > 0 && adjustCounter == 0
			&& player.GetComponent<DevCombat>().notInCombatMove())
		{
			myAnimator.SetBool ("shouldFrontFlip", true);
			Invoke ("stopFrontFlip", 2.1f);
		}
		if(adjustCounter == 0 && myAnimator.GetFloat("VSpeed") > 0) {
			if (!myAnimator.GetBool ("Jumping") && !myAnimator.GetBool ("shouldFrontFlip") && player.GetComponent<DevCombat>().notInCombatMove()) {
				transform.Rotate (Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * Camera.main.GetComponent<MouseMovement>().sensitivityX);
			}
		}
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
}