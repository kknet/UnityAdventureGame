using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLedge : MonoBehaviour {
	public string debugMsg;
	public GameObject dev;
	public DevMovement devMovementScript;
	private GameObject offsetCorrector;
//	private float duration = 52;

	void Start(){
		dev = GameObject.Find ("DevDrake");
		offsetCorrector = transform.Find ("Offset Corrector").gameObject;
		devMovementScript = dev.GetComponent<DevMovement> ();
	}

	void Update(){
		if (devMovementScript.hangDrop) {
			StartCoroutine (doHangDrop());
		}
	}

	IEnumerator doHangDrop(){
		yield return null;
		Vector3 goalPos = new Vector3 (offsetCorrector.transform.position.x, offsetCorrector.transform.position.y, dev.transform.position.z); 
		Debug.Log (goalPos + " " + dev.transform.position);
		dev.transform.position = Vector3.Lerp (dev.transform.position, goalPos, 0.05f);
	}

	void OnTriggerStay(Collider other){
		if (other.gameObject.transform.root.gameObject.name.Equals("DevDrake")) {
			devMovementScript.setHanging ();
		}
	}
}
