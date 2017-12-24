using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLedge : MonoBehaviour {
	public string debugMsg;
	public GameObject dev;
	public DevMovement devMovementScript;
	private GameObject offsetCorrector;

	void Start(){
		dev = GameObject.Find ("DevDrake");
		offsetCorrector = transform.Find ("Offset Corrector").gameObject;
		devMovementScript = dev.GetComponent<DevMovement> ();
	}

	void Update(){
//		if (devMovementScript.isHanging()) {
//			Vector3 goalPos = new Vector3 (offsetCorrector.transform.position.x, dev.transform.position.y, dev.transform.position.z); 
//			Debug.Log (goalPos + " " + dev.transform.position);
//			dev.transform.position = Vector3.Lerp (dev.transform.position, goalPos, 0.01f);
//
//		}
	}

	void OnTriggerStay(Collider other){
		if (other.gameObject.transform.root.gameObject.name.Equals("DevDrake")) {
			devMovementScript.setHanging ();
		}
	}
}
