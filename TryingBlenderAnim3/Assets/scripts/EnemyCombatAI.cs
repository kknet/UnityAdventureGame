using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatAI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		bool collision = Physics.Raycast (transform.position + transform.up + (transform.forward * 0.3f), transform.forward, 0.5f);
	}

//	void OnDrawGizmos(){
//		Gizmos.color = Color.red;
//		Gizmos.DrawRay (transform.position + transform.up + (transform.forward * 0.3f), transform.forward * 0.5f);
//	}
}
