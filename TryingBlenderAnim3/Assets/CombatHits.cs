using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatHits : MonoBehaviour {

	private GameObject scimitar;
	private MeshCollider scimColl;

	private GameObject testCube;
	private BoxCollider cubeColl;

	private GameObject Dev;
	// Use this for initialization
	void Start () {
		scimitar = GameObject.Find ("ScimitarOut");
		scimColl = scimitar.GetComponent<MeshCollider> ();
		testCube = GameObject.Find ("Cube");
		cubeColl = testCube.GetComponent<BoxCollider> ();
		Dev = GameObject.Find ("DevDrake");
	}
	
	// Update is called once per frame

	void OnCollisionEnter(Collision col){
		if (col.gameObject.name == "Cube" && Dev.GetComponent<DevCombat>().isAttacking())
			Debug.LogAssertion ("Awesome!");
	}

	void Update () {
	}
}
