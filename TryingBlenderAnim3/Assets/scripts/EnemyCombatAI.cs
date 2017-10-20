using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatAI : MonoBehaviour {

	public bool setBlocking;
	public int health;

	private GameObject dev;
	private Animator enemyAnim;
	private string[] reactAnimations = {
		"standing_react_large_from_right",
		"standing_react_large_from_left",
		"React from Right and Move Back",
		"React from Right and Move Back"
	};

	private float[] crossFadeTimes = {
		0.05f,
		0.05f,
		0.05f,
		0.05f
	};

	private float[] callDelayTimes = {
		0.25f,
		0.3f,
		0.6f,
		0.6f
	};

	private float[] blockDelayTimes = {
		0.07f,
		0.01f,
		0.15f,
		0.4f
	};

	// Use this for initialization
	void Start () {
		enemyAnim = this.gameObject.GetComponent<Animator> ();
		dev = GameObject.Find ("DevDrake");
		enemyAnim.SetBool ("enemyBlock", setBlocking);
	}
	
	// Update is called once per frame
	void Update () {
		if (health <= 0) {
			enemyAnim.SetBool ("Dead", true);
		}
//		bool collision = Physics.Raycast (transform.position + transform.up + (transform.forward * 0.3f), transform.forward, 0.5f);

	}
		
	public void playReactAnimation(int animationIndex){
		if(health > 0)
			StartCoroutine (callAnimation(animationIndex));
	}

	private IEnumerator callAnimation(int animationIndex){
		if (enemyAnim.GetCurrentAnimatorStateInfo (0).IsName ("sword_and_shield_block_idle")) {
			yield return new WaitForSeconds (blockDelayTimes [animationIndex - 1]);
			enemyAnim.CrossFade ("standing_block_react_large", 0.05f);

			if (animationIndex != 2) {
				yield return new WaitForSeconds (0.3f);
				enemyAnim.CrossFade ("React from Right and Move Back", 0.1f);
			}
		}
		else {
			yield return new WaitForSeconds (callDelayTimes [animationIndex - 1]);
			enemyAnim.CrossFade (reactAnimations [animationIndex - 1], crossFadeTimes [animationIndex - 1]);
			--health;
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
