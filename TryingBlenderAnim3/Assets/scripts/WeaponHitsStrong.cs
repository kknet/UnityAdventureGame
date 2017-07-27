using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitsStrong : MonoBehaviour {

	private AudioSource strongHit;
	private GameObject wep;
	private MeshCollider wepColl;
	private GameObject Character;
	private Animator myAnimator;
	// Use this for initialization
	void Start () {
		wep = GameObject.Find ("ScimitarOut");
		wepColl = wep.GetComponent<MeshCollider> ();
		Character = getAncestor (this.gameObject);
		myAnimator = Character.GetComponent<Animator> ();
		strongHit = findSound ("Strong Hit");
	}
	
	// Update is called once per frame

	public bool isAttacking() {
		AnimatorStateInfo info = myAnimator.GetCurrentAnimatorStateInfo (0);
		return info.IsTag ("attacking");
	}

	public bool isBlocking() {
		AnimatorStateInfo info = myAnimator.GetCurrentAnimatorStateInfo (0);
		return info.IsTag ("blocking");
	}

	public bool isIdleRun() {
		AnimatorStateInfo info = myAnimator.GetCurrentAnimatorStateInfo (0);
		return info.IsTag ("idleRun");
	}
		
	AudioSource findSound(string audioName){
		return GameObject.Find (Character.name + "/AudioSources/" + audioName).GetComponent<AudioSource>();
	}

	GameObject getAncestor (GameObject currentObj) {
		return currentObj.transform.root.gameObject;
	}

	void OnCollisionEnter(Collision col){
		if (isAttacking ())
			return;

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
