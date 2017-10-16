using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatAI : MonoBehaviour {

	private GameObject dev;
	private Animator enemyAnim;
	private string[] reactAnimations = {
		"standing_react_large_from_right",
		"standing_react_large_from_left",
		"React from Right and Move Back"
	};

	private float[] crossFadeTimes = {
		0.05f,
		0.05f,
		0.05f
	};

	private float[] callDelayTimes = {
		0.1f,
		0.1f,
		0.1f
	};

	// Use this for initialization
	void Start () {
		enemyAnim = this.gameObject.GetComponent<Animator> ();
		dev = GameObject.Find ("DevDrake");
	}
	
	// Update is called once per frame
	void Update () {
//		bool collision = Physics.Raycast (transform.position + transform.up + (transform.forward * 0.3f), transform.forward, 0.5f);
	}
		
	public void playReactAnimation(int animationIndex){
		StartCoroutine (callAnimation(animationIndex));
	}

	private IEnumerator callAnimation(int animationIndex){
		yield return new WaitForSeconds (callDelayTimes [animationIndex - 1]);
		enemyAnim.CrossFade (reactAnimations[animationIndex-1], crossFadeTimes[animationIndex-1]);
	}

	/*	void OnDrawGizmos(){
			Gizmos.color = Color.red;
			Gizmos.DrawRay (transform.position + transform.up + (transform.forward * 0.3f), transform.forward * 0.5f);
		}
	*/
}
