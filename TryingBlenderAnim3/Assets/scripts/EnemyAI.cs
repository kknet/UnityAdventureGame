﻿using System.Collections;
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
//		if (GetComponent<ManageHealth> ().isDead ())
//			this.gameObject.SetActive (false);

//		Debug.Log ("oldDev: "+ oldDev + " dev: "+ Dev.transform.position + " enemy: " + transform.position + " dif: " + dif);

		applyRotation ();

		if (!Approx (dif, Vector3.zero)) {
			oldDev = devTarget;
			applySpeed ();
		} 
//			else {
//			reduceSpeed ();
//		}

		if (!isEnemyAttacking() && Vector3.Magnitude (Dev.transform.position - transform.position) < 2f) {
			attack ();
		}


	}

	public bool isEnemyAttacking() {
		AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo (0);
		return info.IsName ("QUICK1") || info.IsName ("QUICK2") || info.IsName ("QUICK3") || info.IsName ("QUICK4") || info.IsName ("QUICK5");
	}


	private void attack() {
		enemyAnim.SetBool ("enemyAttack", true);
		Invoke ("switchAttack", 0.5f);
	}

	public void stopEnemyAttack(){
		enemyAnim.SetBool ("enemyAttack", false);
	}


	private void switchAttack(){
		switch (enemyAnim.GetInteger ("enemyQuick")) {
		case 1:
			enemyAnim.SetInteger ("enemyQuick", 2);
			break;
		case 2:
			enemyAnim.SetInteger ("enemyQuick", 3);
			break;
		case 3:
			enemyAnim.SetInteger ("enemyQuick", 4);
			break;
		case 4:
			enemyAnim.SetInteger ("enemyQuick", 5);
			break;
		case 5:
			enemyAnim.SetInteger ("enemyQuick", 1);
			break;
		default:
			Debug.LogAssertion ("quickAttack is not set to 1-5, look at EnemyAI.cs script (switchAttack method)");
			break;
		}
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
			if ((oldDev == Vector3.zero) || (Vector3.Magnitude (oldDev - Dev.transform.position) > 0.7f && Vector3.Magnitude (Dev.transform.position - transform.position) > 0.7f)) {
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
			if ((Vector3.Magnitude (oldDev - Dev.transform.position) > 0.7f && Vector3.Magnitude (Dev.transform.position - transform.position) > 0.7f)) {
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
		if (isEnemyAttacking ()) {
			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 0f, 5f * Time.deltaTime));
			return;
		}
		if (Vector3.Magnitude (dif) < 2f) {
			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 0f, 5f * Time.deltaTime));
		} else {
			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 1f, 2f * Time.deltaTime));
		}
		transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
	}

//	private void reduceSpeed(){
//		enemyAnim.SetFloat ("enemySpeed", Mathf.Max (0f, enemyAnim.GetFloat ("enemySpeed") - 0.03f));
//	}


}