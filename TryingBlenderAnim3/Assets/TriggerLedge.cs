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

	void OnTriggerEnter(Collider other){
		if (other.gameObject.transform.root.gameObject.name.Equals("DevDrake") && !devMovementScript.isHanging()) {
			devMovementScript.hangDropStage = 0;
			devMovementScript.isInHangDrop = true;
//			devMovementScript.setHanging ();
//			StartCoroutine(doHangStart());
		}
	}
}
