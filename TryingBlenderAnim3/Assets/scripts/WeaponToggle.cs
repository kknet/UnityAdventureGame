using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponToggle : MonoBehaviour {

	public Animator myAnimator;
	public AudioSource Sheath;
	public AudioSource Unsheath;

	private string weaponOut;
	private string newWeaponOut;
	GameObject[] allWeps;
	Dictionary <string, GameObject> weaponsTable;
	GameObject ShieldIn;
	GameObject ShieldOut;
//	bool isShieldOut;

	// Use this for initialization
	void Start () {
		weaponsTable = new Dictionary<string, GameObject> ();
		allWeps = GameObject.FindGameObjectsWithTag ("Weapons");
		myAnimator = GameObject.Find ("DevDrake").GetComponent<Animator> ();
		ShieldIn = GameObject.Find ("ShieldIn");
		ShieldOut = GameObject.Find ("ShieldOut");
		ShieldIn.SetActive (true);
		ShieldOut.SetActive (false);
		weaponOut = "";
		initTable ();
		setOutInactive ();
//		isShieldOut = false;
	}

	void initTable(){
		foreach (GameObject g in allWeps) {
			weaponsTable.Add (g.name, g);
		}
//		if(weaponOut!="")
//			weaponsTable[weaponOut + "In"].SetActive(false);
	}


	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.C)) {
			if (weaponOut != "") {
				StartShieldSheath ();
				Invoke ("StartSheath", 1.0f);
			}
		}
		else if (Input.GetKeyDown(KeyCode.Alpha1)) {	
			if (weaponOut == "Scimitar"){
				return;
			}

			//a weapon is already equipped (not a scimitar)
			if (weaponOut!="") {
				newWeaponOut = "Scimitar";
				StartSwitch ();

			//no weapon is already equipped
			} else {
				StartShieldDraw ();
				weaponOut = "Scimitar";
				Invoke ("StartDraw", 0.5f);
			}
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2)) {	
			if (weaponOut == "Spear"){
				return;
			}

			//a weapon is already equipped (not a scimitar)
			if (weaponOut!="") {
				newWeaponOut = "Spear";
				StartSwitch ();

			//no weapon is already equipped
			} else {
				StartShieldDraw ();
				weaponOut = "Spear";
				Invoke ("StartDraw", 0.5f);
			}
		}
	}

	void StartSwitch(){
		myAnimator.SetBool ("SwitchingWeps", true);
		Sheath.PlayDelayed (0.2f);
		Unsheath.PlayDelayed (1f);
	}


	void StartShieldSheath(){
		if (weaponOut!="") {
			myAnimator.SetBool ("ShieldDraw", false);
			myAnimator.SetBool ("ShieldSheath", true);
		}
	}

	void FinishShieldSheath(){
		ShieldIn.SetActive (true);
		ShieldOut.SetActive (false);
//		isShieldOut = false;
	}

	void StartShieldDraw(){
		if (weaponOut=="") {
			myAnimator.SetBool ("ShieldSheath", false);
			myAnimator.SetBool ("ShieldDraw", true);
		}
	}

	void FinishShieldDraw(){
		ShieldIn.SetActive (false);
		ShieldOut.SetActive (true);
//		isShieldOut = true;
	}
		
	void StartSheath() {
		if (weaponOut!="") {
			myAnimator.SetBool ("Sheathing", true);
			Sheath.PlayDelayed (0.3f);
		}
	}
	public void FinishSheath(){
		weaponsTable[weaponOut + "Out"].SetActive(false);
		weaponsTable[weaponOut + "In"].SetActive(true);

//		ShieldIn.SetActive (true);
//		ShieldOut.SetActive (false);

		if (myAnimator.GetBool ("SwitchingWeps")) {
			weaponOut = newWeaponOut;
		} else {
			weaponOut = "";
		}
		newWeaponOut = "";
		myAnimator.SetBool ("Sheathing", false);
		myAnimator.SetBool(("WeaponDrawn"), false);
	}

	//assumes that previous weapon (if any) has been sheathed
	void StartDraw(){
		if (weaponOut == "") {
			Debug.LogAssertion ("In StartDraw but weaponOut is blank");
			return;
		}
		Unsheath.PlayDelayed (0.3f);
		myAnimator.SetBool ("Drawing", true);
	}

	public void FinishDrawing(){
		weaponsTable[weaponOut + "Out"].SetActive(true);
		weaponsTable[weaponOut + "In"].SetActive(false);
//		ShieldIn.SetActive (false);
//		ShieldOut.SetActive (true);
		myAnimator.SetBool ("Drawing", false);
		myAnimator.SetBool ("SwitchingWeps", false);
		myAnimator.SetBool("WeaponDrawn", true);
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
