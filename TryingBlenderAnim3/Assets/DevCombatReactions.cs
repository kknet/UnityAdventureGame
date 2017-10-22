using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCombatReactions : MonoBehaviour {
	public int health;

	private GameObject dev;
	private Animator enemyAnim;


	private string[] reactAnimations = {
		"standing_react_large_from_right",
		"React from Right and Move Back",
		"standing_react_large_from_left",
		"React from Right and Move Back",
		"standing_react_large_from_left",
	};

	private float[] crossFadeTimes = {
		0.05f,
		0.05f,
		0.05f,
		0.05f,
		0.05f
	};

	private float[] callDelayTimes = {
		0.71f,
		0.73f,
		0.6f,
		0.7f,
		0.7f
	};

	private float[] blockDelayTimes = {
		0.71f,
		0.73f,
		0.6f,
		0.7f,
		0.7f
	};

	// Use this for initialization
	void Start () {
		enemyAnim = this.gameObject.GetComponent<Animator> ();
		dev = GameObject.Find ("DevDrake");
	}

	// Update is called once per frame
	void Update () {
		if (health <= 0) {
			enemyAnim.SetBool ("Dead", true);
		}
		//		bool collision = Physics.Raycast (transform.position + transform.up + (transform.forward * 0.3f), transform.forward, 0.5f);

	}

	private float Clamp(float f){
		if (f > 180f)
			return f - 360f;
		if (f < -180f)
			return f + 360f;
		return f;
	}

	public bool rotationAllowsBlock(){
		float myAngle = GetComponent<DevCombat>().getCurrentEnemy().transform.eulerAngles.y; Clamp (myAngle);
		float devAngle = transform.eulerAngles.y; Clamp (devAngle);
		float rotDifference;
		if (myAngle > devAngle)
			rotDifference = Mathf.Abs (myAngle - devAngle);
		else
			rotDifference = Mathf.Abs (devAngle - devAngle);


		return rotDifference > 110f && rotDifference < 250f;
	}


	public void playReactAnimation(int animationIndex){
		if(health > 0)
			StartCoroutine (callAnimation(animationIndex));
	}

	private IEnumerator callAnimation(int animationIndex){
		if (enemyAnim.GetCurrentAnimatorStateInfo (0).IsName ("sword_and_shield_block_idle") && rotationAllowsBlock()) {
			yield return new WaitForSeconds (blockDelayTimes [animationIndex - 1]);
			enemyAnim.CrossFade ("sword_and_shield_impact_1", 0.05f);

//			if (animationIndex != 2) {
//				yield return new WaitForSeconds (0.3f);
//				enemyAnim.CrossFade ("React from Right and Move Back", 0.1f);
//			}
		}
		else {
			yield return new WaitForSeconds (callDelayTimes [animationIndex - 1]);
			enemyAnim.CrossFade (reactAnimations [animationIndex - 1], crossFadeTimes [animationIndex - 1]);
			--health;
		}
	}

	public bool isBlocking(){
		AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo (0);
		return info.IsName ("sword_and_shield_block_idle") || info.IsName("sword_and_shield_impact_1");
	}

}
