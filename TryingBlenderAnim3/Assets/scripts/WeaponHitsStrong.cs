using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitsStrong : MonoBehaviour {

	private AudioSource strongHit;
	private GameObject wep;
	private CapsuleCollider wepColl;
	private GameObject Character;
	private Animator myAnimator;
	// Use this for initialization
	void Start () {
		//Model/hips/leftleg/spine/spine1/spine2/rightshoulder/rightarm/rightforearm/righthand/PrimaryWeapon
		Character = getAncestor (this.gameObject);
		wep = GameObject.Find (Character.name + "/" + Character.name + "Model/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand/ScimitarOut");
		wepColl = wep.GetComponent<CapsuleCollider> ();
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
		return GameObject.Find (Character.name + "/Audio Sources/" + audioName).GetComponent<AudioSource>();
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
