using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

	private Animator enemyAnim;
	private GameObject Dev;
	private int rotCounter;
	private Vector3 needToRot;

	// Use this for initialization
	void Start () {
		enemyAnim = GetComponent<Animator> ();
		Dev = GameObject.Find ("DevDrake");
		rotCounter = 0;
		needToRot = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 orig = transform.position;
		Vector3 devOrig = Dev.transform.position;

		Vector3 dif = devOrig - orig;
		dif = new Vector3 (dif.x, 0f, dif.z);//we don't care about the y difference

		Vector3 dir = Vector3.Normalize (dif);
		Vector3 target = devOrig - (5.0f * dir);

		//if not facing the correct direction
		if (!Approx (transform.forward, dir)) {
			applyRotation (dif);
		}
		if (!Approx (dif, Vector3.zero)) {
			applySpeed ();
		} else {
			reduceSpeed ();
		}


	}

	private bool Approx(Vector3 a, Vector3 b){
		return Mathf.Approximately (a.x, b.x) && Mathf.Approximately (a.y, b.y) && Mathf.Approximately (a.z, b.z);
	}

	private void applyRotation(Vector3 dif){
		if (rotCounter == 0 && !Approx (dif, Vector3.zero)) {
			rotCounter = 20;
			needToRot = dif / 20f;
			transform.Rotate (needToRot);
			--rotCounter;
		} else if (rotCounter > 0) {
			transform.Rotate (needToRot);
			--rotCounter;
		} else {
			rotCounter = 0;
			needToRot = Vector3.zero;
		}
	}

	private void applySpeed(){
		enemyAnim.SetFloat ("enemySpeed", Mathf.Min (1.0f, enemyAnim.GetFloat ("enemySpeed") + 0.03f));
	}

	private void reduceSpeed(){
		enemyAnim.SetFloat ("enemySpeed", Mathf.Max (0f, enemyAnim.GetFloat ("enemySpeed") - 0.03f));
	}


}
