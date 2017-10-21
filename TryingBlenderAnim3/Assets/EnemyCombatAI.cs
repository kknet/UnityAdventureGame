using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatAI : MonoBehaviour {



	#region variables

	public bool setBlocking;

	private GameObject dev;
	private Animator enemyAnim;

	#endregion


	// Use this for initialization
	void Start () {
		enemyAnim = this.gameObject.GetComponent<Animator> ();
		dev = GameObject.Find ("DevDrake");
		enemyAnim.SetBool ("enemyBlock", setBlocking);
	}


	public void stopEnemyAttack(){
		enemyAnim.SetBool ("enemyAttack", false);
	}


	public bool notInCombatMove() {
		return !isAttacking() && !enemyAnim.GetBool ("enemyBlock");
	}

	public bool isAttacking() {
		AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo (0);
		return info.IsName ("QUICK1") || info.IsName ("QUICK2") || info.IsName ("QUICK3") || info.IsName("QUICK4") || info.IsName("QUICK5");
	}


	// Update is called once per frame
	void Update () {
		if (isAttacking ())
			return;
		
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			enemyAnim.SetBool ("enemyAttack", true);
			enemyAnim.SetInteger ("enemyQuick", 1);
		} else if (Input.GetKeyDown (KeyCode.Alpha2)) {
			enemyAnim.SetBool ("enemyAttack", true);
			enemyAnim.SetInteger ("enemyQuick", 2);
		} else if (Input.GetKeyDown (KeyCode.Alpha3)) {
			enemyAnim.SetBool ("enemyAttack", true);
			enemyAnim.SetInteger ("enemyQuick", 3);
		} else if (Input.GetKeyDown (KeyCode.Alpha4)) {
			enemyAnim.SetBool ("enemyAttack", true);
			enemyAnim.SetInteger ("enemyQuick", 4);
		} else if (Input.GetKeyDown (KeyCode.Alpha5)) {
			enemyAnim.SetBool ("enemyAttack", true);
			enemyAnim.SetInteger ("enemyQuick", 5);
		}
	}
}
