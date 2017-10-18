using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatAI : MonoBehaviour {

	public bool setBlocking;

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
		0.25f,
		0.3f,
		0.6f
	};

	private float[] blockDelayTimes = {
		0.05f,
		0.01f,
		0.01f
	};

	// Use this for initialization
	void Start () {
		enemyAnim = this.gameObject.GetComponent<Animator> ();
		dev = GameObject.Find ("DevDrake");
		enemyAnim.SetBool ("enemyBlock", setBlocking);
	}
	
	// Update is called once per frame
	void Update () {
//		bool collision = Physics.Raycast (transform.position + transform.up + (transform.forward * 0.3f), transform.forward, 0.5f);

	}
		
	public void playReactAnimation(int animationIndex){
		StartCoroutine (callAnimation(animationIndex));
	}

	private IEnumerator callAnimation(int animationIndex){
		if (enemyAnim.GetCurrentAnimatorStateInfo (0).IsName ("sword_and_shield_block_idle")) {
			yield return new WaitForSeconds (blockDelayTimes [animationIndex - 1]);
			enemyAnim.CrossFade ("standing_block_react_large", 0.05f);
		}
		else {
			yield return new WaitForSeconds (callDelayTimes [animationIndex - 1]);
			enemyAnim.CrossFade (reactAnimations [animationIndex - 1], crossFadeTimes [animationIndex - 1]);
		}
	}

	public bool isBlocking(){
		AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo (0);
		return info.IsName ("sword_and_shield_block_idle") || info.IsName("standing_block_react_large");
	}

	/*	void OnDrawGizmos(){
			Gizmos.color = Color.red;
			Gizmos.DrawRay (transform.position + transform.up + (transform.forward * 0.3f), transform.forward * 0.5f);
		}
	*/
}
