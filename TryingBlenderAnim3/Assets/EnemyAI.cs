using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

	private Animator enemyAnim;
	private GameObject Dev;
	private int rotCounter;
	private float needToRot;
	private bool firstTimeAdjust;
	private float rotSpeed;

	// Use this for initialization
	void Start () {
		enemyAnim = GetComponent<Animator> ();
		Dev = GameObject.Find ("DevDrake");
		rotCounter = 0;
		needToRot = 0f;
		firstTimeAdjust = false;
		rotSpeed = 5f;
	}
	
	// Update is called once per frame
	void Update () {
		applyRotation ();

//		if (!Approx (dif, Vector3.zero)) {
//			applySpeed ();
//		} else {
//			reduceSpeed ();
//		}


	}

	private bool Approx(Vector3 a, Vector3 b){
		return Mathf.Approximately (a.x, b.x) && Mathf.Approximately (a.y, b.y) && Mathf.Approximately (a.z, b.z);
	}

	private void applyRotation() {

		Vector3 dif = Dev.transform.position - transform.position;
		dif = new Vector3 (dif.x, 0f, dif.z);

		transform.forward = 
		Vector3.RotateTowards (transform.forward, dif, rotSpeed * Time.deltaTime, 0.0f); 
//		transform.Rotate (needToRot * Vector3.up);
//		--rotCounter;
	}

	private void applySpeed(){
		enemyAnim.SetFloat ("enemySpeed", Mathf.Min (1.0f, enemyAnim.GetFloat ("enemySpeed") + 0.03f));
	}

	private void reduceSpeed(){
		enemyAnim.SetFloat ("enemySpeed", Mathf.Max (0f, enemyAnim.GetFloat ("enemySpeed") - 0.03f));
	}


}
