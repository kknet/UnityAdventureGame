using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponToggle : MonoBehaviour {

	GameObject scimitar;
	GameObject[] allWeps;

	// Use this for initialization
	void Start () {
		allWeps = GameObject.FindGameObjectsWithTag ("Weapons");
		scimitar = GameObject.Find ("Scimitar");
	}
	
	// Update is called once per frame
	void Update () {	
		if (Input.GetKeyDown(KeyCode.Alpha1)) {	
			setAllInactive ();
			scimitar.SetActive (true);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha0)) {
			setAllInactive ();
		}

	}

	void setAllInactive()
	{
//		allWeps = GameObject.FindGameObjectsWithTag ("Weapons");
		foreach (GameObject g in allWeps) {
			g.SetActive (false);
		}		
	}
}
