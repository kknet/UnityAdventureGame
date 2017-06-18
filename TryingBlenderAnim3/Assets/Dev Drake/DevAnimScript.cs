using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevAnimScript : MonoBehaviour {
	public Transform CamTransform;

	private Animator myAnimator;
	private float needToRot;

	// Use this for initialization
	void Start () {
		myAnimator = GetComponent<Animator>();
		needToRot = 0;
	}

	public void adjustToCam(float dif, bool needToAdjust)
	{
		if (Mathf.Approximately (dif, 0f)) {
			myAnimator.SetBool ("adjustingToCam", false);
			needToAdjust = false;
			return;
		}
		if(myAnimator.GetBool ("adjustingToCam")){
			needToRot = dif / 15.0f;
		}

		myAnimator.SetBool ("adjustingToCam", true);
		transform.Rotate (Vector3.up * needToRot);
		if (Mathf.Approximately(transform.rotation.eulerAngles.y, CamTransform.rotation.eulerAngles.y)) {
			myAnimator.SetBool ("adjustingToCam", false);
			needToAdjust = false;
		}
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

//		if(myAnimator.GetBool("adjustingToCam"))
//			return;

		//Debug.Log("HSpeed: " + myAnimator.GetFloat("HSpeed"));

		myAnimator.SetFloat("VSpeed", Input.GetAxis("Vertical"));
		myAnimator.SetFloat("HorizSpeed", Input.GetAxis("Horizontal"));

		//		if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && transform.rotation.eulerAngles.y < CamTransform.rotation.eulerAngles.y)
		//			transform.Rotate(Vector3.up * Time.deltaTime * 40.0f);
		//		else if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && transform.rotation.eulerAngles.y > CamTransform.rotation.eulerAngles.y)
		//			transform.Rotate(Vector3.down * Time.deltaTime * 40.0f);

		//		if (Mathf.Abs (myAnimator.GetFloat ("VSpeed")) > 0.0f && transform.rotation.eulerAngles.y != CamTransform.rotation.eulerAngles.y)
		//			transform.rotation.Set(CamTransform.rotation.x, CamTransform.rotation.y, CamTransform.rotation.z, CamTransform.rotation.w);
		//		


		if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && Input.GetAxis("Mouse X") > 0.2)
		{
			transform.Rotate(Vector3.up * Time.deltaTime * 100.0f);
		}
		else if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && Input.GetAxis("Mouse X") < -0.2)
		{
			transform.Rotate(Vector3.down * Time.deltaTime * 100.0f);
		}

		if(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))
		{
			transform.Translate(Vector3.left * Time.deltaTime * 5);
		}

		if(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow))
		{
			transform.Translate(Vector3.right * Time.deltaTime * 5);
		}

		if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow))
		{
			transform.Translate(Vector3.forward * Time.deltaTime * 10);
		}

		if(Input.GetButtonDown("Jump"))
		{
			myAnimator.SetBool("Jumping", true);
			transform.Translate(Vector3.back * Time.deltaTime * 10);
			Invoke("stopJumping", 0.1f);
		}

		if(Input.GetButtonDown("FrontFlip"))
		{
			myAnimator.SetBool ("shouldFrontFlip", true);
			//transform.Translate(Vector3.back * Time.deltaTime * 10);
			Invoke ("stopFrontFlip", 0.1f);
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