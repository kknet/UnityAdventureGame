using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatAI : MonoBehaviour {

	private GameObject dev;
	private Animator enemyAnim;

	// Use this for initialization
	void Start () {
		enemyAnim = this.gameObject.GetComponent<Animator> ();
		dev = GameObject.Find ("DevDrake");
	}
	
	// Update is called once per frame
	void Update () {
//		bool collision = Physics.Raycast (transform.position + transform.up + (transform.forward * 0.3f), transform.forward, 0.5f);
	}
		
	public void playReactAnimation(){
//		Debug.Log(Vector3.Distance(transform.position, dev.transform.position));
		enemyAnim.CrossFade ("standing_react_large_from_right", 0.3f);
	}

	/*	void OnDrawGizmos(){
			Gizmos.color = Color.red;
			Gizmos.DrawRay (transform.position + transform.up + (transform.forward * 0.3f), transform.forward * 0.5f);
		}
	*/
}
