using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDuringReactions : MonoBehaviour {

	private Animator myAnimator;

	// Use this for initialization
	void Start () {
		myAnimator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
//		if (myAnimator.GetCurrentAnimatorStateInfo (0).IsName ("React from Right and Move Back")) {
//			transform.Translate (-transform.forward /* * -myAnimator.GetFloat ("RunSpeed")*/ * Time.deltaTime * 1f);
//		}
	}
}
