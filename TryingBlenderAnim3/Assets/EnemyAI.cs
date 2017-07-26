using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

	private Animator enemyAnim;
	private GameObject Dev;
	private float rotSpeed;
	private float moveSpeed;
	private Vector3 dif;
	private Vector3 oldDev;
	private Vector3 target;
	private Vector3 devTarget;

	// Use this for initialization
	void Start () {
		enemyAnim = GetComponent<Animator> ();
		Dev = GameObject.Find ("DevDrake");
		rotSpeed = 5f;
		moveSpeed = 3f;
		oldDev = Vector3.zero;
		target = Vector3.zero;
	}

	// Update is called once per frame
	void Update () {
		Debug.Log ("oldDev: "+ oldDev + " dev: "+ Dev.transform.position + " enemy: " + transform.position + " dif: " + dif);

		applyRotation ();

		if (!Approx (dif, Vector3.zero)) {
			oldDev = devTarget;
			applySpeed ();
		} 
//			else {
//			reduceSpeed ();
//		}


	}
	private float rand(float a, float b){
		return UnityEngine.Random.Range (a, b);
	}

	private bool Approx(Vector3 a, Vector3 b){
		return Mathf.Approximately (a.x, b.x) && Mathf.Approximately (a.y, b.y) && Mathf.Approximately (a.z, b.z);
	}

	private void applyRotation() {
		//if not moving
		if (Mathf.Approximately (enemyAnim.GetFloat ("enemySpeed"), 0f)) {
			if ((oldDev == Vector3.zero) || (Vector3.Magnitude (oldDev - Dev.transform.position) > 2f && Vector3.Magnitude (Dev.transform.position - transform.position) > 2f)) {
				dif = Dev.transform.position - transform.position;
				dif = new Vector3 (dif.x, 0f, dif.z);
				Vector3 perpenDif = Vector3.Normalize (Vector3.Cross (dif, -1.0f * dif)) * rand (1f, -1f);
				devTarget = Dev.transform.position;
				target = Dev.transform.position + perpenDif;
				dif = target - transform.position;
				dif = new Vector3 (dif.x, 0f, dif.z);
			}		
		//if moving
		} else {
			if ((Vector3.Magnitude (oldDev - Dev.transform.position) > 2f && Vector3.Magnitude (Dev.transform.position - transform.position) > 2f)) {
				dif = Dev.transform.position - transform.position;
				dif = new Vector3 (dif.x, 0f, dif.z);
				Vector3 perpenDif = Vector3.Normalize (Vector3.Cross (dif, -1.0f * dif)) * rand (1f, -1f);
				devTarget = Dev.transform.position;
				target = Dev.transform.position + perpenDif;
				dif = target - transform.position;
				dif = new Vector3 (dif.x, 0f, dif.z);
			} else {
				dif = target - transform.position;
				dif = new Vector3 (dif.x, 0f, dif.z);
			}
		}

		transform.forward = Vector3.RotateTowards (transform.forward, dif, rotSpeed * Time.deltaTime, 0.0f); 
	}

	private void applySpeed() {
		if (Vector3.Magnitude (dif) < 2f) {
			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 0f, 2f * Time.deltaTime));
		} else {
			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 1f, 2f * Time.deltaTime));
		}
		transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
	}

//	private void reduceSpeed(){
//		enemyAnim.SetFloat ("enemySpeed", Mathf.Max (0f, enemyAnim.GetFloat ("enemySpeed") - 0.03f));
//	}


}