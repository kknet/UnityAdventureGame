using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLedge : MonoBehaviour {
	public string debugMsg;
	public GameObject dev;
	public DevMovement devMovementScript;
	private GameObject offsetCorrector;
	private bool canDoHangDrop;
//	private float duration = 52;

	void Start(){
		canDoHangDrop = true;
		dev = GameObject.Find ("DevDrake");
//		offsetCorrector = transform.Find ("Offset Corrector").gameObject;
		devMovementScript = dev.GetComponent<DevMovement> ();
	}

//	void Update(){
//		if (devMovementScript.hangDrop && canDoHangDrop) {
////		if (devMovementScript.isHanging() && canDoHangDrop) {
//			canDoHangDrop = false;
//			dev.GetComponent<Animator> ().SetBool ("adjustedForHang", false);
//			StartCoroutine (doHangDrop());
//		}
//		else if (!devMovementScript.hangDrop)
//			canDoHangDrop = true;
//	}

//	IEnumerator doHangDrop(){
//		int counter = 0;
//		Vector3 goalPos = new Vector3 (offsetCorrector.transform.position.x, offsetCorrector.transform.position.y, dev.transform.position.z); 
//		while(!Mathf.Approximately((dev.transform.position-goalPos).magnitude, 0f) && counter < 40)
//		{
//			yield return null;			
//			dev.transform.position = Vector3.Lerp (dev.transform.position, goalPos, 0.1f);
//			++counter;
//		}
//		devMovementScript.hangDrop = false;
//		dev.GetComponent<Animator> ().SetBool ("adjustedForHang", true);
//	}

//	IEnumerator doHangStart(){
//		dev.GetComponent<Animator> ().SetBool ("hanging", false);
//		int counter = 0;
//		Vector3 goalPos = new Vector3 (offsetCorrector.transform.position.x, dev.transform.position.y, dev.transform.position.z); 
//		while(!Mathf.Approximately((dev.transform.position-goalPos).magnitude, 0f) && counter < 100)
//		{
//			yield return null;			
//			dev.transform.position = Vector3.Lerp (dev.transform.position, goalPos, 0.01f);
//			++counter;
//		}
//		dev.GetComponent<Animator> ().SetBool ("hanging", true);
////		StartCoroutine (doHangDrop ());
//	}


//	IEnumerator doHangDrop(){
////		while (!devMovementScript.hangDrop) {
////			yield return null;
////		}
//
//		int counter = 0;
//		Vector3 goalPos = new Vector3 (dev.transform.position.x, offsetCorrector.transform.position.y, dev.transform.position.z); 
//		while(!Mathf.Approximately((dev.transform.position-goalPos).magnitude, 0f) && counter < 200)
//		{
//			dev.transform.position = Vector3.Lerp (dev.transform.position, goalPos, 0.02f);
//			++counter;
//			yield return null;			
//		}
//		devMovementScript.hangDrop = false;
////		dev.GetComponent<Animator> ().SetBool ("adjustedForHang", true);
//	}

	void OnTriggerEnter(Collider other){
		if (other.gameObject.transform.root.gameObject.name.Equals("DevDrake")) {
			devMovementScript.isInHangDrop = true;
			dev.GetComponent<Animator> ().CrossFade ("Drop To Freehang Start", 0.1f);
//			devMovementScript.setHanging ();
//			StartCoroutine(doHangStart());
		}
	}
}
