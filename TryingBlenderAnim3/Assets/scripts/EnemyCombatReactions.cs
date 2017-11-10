using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCombatReactions : MonoBehaviour {

	public int health;
	public Image enemyHealthBar;

	private float maxHealth;
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
		maxHealth = health;
	}
	
	// Update is called once per frame
	void Update () {
		if (health <= 0) {
			enemyAnim.SetBool ("Dead", true);
		}
		updateHealthBar ();
//		bool collision = Physics.Raycast (transform.position + transform.up + (transform.forward * 0.3f), transform.forward, 0.5f);

	}

	private void updateHealthBar(){
		float ratio = health / maxHealth;
		if (ratio < 0f)
			ratio = 0f;
		enemyHealthBar.rectTransform.localScale = new Vector3 (Mathf.MoveTowards(enemyHealthBar.rectTransform.localScale.x, ratio, 0.005f), 1f, 1f);
	}

	private float Clamp(float f){
		if (f > 180f)
			return f - 360f;
		if (f < -180f)
			return f + 360f;
		return f;
	}

	public bool rotationAllowsBlock(){
		float myAngle = transform.eulerAngles.y; Clamp (myAngle);
		float devAngle = dev.transform.eulerAngles.y; Clamp (devAngle);
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
		bool rotationAllows = rotationAllowsBlock ();
		if (enemyAnim.GetCurrentAnimatorStateInfo (0).IsName ("sword_and_shield_block_idle") && rotationAllows) {
			yield return new WaitForSeconds (blockDelayTimes [animationIndex - 1]);
			enemyAnim.CrossFade ("standing_block_react_large", 0.05f);

			if (animationIndex != 2) {
				yield return new WaitForSeconds (0.3f);
				enemyAnim.CrossFade ("React from Right and Move Back", 0.1f);
			}
		} else if (rotationAllows) {
			yield return new WaitForSeconds (callDelayTimes [animationIndex - 1]);
			enemyAnim.CrossFade (reactAnimations [animationIndex - 1], crossFadeTimes [animationIndex - 1]);
			--health;
			if (animationIndex == 4)
				health -= 2;
		} else {
			yield return new WaitForSeconds (callDelayTimes [animationIndex - 1]);
			enemyAnim.CrossFade ("standing_react_large_gut", crossFadeTimes [animationIndex - 1]);
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
