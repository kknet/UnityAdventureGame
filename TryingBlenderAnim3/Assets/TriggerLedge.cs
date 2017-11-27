using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLedge : MonoBehaviour {
	public string debugMsg;
	public GameObject dev;
	public DevMovement devMovementScript;

	void Start(){
		dev = GameObject.Find ("DevDrake");
		devMovementScript = dev.GetComponent<DevMovement> ();
	}

	void OnTriggerStay(Collider other){
		if (other.gameObject.transform.root.gameObject.name.Equals("DevDrake")) {
			devMovementScript.setHanging ();
		}
	}
}
