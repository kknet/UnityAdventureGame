using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPathfind : MonoBehaviour {

	public mapNode[][] grid;

	private float len;
	private float wid;
	private Terrain ter;
	private int numNodes;
	private int nodesPerSide;
	public Vector3 min;
	public Vector3 max;

	// Use this for initialization
	void Start () {
		ter = this.GetComponent<Terrain> ();
		Vector3 dimensions = ter.terrainData.size;
		len = dimensions [2];
		wid = dimensions [0];
		numNodes = ((int)(len * wid)) / 4;
		nodesPerSide = (int) Mathf.Sqrt (numNodes);
		min = transform.position;
		max = new Vector3 (transform.position.x + wid, transform.position.y, transform.position.z + len);
		buildGridGraph ();
	}

	//return the mapNode cell in the grid that contains the given pt, or NULL if out of bounds
	public mapNode containingCell(Vector3 givenPt) {
		int zIndex = (int)((givenPt.z - transform.position.z) / 2f);
		int xIndex = (int)((givenPt.x - transform.position.x) / 2f);

		if (zIndex < 0 || zIndex >= nodesPerSide || xIndex < 0 || xIndex >= nodesPerSide) {
			Debug.LogAssertion ("the givenPt is outside of the terrain: " + zIndex + ", " + xIndex + ", nodesPerSide: " + nodesPerSide);
			return null;
		}

		return grid [zIndex] [xIndex];
	}

	void buildGridGraph(){
		//set up the dimensions of the 2d array
		
		grid = new mapNode[nodesPerSide][];
		for (int z = 0; z < nodesPerSide; ++z) {
			grid [z] = new mapNode[nodesPerSide];
		}

		//build each node, starting with the one at (0, 0)
		Vector3 terrainOrigin = new Vector3 (transform.position.x - 15f, transform.position.y, transform.position.z - 15f);
		Vector3 originCenter = new Vector3 (terrainOrigin.x + 1f, terrainOrigin.y, terrainOrigin.z + 1f);

		for (int z = 0; z < nodesPerSide; ++z) {
			for (int x = 0; x < nodesPerSide; ++x) {
				grid [z] [x] = new mapNode (new Vector3(originCenter.x + (x*2f), originCenter.y, originCenter.z + (z*2f)), z, x);
			}
		}

		//connect the nodes via setNeighbors, forming a grid graph
		mapNode[] neighbors = new mapNode[3];
		neighbors [0] = grid [0] [1];
		neighbors [1] = grid [1] [0];
		neighbors [2] = grid [1] [1];
		grid [0] [0].setNeighbors(neighbors);

		neighbors [0] = grid [0] [nodesPerSide-2];
		neighbors [1] = grid [1] [nodesPerSide-1];
		neighbors [2] = grid [1] [nodesPerSide-2];
		grid [0] [nodesPerSide-1].setNeighbors(neighbors);

		neighbors [0] = grid [nodesPerSide-2] [nodesPerSide-1];
		neighbors [1] = grid [nodesPerSide-1] [nodesPerSide-2];
		neighbors [2] = grid [nodesPerSide-2] [nodesPerSide-2];
		grid [nodesPerSide-1] [nodesPerSide-1].setNeighbors(neighbors);


		neighbors [0] = grid [nodesPerSide-2] [0];
		neighbors [1] = grid [nodesPerSide-1] [1];
		neighbors [2] = grid [nodesPerSide-2] [1];
		grid [nodesPerSide-1] [0].setNeighbors(neighbors);


		neighbors = new mapNode[5];
		for (int x = 1; x < nodesPerSide - 1; ++x) {

			//bottom row
			neighbors [0] = grid [1] [x];
			neighbors [1] = grid [0] [x + 1];
			neighbors [2] = grid [0] [x - 1];
			neighbors [3] = grid [1] [x + 1];
			neighbors [4] = grid [1] [x - 1];
			grid [0] [x].setNeighbors (neighbors);

			//top row
			neighbors [0] = grid [nodesPerSide - 2] [x];
			neighbors [1] = grid [nodesPerSide - 1] [x + 1];
			neighbors [2] = grid [nodesPerSide - 1] [x - 1];
			neighbors [3] = grid [nodesPerSide - 2] [x + 1];
			neighbors [4] = grid [nodesPerSide - 1] [x - 1];
			grid [nodesPerSide - 1] [x].setNeighbors (neighbors);
		}

		for(int z = 1; z < nodesPerSide-1; ++z) {
			//leftmost column
			neighbors [0] = grid [z]   [1];
			neighbors [1] = grid [z+1] [0];
			neighbors [2] = grid [z-1] [0];
			neighbors [3] = grid [z+1] [1];
			neighbors [4] = grid [z-1] [1];
			grid [z] [0].setNeighbors(neighbors);

			//rightmost column
			neighbors [0] = grid [z]   [nodesPerSide-2];
			neighbors [1] = grid [z+1] [nodesPerSide-1];
			neighbors [2] = grid [z-1] [nodesPerSide-1];
			neighbors [3] = grid [z+1] [nodesPerSide-2];
			neighbors [4] = grid [z-1] [nodesPerSide-2];
			grid [z] [nodesPerSide-1].setNeighbors(neighbors);
		}

		//all middle cells
		neighbors = new mapNode[8];
		for (int z = 1; z < nodesPerSide-1; ++z) {
			for (int x = 1; x < nodesPerSide-1; ++x) {
				neighbors [0] = grid [z+1] [x];
				neighbors [1] = grid [z-1] [x];
				neighbors [2] = grid [z] [x+1];
				neighbors [3] = grid [z] [x-1];
				neighbors [4] = grid [z+1] [x+1];
				neighbors [5] = grid [z+1] [x-1];
				neighbors [6] = grid [z-1] [x+1];
				neighbors [7] = grid [z-1] [x-1];
				grid [z] [x].setNeighbors(neighbors);
			}
		}
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

	//format for points: 
	// 1: (minX, maxZ), 2: (maxX, maxZ)
	// 0: (minX, minZ), 3: (maxX, minZ)
	//edges are 0 to 1, 1 to 2, 2 to 3, and 3 to 0
	//the 0,1,2,3 is the order of the points in the points[] array

	Vector3[] points;
	Vector3 center;
	mapNode[] neighbors;
	int zIndex;
	int xIndex;

	public mapNode(Vector3 ctr, int zIdx, int xIdx) {
		center = ctr;
		points = new Vector3[4];
		points[0] = new Vector3(ctr.x - 1f, ctr.y, ctr.z - 1f);
		points[1] = new Vector3(ctr.x - 1f, ctr.y, ctr.z + 1f);
		points[2] = new Vector3(ctr.x + 1f, ctr.y, ctr.z + 1f);
		points[3] = new Vector3(ctr.x + 1f, ctr.y, ctr.z - 1f);
		zIndex = zIdx;
		xIndex = xIdx;
	}

	public void setNeighbors(mapNode[] n){
		neighbors = n;
	}

	public mapNode[] getNeighbors(){
		return neighbors;
	}

	public Vector3 getCenter(){
		return center;
	}

	public Vector3[] getPoints(){
		return points;
	}

	public KeyValuePair<int, int> getIndices(){
		return new KeyValuePair<int, int> (zIndex, xIndex);
	}

//	public Vector3 getCenter(){
//		float cenX = 0f;
//		float cenZ = 0f;
//		foreach(Vector3 pt in points){
//			cenX += pt[0];
//			cenZ += pt[2];
//		}
//		cenX = cenX/4f;
//		cenZ = cenZ/4f;
//		return new Vector3(cenX, 0f, cenZ);
//	}
//
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
