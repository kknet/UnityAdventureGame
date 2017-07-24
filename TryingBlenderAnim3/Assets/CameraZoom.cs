//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//
//public class CameraZoom : MonoBehaviour {
//
//	public GameObject Dev;
//	private Vector3 target;
//
//	// Use this for initialization
//	void Start () {
//	}
//	
//	void Update () {
//		//make a ray from the camera to the character
//		target = Dev.transform.position;
//		Vector3 orig = transform.position;
//		Vector3 dir = target - transform.position;
//		Ray toDev = new Ray (orig, dir);
//
//		//use the ray to see if there are any objects between the camera and player
//		RaycastHit hitInfo;
//		float maxDist = dir.magnitude;
//		bool hit = Physics.Raycast (toDev, out hitInfo, maxDist);
//		if (hit && hitInfo.collider.gameObject.name == "Cube") {
////			Debug.LogError ("Hit Cube!");
////			Debug.Log (dir);
//			GetComponent<MouseMovement>().ZoomIn();	
//		}
////		else {
////			GetComponent<MouseMovement>().ZoomOut(dir);	
////		}
//			
//	}
//}
