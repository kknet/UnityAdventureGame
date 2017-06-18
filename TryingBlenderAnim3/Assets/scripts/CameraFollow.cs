using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public GameObject player;
	public Vector3 lookOffset = new Vector3 (0, 1, 0);
	public float distance = 3;
	public float CameraSpeed = 10000;




	// Update is called once per frame
	void Update () {
//		Vector3 lookPosition = player.position + lookOffset;
//		this.transform.LookAt (lookPosition);
//
//		if (Vector3.Distance (this.transform.position, lookPosition) > distance) {
//			this.transform.Translate (0, 0, CameraSpeed * Time.deltaTime);
//		}
//
//		if(Input.GetAxis("Mouse X") > 0)
//		{
//			transform.Rotate(Vector3.up * Time.deltaTime * 1000.0f);
//		}
//		else if(Input.GetAxis("Mouse X") < 0)
//		{
//			transform.Rotate(Vector3.down * Time.deltaTime * 1000.0f);
//		}
		if (player.ToString()=="Idle") {
		}
	}
}
