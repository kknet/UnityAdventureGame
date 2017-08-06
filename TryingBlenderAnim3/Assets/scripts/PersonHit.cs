using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonHit : MonoBehaviour {

	private Animator myAnim;
	private AudioSource strongHit;

	// Use this for initialization
	void Start () {
		myAnim = GetComponent<Animator> ();
		strongHit = findSound("Strong Hit");
	}

	AudioSource findSound(string audioName){
		string charName = transform.name;
		return GameObject.Find (charName + "/Audio Sources/" + audioName).GetComponent<AudioSource>();
	}

	void onCollisionEnter (Collision col) {
		if (col.gameObject.CompareTag ("Weapons") && !strongHit.isPlaying) {
			strongHit.Play ();
			myAnim.SetBool ("hitStrong", true);
			Invoke ("stopStrong", 1.0f);
		}
	}

	void stopStrong(){
		myAnim.SetBool ("hitStrong", false);
	}


	// Update is called once per frame
	void Update () {

	}

}
