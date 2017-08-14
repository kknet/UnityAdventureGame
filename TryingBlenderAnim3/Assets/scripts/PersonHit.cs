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

	void OnTriggerEnter(Collider col){
		Debug.Log ("got hit");

		if (col.gameObject.CompareTag ("Weapons") && !strongHit.isPlaying) {
			myAnim.SetBool ("hitStrong", true);
			strongHit.Play ();
			decreaseHealth (100f);
			Invoke ("stopStrong", 1.0f);
		}
	}

	void stopStrong(){
		myAnim.SetBool ("hitStrong", false);
	}


	// Update is called once per frame
	void Update () {

	}

	void decreaseHealth(float decrease){
		GetComponent<ManageHealth> ().decreaseHealth (decrease);
	}

}
