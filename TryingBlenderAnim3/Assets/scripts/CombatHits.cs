using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatHits : MonoBehaviour {

	public AudioSource strongHit;


	private GameObject scimitar;
	private MeshCollider scimColl;

	private GameObject testCube;
	private BoxCollider cubeColl;

	private GameObject Dev;
	private Animator myAnimator;
	// Use this for initialization
	void Start () {
		scimitar = GameObject.Find ("ScimitarOut");
		scimColl = scimitar.GetComponent<MeshCollider> ();
		testCube = GameObject.Find ("Cube");
		cubeColl = testCube.GetComponent<BoxCollider> ();
		Dev = GameObject.Find ("DevDrake");
		myAnimator = Dev.GetComponent<Animator> ();
	}
	
	// Update is called once per frame

	void OnCollisionEnter(Collision col){
		if (!Dev.GetComponent<DevCombat> ().isAttacking ())
			return;
		
//		if (col.gameObject.name == "Cube")
//			Debug.LogAssertion ("Awesome!");
		if (col.gameObject.CompareTag ("Strongs") && !strongHit.isPlaying) {
			strongHit.Play ();
			myAnimator.SetBool ("hitStrong", true);
			Invoke ("stopStrong", 1.0f);
		}
	}

	void stopStrong(){
		myAnimator.SetBool ("hitStrong", false);
	}


	void Update () {
	}
}
