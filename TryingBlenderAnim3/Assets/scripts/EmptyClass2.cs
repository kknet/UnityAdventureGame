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
//	private Vector3 dif;
//	// Use this for initialization
//	void Start () {
//		enemyAnim = GetComponent<Animator> ();
//		Dev = GameObject.Find ("DevDrake");
//		rotSpeed = 5f;
//		moveSpeed = 3f;
//	}
//
//	// Update is called once per frame
//	void Update () {
//		Debug.Log (Dev.transform.position + " " + transform.position + " " + dif);
//
//		applyRotation ();
//
//		if (!Approx (dif, Vector3.zero)) {
//			applySpeed ();
//		} 
//		//			else {
//		//			reduceSpeed ();
//		//		}
//
//
//	}
//
//	private bool Approx(Vector3 a, Vector3 b){
//		return Mathf.Approximately (a.x, b.x) && Mathf.Approximately (a.y, b.y) && Mathf.Approximately (a.z, b.z);
//	}
//
//	private void applyRotation() {
//		dif = Dev.transform.position - transform.position;
//		dif = new Vector3 (dif.x, 0f, dif.z);
//
//		transform.forward = Vector3.RotateTowards (transform.forward, dif, rotSpeed * Time.deltaTime, 0.0f); 
//	}
//
//	private void applySpeed(){
//		if (Vector3.Magnitude (dif) < 2f) {
//			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 0f, 2f * Time.deltaTime));
//		} else {
//			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 1f, 2f * Time.deltaTime));
//		}
//		transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
//	}
//
//	//	private void reduceSpeed(){
//	//		enemyAnim.SetFloat ("enemySpeed", Mathf.Max (0f, enemyAnim.GetFloat ("enemySpeed") - 0.03f));
//	//	}
//
//
//}