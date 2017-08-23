using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPathfind : MonoBehaviour {

	private float len;
	private float wid;
	private Terrain ter;
	private int numNodes;
	public Vector3 min;
	public Vector3 max;

	// Use this for initialization
	void Start () {
		ter = this.GetComponent<Terrain> ();
		Vector3 dimensions = ter.terrainData.size;
		len = dimensions [2];
		wid = dimensions [0];
		numNodes = ((int)(len * wid)) / 4;
		min = new Vector3 (transform.position.x - (wid / 2f), transform.position.y, transform.position.z- (len / 2f));
		max = new Vector3 (transform.position.x + (wid / 2f), transform.position.y, transform.position.z + (len / 2f));
	}

	void buildGridGraph(){
	
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



public class mapNode { 

	//format: 
	// 0: (minX, minZ), 1: (minX, maxZ)
	// 2 :(maxX, minZ), 3: (maxX, maxZ)
	Vector3[] points;

	public mapNode(Vector3[] pts) {
		points = pts;
	}

	public Vector3 getCenter(){
		float cenX = 0f;
		float cenZ = 0f;
		foreach(Vector3 pt in points){
			cenX += pt[0];
			cenZ += pt[2];
		}
		cenX = cenX/4f;
		cenZ = cenZ/4f;
		return new Vector3(cenX, 0f, cenZ);
	}

//	public mapNode[] getNeighbors(){
//		bool hasMinX = Mathf.Approximately (points [0][0], GameObject.Find ("Terrain").GetComponent<MapPathfind> ().min [0]);
//		bool hasMaxX = Mathf.Approximately (points [4][0], GameObject.Find ("Terrain").GetComponent<MapPathfind> ().max [0]);
//
//		bool hasMinZ = Mathf.Approximately (points [0][2], GameObject.Find ("Terrain").GetComponent<MapPathfind> ().min [2]);
//		bool hasMaxZ = Mathf.Approximately (points [4][2], GameObject.Find ("Terrain").GetComponent<MapPathfind> ().max [2]);
//
//		//-------------------------------------------------------------------------------------------------
//	   //rotate your camera so that forward is +Z and right is +X, and you're looking down at the terrain
//	  //-------------------------------------------------------------------------------------------------
//		if (hasMinX) {
//			if (hasMinZ) {//minX & minZ (bottom left corner)
//			//3 neighbors: 1 up, and 1 diagonal
//				//1 right
//				Vector3 [][] neighbors = new Vector3[4][3];
//				neighbors [0] [0] = points [2];
//				neighbors [0] [1] = points [3];
//				neighbors [0] [2] = new Vector3 (points [2] [0] + 2f, points [2] [1], points [2] [2]);
//				neighbors [0] [3] = new Vector3 (points [3] [0] + 2f, points [3] [1], points [3] [2]);
//			
//				//1 up
//				neighbors [0] [0] = points [1];
//				neighbors [0] [1] = points [3];
//				neighbors[0][
//				neighbors [0] [2] = new Vector3 (points [2] [0] + 2f, points [2] [1], points [2] [2]);
//				neighbors [0] [3] = new Vector3 (points [3] [0] + 2f, points [3] [1], points [3] [2]);
//			
//				
//			
//			
//			} else if (hasMaxZ) {//minX & maxZ (top left corner)
//			//3 neighbors: 1 right, 1 down, and 1 diagonal
//				Vector3 [] neighbors = new Vector3[3];
//
//			} else {//just minX (leftmost column (excluding corners))
//			//5 neighbors: 1 down, 1 right, 1 up, 2 diagonals
//				Vector3 [] neighbors = new Vector3[5];
//			}
//		} else if (hasMaxX) {
//			if (hasMinZ) {//maxX & minZ (bottom right corner)
//			//3 neighbors: 1 left, 1 up, 1 diagonal
//				Vector3 [] neighbors = new Vector3[3];
//
//			} else if (hasMaxZ) {//maxX & maxZ (top right corner)
//			//3 neighbors: 1 left, 1 down, 1 diagonal
//				Vector3 [] neighbors = new Vector3[3];
//
//			} else {//just maxX (rightmost column (excluding corners))
//			//5 neighbors: 1 down, 1 left, 1 up, 2 diagonals
//			}
//		} else if (hasMinZ) {//just minZ (bottom row)
//			//5 neighbors: 
//		} else if (hasMaxZ) {//just maxZ (top row)
//		} else {//not in any of the edges or corners; in the middle --> 8 neighbors!
//		}
//
//
//	}
}
