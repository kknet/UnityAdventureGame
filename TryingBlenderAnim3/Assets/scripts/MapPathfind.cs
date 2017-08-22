using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPathfind : MonoBehaviour {

	private float len;
	private float wid;
	private Terrain ter;

	// Use this for initialization
	void Start () {
		ter = this.GetComponent<Terrain> ();
		Vector3 dimensions = ter.terrainData.size;
		len = dimensions [2];
		wid = dimensions [0];
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public float length(){
		return len;
	}
	public float width(){
		return wid;
	}
}
