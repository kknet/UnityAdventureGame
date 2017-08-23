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
		AnimatorStateInfo anim = myAnim.GetCurrentAnimatorStateInfo (0);

		bool isThisAnEnemy = gameObject.CompareTag ("Enemy");
		bool gotHitByOther = false;
		if (isThisAnEnemy)
			gotHitByOther = col.gameObject.CompareTag ("OurWeapons");
		else
			gotHitByOther = col.gameObject.CompareTag ("EnemyWeapons");

		bool notHitAlready = !strongHit.isPlaying && !anim.IsTag("impact");

		if (gotHitByOther && notHitAlready) {
			myAnim.SetBool ("hitStrong", true);
			Debug.Log ("got hit");
			strongHit.Play ();
			decreaseHealth (100f);
			Invoke ("stopStrong", 0.3f);
//			col.gameObject.transform.root.gameObject.GetComponent<PersonHit> ().pauseAnim ();
//			while (!anim.IsTag ("impact")) {
//			}
//			col.gameObject.transform.root.gameObject.GetComponent<PersonHit> ().playAnim ();
		}
	}

	void stopStrong(){
		myAnim.SetBool ("hitStrong", false);
	}

//	public void pauseAnim(){
//		AnimatorStateInfo anim = myAnim.GetCurrentAnimatorStateInfo (0);
//		myAnim.
////		myAnim.enabled = false;
//	}
//
//	public void playAnim(){
//		myAnim.speed = 1;
//		AnimatorStateInfo anim = myAnim.GetCurrentAnimatorStateInfo (0);
//		anim.speed = 1;
////		myAnim.enabled = true;
//	}


	// Update is called once per frame
	void Update () {

	}

	void decreaseHealth(float decrease){
		GetComponent<ManageHealth> ().decreaseHealth (decrease);
	}

}
