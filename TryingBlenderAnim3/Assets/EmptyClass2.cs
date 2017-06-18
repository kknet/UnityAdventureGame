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
//	private Animator myAnimator;
//
//	// Use this for initialization
//	void Start () {
//		myAnimator = GetComponent<Animator>();
//	}
//
//	public string ToString()
//	{
//		if (myAnimator.GetFloat ("VSpeed") == 0 && myAnimator.GetFloat ("HorizSpeed") == 0 && myAnimator.GetBool ("shouldFrontFlip") == false && myAnimator.GetBool ("Jumping") == false);
//		{
//			return "Idle";
//		}
//		return "NotIdle";
//	}
//
//	// Update is called once per frame
//	void Update () {
//		//Debug.Log("HSpeed: " + myAnimator.GetFloat("HSpeed"));
//
//		myAnimator.SetFloat("VSpeed", Input.GetAxis("Vertical"));
//		myAnimator.SetFloat("HorizSpeed", Input.GetAxis("Horizontal"));
//
//		//		if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && transform.rotation.eulerAngles.y < CamTransform.rotation.eulerAngles.y)
//		//			transform.Rotate(Vector3.up * Time.deltaTime * 40.0f);
//		//		else if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && transform.rotation.eulerAngles.y > CamTransform.rotation.eulerAngles.y)
//		//			transform.Rotate(Vector3.down * Time.deltaTime * 40.0f);
//
//		//		if (Mathf.Abs (myAnimator.GetFloat ("VSpeed")) > 0.0f && transform.rotation.eulerAngles.y != CamTransform.rotation.eulerAngles.y)
//		//			transform.rotation.Set(CamTransform.rotation.x, CamTransform.rotation.y, CamTransform.rotation.z, CamTransform.rotation.w);
//		//		
//
//
//		if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && Input.GetAxis("Mouse X") > 0.2)
//		{
//			transform.Rotate(Vector3.up * Time.deltaTime * 100.0f);
//		}
//		else if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && Input.GetAxis("Mouse X") < -0.2)
//		{
//			transform.Rotate(Vector3.down * Time.deltaTime * 100.0f);
//		}
//
//		if(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))
//		{
//			transform.Translate(Vector3.left * Time.deltaTime * 5);
//		}
//
//		if(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow))
//		{
//			transform.Translate(Vector3.right * Time.deltaTime * 5);
//		}
//
//		if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow))
//		{
//			transform.Translate(Vector3.forward * Time.deltaTime * 10);
//		}
//
//		if(Input.GetButtonDown("Jump"))
//		{
//			myAnimator.SetBool("Jumping", true);
//			transform.Translate(Vector3.back * Time.deltaTime * 10);
//			Invoke("stopJumping", 0.1f);
//		}
//
//		if(Input.GetButtonDown("FrontFlip"))
//		{
//			myAnimator.SetBool ("shouldFrontFlip", true);
//			//transform.Translate(Vector3.back * Time.deltaTime * 10);
//			Invoke ("stopFrontFlip", 0.1f);
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


/// <summary>
/// ////////////////////////
/// </summary>

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
//	private Animator myAnimator;
//
//	// Use this for initialization
//	void Start () {
//		myAnimator = GetComponent<Animator>();
//	}
//
//
//	public string toString()
//	{
//		if (myAnimator.GetFloat ("VSpeed") == 0 && myAnimator.GetFloat ("HorizSpeed") == 0 && myAnimator.GetBool ("ShouldFrontFlip") == false && myAnimator.GetBool ("Jumping") == false);
//		{
//			return "Idle";
//		}
//		return "NotIdle";
//	}
//
//	// Update is called once per frame
//	void Update () {
//		//Debug.Log("HSpeed: " + myAnimator.GetFloat("HSpeed"));
//
//		myAnimator.SetFloat("VSpeed", Input.GetAxis("Vertical"));
//		myAnimator.SetFloat("HorizSpeed", Input.GetAxis("Horizontal"));
//
//		if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && transform.rotation.eulerAngles.y < CamTransform.rotation.eulerAngles.y)
//			transform.Rotate(Vector3.up * Time.deltaTime * 50);
//		else if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && transform.rotation.eulerAngles.y > CamTransform.rotation.eulerAngles.y)
//			transform.Rotate(Vector3.down * Time.deltaTime * 50);
//
//		//		if (Mathf.Abs (myAnimator.GetFloat ("VSpeed")) > 0.0f && transform.rotation.eulerAngles.y != CamTransform.rotation.eulerAngles.y)
//		//			transform.rotation.Set(CamTransform.rotation.x, CamTransform.rotation.y, CamTransform.rotation.z, CamTransform.rotation.w);
//		//		
//
//
//		//		if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && Input.GetAxis("Mouse X") > 0.2)
//		//		{
//		//			transform.Rotate(Vector3.up * Time.deltaTime * 400.0f);
//		//		}
//		//		else if(Mathf.Abs(myAnimator.GetFloat("VSpeed")) > 0.0f && Input.GetAxis("Mouse X") < -0.2)
//		//		{
//		//			transform.Rotate(Vector3.down * Time.deltaTime * 400.0f);
//		//		}
//
//		if(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))
//		{
//			transform.Translate(Vector3.left * Time.deltaTime * 5);
//		}
//
//		if(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow))
//		{
//			transform.Translate(Vector3.right * Time.deltaTime * 5);
//		}
//
//		if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow))
//		{
//			transform.Translate(Vector3.forward * Time.deltaTime * 10);
//		}
//
//		if(Input.GetButtonDown("Jump"))
//		{
//			myAnimator.SetBool("Jumping", true);
//			transform.Translate(Vector3.back * Time.deltaTime * 10);
//			Invoke("stopJumping", 0.1f);
//		}
//
//		if(Input.GetButtonDown("FrontFlip"))
//		{
//			myAnimator.SetBool ("shouldFrontFlip", true);
//			//transform.Translate(Vector3.back * Time.deltaTime * 10);
//			Invoke ("stopFrontFlip", 0.1f);
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