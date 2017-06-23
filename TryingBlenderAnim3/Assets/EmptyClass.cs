//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//
//public class DevAnimScript : MonoBehaviour {
//	public Transform CamTransform;
//
//	public Animator myAnimator;
//	private float needToRot;
//
//	// Use this for initialization
//	void Start () {
//		myAnimator = GetComponent<Animator>();
//		needToRot = 0;
//		myAnimator.SetBool ("adjustingToCam", false);
//	}
//
//	public void adjustToCam(float dif, bool needToAdjust)
//	{
//		if (Mathf.Approximately (dif, 0f)) {
//			myAnimator.SetBool ("adjustingToCam", false);
//			needToAdjust = false;
//			return;
//		}
//		if(myAnimator.GetBool ("adjustingToCam")){
//			if (dif > 180f)
//				dif = dif - 360f;
//			else if (dif < -180f)
//				dif = dif + 360f;
//			needToRot = dif / 15.0f;
//		}
//		myAnimator.SetBool ("adjustingToCam", true);
//		transform.Rotate (Vector3.up * needToRot);
//		//		if (Mathf.Approximately(transform.rotation.eulerAngles.y, CamTransform.rotation.eulerAngles.y)) {
//		//			myAnimator.SetBool ("adjustingToCam", false);
//		//			needToAdjust = false;
//		//		}
//	}
//
//	public bool isIdle()
//	{
//		if (myAnimator.GetFloat ("VSpeed") == 0 && myAnimator.GetFloat ("HorizSpeed") == 0 && myAnimator.GetBool ("shouldFrontFlip") == false && myAnimator.GetBool ("Jumping") == false)
//		{
//			return true;
//		}
//		return false;
//	}
//
//	// Update is called once per frame
//	void Update () {
//
//		myAnimator.SetFloat("VSpeed", Input.GetAxis("Vertical"));
//
//		//		myAnimator.SetFloat("HorizSpeed", Input.GetAxis("Horizontal"));
//
//		//		if(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))
//		//		{
//		//			transform.Translate(Vector3.left * Time.deltaTime * 5);
//		//		}
//		//
//		//		else if(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow))
//		//		{
//		//			transform.Translate(Vector3.right * Time.deltaTime * 5);
//		//		}
//
//		if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow))
//		{
//			transform.Translate(Vector3.forward * Time.deltaTime * 7);
//		}
//		if(Input.GetButtonDown("Jump"))
//		{
//			myAnimator.SetBool("Jumping", true);
//			Invoke("stopJumping", 0.5f);
//		}
//		else if(Input.GetButtonDown("FrontFlip"))
//		{
//			myAnimator.SetBool ("shouldFrontFlip", true);
//			Invoke ("stopFrontFlip", 0.5f);
//		}
//		if(!myAnimator.GetBool("adjustingToCam")) {
//			if (!myAnimator.GetBool ("Jumping") && !myAnimator.GetBool ("shouldFrontFlip")) {
//				if (Mathf.Abs (myAnimator.GetFloat ("VSpeed")) > 0.0f && Input.GetAxis ("Mouse X") > 0) {
//					transform.Rotate (Vector3.up * Time.deltaTime * 100.0f);
//				} else if (Mathf.Abs (myAnimator.GetFloat ("VSpeed")) > 0.0f && Input.GetAxis ("Mouse X") < 0) {
//					transform.Rotate (Vector3.down * Time.deltaTime * 100.0f);
//				}
//			}
//		}
//	}
//
//	void stopJumping()
//	{
//		myAnimator.SetBool("Jumping", false);
//	}
//
//	void stopFrontFlip()
//	{
//		myAnimator.SetBool ("shouldFrontFlip", false);
//	}
//}