using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponToggle : MonoBehaviour {

	GameObject[] allWeps;
	Dictionary <string, GameObject> weaponsTable;
	public string weaponOut;
	public Animator myAnimator;

	// Use this for initialization
	void Start () {
		weaponsTable = new Dictionary<string, GameObject> ();
		allWeps = GameObject.FindGameObjectsWithTag ("Weapons");
		myAnimator = GameObject.Find ("DevDrake").GetComponent<Animator> ();
		weaponOut = "Scimitar";
		initTable ();
	}

	void initTable(){
		foreach (GameObject g in allWeps) {
			weaponsTable.Add (g.name, g);
		}
		if(weaponOut!="")
			weaponsTable[weaponOut + "In"].SetActive(false);
	}


	// Update is called once per frame
	void Update () {	
		if (Input.GetKeyDown(KeyCode.Alpha0)) {
			StartSheath ();
		}
//		else if (Input.GetKeyDown(KeyCode.Alpha1)) {	
//			if (weaponOut == "Scimitar")
//				return;
//			weaponOut = "Scimitar";
//		}
	}



	void StartSheath() {
		if(myAnimator.GetBool("WeaponDrawn"))
			myAnimator.SetBool ("Sheathing", true);
	}
	public void FinishSheath(){
		weaponsTable[weaponOut + "Out"].SetActive(false);
		weaponsTable[weaponOut + "In"].SetActive(true);
		weaponOut = "";
		myAnimator.SetBool ("Sheathing", false);
		myAnimator.SetBool(("WeaponDrawn"), false);
	}

//	void StartDraw(){
//		GameObject.Find (weaponOut + "Out").SetActive(true);
//		myAnimator.SetBool ("Sheathing", true);
//	}
	
//	void setAllInactive()
//	{
////		allWeps = GameObject.FindGameObjectsWithTag ("Weapons");
//		foreach (GameObject g in allWeps) {
//			g.SetActive (false);
//		}		
//	}
}
