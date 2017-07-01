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
		weaponOut = "";
		initTable ();
		setOutInactive ();
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
		if (Input.GetKeyDown (KeyCode.C)) {
			if (weaponOut != "")
				StartSheath ();
		}
		if (Input.GetKeyDown(KeyCode.Alpha1)) {	
			if (weaponOut == "Scimitar"){
				return;
			}
			if (weaponOut != "") {
				StartSheath ();
			}
			weaponOut = "Scimitar";
			StartDraw ();
		}
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

	//assumes that previous weapon (if any) has been sheathed
	void StartDraw(){
		myAnimator.SetBool ("Drawing", true);
	}
	public void FinishDrawing(){
		weaponsTable[weaponOut + "Out"].SetActive(true);
		weaponsTable[weaponOut + "In"].SetActive(false);
		myAnimator.SetBool ("Drawing", false);
		myAnimator.SetBool(("WeaponDrawn"), true);
	}
		
	void setOutInactive()
	{
		foreach (GameObject g in allWeps) {
			if (g.name.Contains ("Out"))
				g.SetActive (false);
			else
				g.SetActive (true);
		}		
	}
}
