//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//
//public class EnemyAI : MonoBehaviour {
//
//	private Animator enemyAnim;
//	private GameObject Dev;
//	private float rotSpeed;
//	private float moveSpeed;
//	private int rotCounter;
//	private Vector3 target;
//	private Vector3 dif;
//	private Vector3 oldDif;
//	private Vector3 oldDev;
//
//	// Use this for initialization
//	void Start () {
//		enemyAnim = GetComponent<Animator> ();
//		Dev = GameObject.Find ("DevDrake");
//		rotSpeed = 5f;
//		moveSpeed = 3f;
//		rotCounter = 0;
//		oldDif = Vector3.zero;
//		oldDev = Vector3.zero;
//	}
//
//	// Update is called once per frame
//	void Update () {
//		Debug.Log (Dev.transform.position + " " + target);
//
//		applyRotation ();
//		applySpeed ();
//
//		//		else {
//		//			reduceSpeed (dif);
//		//		}
//	}
//
//	private float rand(float a, float b){
//		return UnityEngine.Random.Range (a, b);
//	}
//
//	private bool Approx(Vector3 a, Vector3 b){
//		return Mathf.Approximately (a.x, b.x) && Mathf.Approximately (a.y, b.y) && Mathf.Approximately (a.z, b.z);
//	}
//
//	private void setRandomizedTarget(){
//		//		oldDif = dif;
//		dif = (Dev.transform.position - transform.position);
//		Vector3 perpenDif = Vector3.Normalize (Vector3.Cross (dif, -1.0f * dif)) * rand (1f, -1f);
//		target = Dev.transform.position + perpenDif;
//		dif = target - transform.position;
//	}
//
//	private void applyRotation() {
//		if ((oldDev == Vector3.zero) || (Vector3.Magnitude (oldDev - Dev.transform.position) > 2f && Vector3.Magnitude(Dev.transform.position - transform.position) > 2f)) {
//			setRandomizedTarget();
//		}
//
//		transform.forward = Vector3.RotateTowards (transform.forward, dif, rotSpeed * Time.deltaTime, 3.14f); 
//	}
//
//	private void applySpeed(){
//		if (Vector3.Magnitude (dif) < 2f) {
//			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 0f, 2f * Time.deltaTime));
//		} else {
//			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 1f, 2f * Time.deltaTime));
//		}
//		transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
//		oldDev = Dev.transform.position;
//	}
//
//
//
//	//	private void reduceSpeed(Vector3 dif){
//	//		if (Vector3.Magnitude (dif) < 1f) {
//	//			
//	//			return;
//	//		}
//	//		enemyAnim.SetFloat ("enemySpeed", Mathf.Max (0f, enemyAnim.GetFloat ("enemySpeed") - 0.03f));
//	//		transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
//	//	}
//
//
//}
