using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevAnimScript : MonoBehaviour {
	public Transform CamTransform;

	public Animator myAnimator;
	private float needToRot;
	public int adjustCounter;

	// Use this for initialization
	void Start () {
		myAnimator = GetComponent<Animator>();
		needToRot = 0;
		adjustCounter = 0;
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

		myAnimator.SetFloat("VSpeed", Input.GetAxis("Vertical"));

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

		if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow))
		{
			transform.Translate(Vector3.forward * Time.deltaTime * 7);
		}
		if(Input.GetButtonDown("Jump") && myAnimator.GetFloat("VSpeed") > 0 && adjustCounter == 0)
		{
			myAnimator.SetBool("Jumping", true);
			Invoke("stopJumping", 0.7f);
		}
		else if(Input.GetButtonDown("FrontFlip") && myAnimator.GetFloat("VSpeed") > 0 && adjustCounter == 0)
		{
			myAnimator.SetBool ("shouldFrontFlip", true);
			Invoke ("stopFrontFlip", 0.7f);
		}
		if(adjustCounter == 0) {
			if (!myAnimator.GetBool ("Jumping") && !myAnimator.GetBool ("shouldFrontFlip")) {
				if (Mathf.Abs (myAnimator.GetFloat ("VSpeed")) > 0.0f && Input.GetAxis ("Mouse X") > 0) {
					transform.Rotate (Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * Camera.main.GetComponent<Orbit>().sensitivityX);
				} else if (Mathf.Abs (myAnimator.GetFloat ("VSpeed")) > 0.0f && Input.GetAxis ("Mouse X") < 0) {
					transform.Rotate (Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * Camera.main.GetComponent<Orbit>().sensitivityX);
				}
			}
		}
	}

	void stopJumping()
	{
		myAnimator.SetBool("Jumping", false);
	}

	void stopFrontFlip()
	{
		myAnimator.SetBool ("shouldFrontFlip", false);
	}
}