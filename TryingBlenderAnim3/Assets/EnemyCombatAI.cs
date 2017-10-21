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
	
	// Update is called once per frame
	void Update () {
		
	}
}
