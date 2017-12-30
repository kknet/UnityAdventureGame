using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevCombatReactions : MonoBehaviour {
	public int health;
	public Image healthBar;

	private GameObject dev;
	private Animator myAnimator;
	private float maxHealth;
	private Color green, yellow, red;

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
	public void Init () {
		myAnimator = this.gameObject.GetComponent<Animator> ();
		dev = GameObject.Find ("DevDrake");
		maxHealth = health;
	}

	// Update is called once per frame
	public void FrameUpdate () {
		if (health <= 0) {
			myAnimator.SetBool ("Dead", true);
		}
		updateHealthBar ();
		//		bool collision = Physics.Raycast (transform.position + transform.up + (transform.forward * 0.3f), transform.forward, 0.5f);

	}

	private void updateHealthBar(){
		float ratio = health / maxHealth;
		if (ratio < 0f)
			ratio = 0f;
		healthBar.rectTransform.localScale = new Vector3 (Mathf.MoveTowards(healthBar.rectTransform.localScale.x, ratio, 0.005f), 1f, 1f);

		Color blendColor;
		if (ratio < .5) { 
			blendColor = Color.Lerp (Color.red, Color.yellow, ratio*2f);
		} else {
			blendColor = Color.Lerp (Color.yellow, Color.green, (ratio-.5f)*2f);
		}
		healthBar.color = blendColor;
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

	private IEnumerator callConcurrentAnimation(int animationIndex){
		yield return new WaitForSeconds (callDelayTimes [animationIndex - 1]);
		myAnimator.CrossFade (reactAnimations [animationIndex - 1], crossFadeTimes [animationIndex - 1]);
	}

	private void stopBlockReact(){
		myAnimator.SetBool ("blockReact", false);
	}

	private void stopRegReact(){
		myAnimator.SetBool ("regReact", false);
	}
		
	private IEnumerator callAnimation(int animationIndex){
		if ((myAnimator.GetCurrentAnimatorStateInfo (0).IsName ("sword_and_shield_block_idle") || myAnimator.GetCurrentAnimatorStateInfo (1).IsName ("sword_and_shield_block_idle")) && rotationAllowsBlock()) {
			myAnimator.SetBool ("blockReact", true);
			StartCoroutine (callConcurrentAnimation (animationIndex));
			Invoke ("stopBlockReact", 1.5f);
//			yield return new WaitForSeconds (blockDelayTimes [animationIndex - 1]);
//			myAnimator.CrossFade ("sword_and_shield_impact_1", 0.05f);

//			if (animationIndex != 2) {
//				yield return new WaitForSeconds (0.3f);
//				myAnimator.CrossFade ("React from Right and Move Back", 0.1f);
//			}
		}
		else {
			myAnimator.SetBool ("regReact", true);
			Invoke ("stopRegReact", 1.5f);
			yield return new WaitForSeconds (callDelayTimes [animationIndex - 1]);
			myAnimator.CrossFade (reactAnimations [animationIndex - 1], crossFadeTimes [animationIndex - 1]);
			--health;
		}
	}

	public bool isBlocking(){
		AnimatorStateInfo info = myAnimator.GetCurrentAnimatorStateInfo (0);
		AnimatorStateInfo info2 = myAnimator.GetCurrentAnimatorStateInfo (1);
		return info.IsName ("sword_and_shield_block_idle") || info.IsName("sword_and_shield_impact_1")
			||  info2.IsName ("sword_and_shield_block_idle") || info2.IsName("sword_and_shield_impact_1");
	}

}
