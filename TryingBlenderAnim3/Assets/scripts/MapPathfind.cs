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
	public float cellSize;
	public Vector3 min;
	public Vector3 max;
	public mapNode[] devSurroundingSpots;
	public mapNode devCell;

	// Use this for initialization
	void Start () {
		ter = this.GetComponent<Terrain> ();
		Vector3 dimensions = ter.terrainData.size;
		len = dimensions [2];
		wid = dimensions [0];
		cellSize = 1f;
		numNodes = ((int)(len * wid/cellSize));
		nodesPerSide = (int) Mathf.Sqrt (numNodes);
		min = transform.position;
		max = new Vector3 (transform.position.x + wid, transform.position.y, transform.position.z + len);
		buildGridGraph ();

	}

	//return the mapNode cell in the grid that contains the given pt, or NULL if out of bounds
	public mapNode containingCell(Vector3 givenPt) {
		int zIndex = (int)((givenPt.z - transform.position.z) / cellSize);
		int xIndex = (int)((givenPt.x - transform.position.x) / cellSize);

		if (zIndex < 0 || zIndex >= nodesPerSide || xIndex < 0 || xIndex >= nodesPerSide) {
			Debug.LogAssertion ("the givenPt is outside of the terrain: " + zIndex + ", " + xIndex + ", nodesPerSide: " + nodesPerSide);
			return null;
		}

		return grid [zIndex] [xIndex];
	}

	public Queue<mapNode> findPath(mapNode start, mapNode dest, int enemyID)
	{
		Queue<mapNode> path = new Queue<mapNode>();
		KeyValuePair<int,int> startCoords = start.getIndices ();
		KeyValuePair<int,int> destCoords = dest.getIndices ();

		//reached destination (current node = destination node)
		if (startCoords.Key == destCoords.Key && startCoords.Value == destCoords.Value) {
			return path;
		}

		bool goRight = startCoords.Value < destCoords.Value;
		bool goUp = startCoords.Key < destCoords.Key;

		//z coords match
		if (startCoords.Key == destCoords.Key) {

			//go to the right
			if (goRight) {
				path.Enqueue (grid [startCoords.Key] [startCoords.Value + 1]);
				Queue<mapNode> continuation = findPath (grid [startCoords.Key] [startCoords.Value + 1], dest, enemyID);
				while (continuation.Count != 0) {
					path.Enqueue (continuation.Dequeue());
				}
				return path;
			}

			//go to the left
			else {
				path.Enqueue (grid [startCoords.Key] [startCoords.Value - 1]);
				Queue<mapNode> continuation = findPath (grid [startCoords.Key] [startCoords.Value - 1], dest, enemyID);
				while (continuation.Count != 0) {
					path.Enqueue (continuation.Dequeue());
				}
				return path;
			}
		} 

		//x coords match
		else if (startCoords.Value == destCoords.Value) {

			//go up
			if (goUp) {
				path.Enqueue (grid [startCoords.Key + 1] [startCoords.Value]);
				Queue<mapNode> continuation = findPath (grid [startCoords.Key + 1] [startCoords.Value], dest, enemyID);
				while (continuation.Count != 0) {
					path.Enqueue (continuation.Dequeue());
				}
				return path;
			}

			//go down
			else {
				path.Enqueue (grid [startCoords.Key - 1] [startCoords.Value]);
				Queue<mapNode> continuation = findPath (grid [startCoords.Key - 1] [startCoords.Value], dest, enemyID);
				while (continuation.Count != 0) {
					path.Enqueue (continuation.Dequeue ());
				}
				return path;
			}

		} else {
			if (goUp) {

				//go diagonal up and right
				if (goRight) {
					mapNode curNode = grid [startCoords.Key + 1] [startCoords.Value + 1];
					if (curNode.hasOtherOwner(enemyID)) {
						if (!grid [startCoords.Key] [startCoords.Value + 1].hasOtherOwner(enemyID)) {
							curNode = grid [startCoords.Key] [startCoords.Value + 1];
						} else if (!grid [startCoords.Key + 1] [startCoords.Value].hasOtherOwner(enemyID)) {
							curNode = grid [startCoords.Key + 1] [startCoords.Value];
						}
					} else {
						curNode.setFull (enemyID);
					}

					path.Enqueue (curNode);
					Queue<mapNode> continuation = findPath (curNode, dest, enemyID);
					while (continuation.Count != 0) {
						path.Enqueue (continuation.Dequeue ());
					}
					return path;
				} 

				//go diagonal up and left
				else {
					mapNode curNode = grid [startCoords.Key + 1] [startCoords.Value - 1];
					if (curNode.hasOtherOwner(enemyID)) {
						if (!grid [startCoords.Key] [startCoords.Value - 1].hasOtherOwner(enemyID)) {
							curNode = grid [startCoords.Key] [startCoords.Value - 1];
						} else if (!grid [startCoords.Key + 1] [startCoords.Value].hasOtherOwner(enemyID)) {
							curNode = grid [startCoords.Key + 1] [startCoords.Value];
						}
					} else {
						curNode.setFull (enemyID);
					}

					path.Enqueue (curNode);
					Queue<mapNode> continuation = findPath (curNode, dest, enemyID);
					while (continuation.Count != 0) {
						path.Enqueue (continuation.Dequeue ());
					}
					return path;
				}
			} else {

				//go diagonal down and right
				if (goRight) {
					mapNode curNode = grid [startCoords.Key - 1] [startCoords.Value + 1];
					if (curNode.hasOtherOwner(enemyID)) {
						if (!grid [startCoords.Key] [startCoords.Value + 1].hasOtherOwner(enemyID)) {
							curNode = grid [startCoords.Key] [startCoords.Value + 1];
						} else if (!grid [startCoords.Key - 1] [startCoords.Value].hasOtherOwner(enemyID)) {
							curNode = grid [startCoords.Key - 1] [startCoords.Value];
						}
					} else {
						curNode.setFull (enemyID);
					}

					path.Enqueue (curNode);
					Queue<mapNode> continuation = findPath (curNode, dest, enemyID);
					while (continuation.Count != 0) {
						path.Enqueue (continuation.Dequeue ());
					}
					return path;
				} 

				//go diagonal down and left
				else {
					mapNode curNode = grid [startCoords.Key - 1] [startCoords.Value - 1];
					if (curNode.hasOtherOwner(enemyID)) {
						if (!grid [startCoords.Key] [startCoords.Value - 1].hasOtherOwner(enemyID)) {
							curNode = grid [startCoords.Key] [startCoords.Value - 1];
						} else if (!grid [startCoords.Key - 1] [startCoords.Value].hasOtherOwner(enemyID)) {
							curNode = grid [startCoords.Key - 1] [startCoords.Value];
						}
					} else {
						curNode.setFull (enemyID);
					}
						
					path.Enqueue (curNode);
					Queue<mapNode> continuation = findPath (curNode, dest, enemyID);
					while (continuation.Count != 0) {
						path.Enqueue (continuation.Dequeue ());
					}
					return path;
				}
			}
		}
	}

	void buildGridGraph(){
		
		//set up the dimensions of the 2d array
		grid = new mapNode[nodesPerSide][];
		for (int z = 0; z < nodesPerSide; ++z) {
			grid [z] = new mapNode[nodesPerSide];
		}

		//build each node, starting with the one at (0, 0)
		Vector3 terrainOrigin = transform.position;
		Vector3 originCenter = new Vector3 (terrainOrigin.x + (cellSize/2f), terrainOrigin.y, terrainOrigin.z + (cellSize/2f));

		for (int z = 0; z < nodesPerSide; ++z) {
			for (int x = 0; x < nodesPerSide; ++x) {
				grid [z] [x] = new mapNode (new Vector3(originCenter.x + (x*cellSize), originCenter.y, originCenter.z + (z*cellSize)), z, x);
			}
		}

		//connect the nodes via setNeighbors, forming a grid graph
		{
			mapNode[] neighbors = new mapNode[3];
			neighbors [0] = grid [0] [1];
			neighbors [1] = grid [1] [0];
			neighbors [2] = grid [1] [1];

//			mapNode[] neighbors = new mapNode[2];
//			neighbors [0] = grid [0] [1];
//			neighbors [1] = grid [1] [0];

			grid [0] [0].setNeighbors (neighbors);
		}

		{
			mapNode[] neighbors = new mapNode[3];
			neighbors [0] = grid [0] [nodesPerSide - 2];
			neighbors [1] = grid [1] [nodesPerSide - 1];
			neighbors [2] = grid [1] [nodesPerSide - 2];

//			mapNode[] neighbors = new mapNode[2];
//			neighbors [0] = grid [0] [nodesPerSide - 2];
//			neighbors [1] = grid [1] [nodesPerSide - 1];

			grid [0] [nodesPerSide - 1].setNeighbors (neighbors);
		}
		{
			mapNode[] neighbors = new mapNode[3];
			neighbors [0] = grid [nodesPerSide - 2] [nodesPerSide - 1];
			neighbors [1] = grid [nodesPerSide - 1] [nodesPerSide - 2];
			neighbors [2] = grid [nodesPerSide - 2] [nodesPerSide - 2];

//			mapNode[] neighbors = new mapNode[2];
//			neighbors [0] = grid [nodesPerSide - 2] [nodesPerSide - 1];
//			neighbors [1] = grid [nodesPerSide - 1] [nodesPerSide - 2];

			grid [nodesPerSide - 1] [nodesPerSide - 1].setNeighbors (neighbors);
		}
		{
			mapNode[] neighbors = new mapNode[3];
			neighbors [0] = grid [nodesPerSide - 2] [0];
			neighbors [1] = grid [nodesPerSide - 1] [1];
			neighbors [2] = grid [nodesPerSide - 2] [1];

//			mapNode[] neighbors = new mapNode[2];
//			neighbors [0] = grid [nodesPerSide - 2] [0];
//			neighbors [1] = grid [nodesPerSide - 1] [1];

			grid [nodesPerSide - 1] [0].setNeighbors (neighbors);
		}

		for (int x = 1; x < nodesPerSide - 1; ++x) {
			{
				//bottom row
				mapNode[] neighbors = new mapNode[5];
				neighbors [0] = grid [1] [x];
				neighbors [1] = grid [0] [x + 1];
				neighbors [2] = grid [0] [x - 1];
				neighbors [3] = grid [1] [x + 1];
				neighbors [4] = grid [1] [x - 1];

//				mapNode[] neighbors = new mapNode[3];
//				neighbors [0] = grid [1] [x];
//				neighbors [1] = grid [0] [x + 1];
//				neighbors [2] = grid [0] [x - 1];

				grid [0] [x].setNeighbors (neighbors);
			}
			{
				//top row
				mapNode[] neighbors = new mapNode[5];
				neighbors [0] = grid [nodesPerSide - 2] [x];
				neighbors [1] = grid [nodesPerSide - 1] [x + 1];
				neighbors [2] = grid [nodesPerSide - 1] [x - 1];
				neighbors [3] = grid [nodesPerSide - 2] [x + 1];
				neighbors [4] = grid [nodesPerSide - 1] [x - 1];
				grid [nodesPerSide - 1] [x].setNeighbors (neighbors);
			}
		}

		for(int z = 1; z < nodesPerSide-1; ++z) {
			{
				//leftmost column
				mapNode[] neighbors = new mapNode[5];
				neighbors [0] = grid [z] [1];
				neighbors [1] = grid [z + 1] [0];
				neighbors [2] = grid [z - 1] [0];
				neighbors [3] = grid [z + 1] [1];
				neighbors [4] = grid [z - 1] [1];

//				mapNode[] neighbors = new mapNode[3];
//				neighbors [0] = grid [z] [1];
//				neighbors [1] = grid [z + 1] [0];
//				neighbors [2] = grid [z - 1] [0];

				grid [z] [0].setNeighbors (neighbors);
			}
			{
				//rightmost column
				mapNode[] neighbors = new mapNode[5];
				neighbors [0] = grid [z] [nodesPerSide - 2];
				neighbors [1] = grid [z + 1] [nodesPerSide - 1];
				neighbors [2] = grid [z - 1] [nodesPerSide - 1];
				neighbors [3] = grid [z + 1] [nodesPerSide - 2];
				neighbors [4] = grid [z - 1] [nodesPerSide - 2];

//				mapNode[] neighbors = new mapNode[3];
//				neighbors [0] = grid [z] [nodesPerSide - 2];
//				neighbors [1] = grid [z + 1] [nodesPerSide - 1];
//				neighbors [2] = grid [z - 1] [nodesPerSide - 1];

				grid [z] [nodesPerSide - 1].setNeighbors (neighbors);
			}
		}

		//all middle cells
		for (int z = 1; z < nodesPerSide-1; ++z) {
			for (int x = 1; x < nodesPerSide-1; ++x) {
				mapNode[] neighbors = new mapNode[8];
				neighbors [0] = grid [z+1] [x];
				neighbors [1] = grid [z-1] [x];
				neighbors [2] = grid [z] [x+1];
				neighbors [3] = grid [z] [x-1];
				neighbors [4] = grid [z+1] [x+1];
				neighbors [5] = grid [z+1] [x-1];
				neighbors [6] = grid [z-1] [x+1];
				neighbors [7] = grid [z-1] [x-1];

//				mapNode[] neighbors = new mapNode[4];
//				neighbors [0] = grid [z+1] [x+1];
//				neighbors [1] = grid [z+1] [x-1];
//				neighbors [2] = grid [z-1] [x+1];
//				neighbors [3] = grid [z-1] [x-1];

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
//	Vector3[] points;

	Vector3 center;
	mapNode[] neighbors;
	int zIndex;
	int xIndex;
	int owner;

	public mapNode(Vector3 ctr, int zIdx, int xIdx) {
		center = ctr;
		zIndex = zIdx;
		xIndex = xIdx;
		owner = -1;
//		points = new Vector3[4];
//		float halfCell = 0.5f * GameObject.Find ("Terrain").GetComponent<MapPathfind> ().cellSize;
//		points[0] = new Vector3(ctr.x - halfCell, ctr.y, ctr.z - halfCell);
//		points[1] = new Vector3(ctr.x - halfCell, ctr.y, ctr.z + halfCell);
//		points[2] = new Vector3(ctr.x + halfCell, ctr.y, ctr.z + halfCell);
//		points[3] = new Vector3(ctr.x + halfCell, ctr.y, ctr.z - halfCell);
	}

	public bool hasOtherOwner (int yourEnemyID) {
		return owner != -1 && owner != yourEnemyID;
	}

//	public bool isFull(){
//		return owner != -1;
//	}


	public void setFull(int enemyID){
		owner = enemyID;
	}

	public void setEmpty(){
		owner = -1;
	}
		
	public bool equalTo(mapNode other){
		KeyValuePair<int, int> b = other.getIndices ();
		return (zIndex == b.Key) && (xIndex == b.Value);
	}

	public void setNeighbors(mapNode[] n){
		neighbors = n;
	}

	public mapNode[] getNeighbors(){
		return neighbors; 
	}

	public void setSurroundingSpots(){
		GameObject.Find ("Terrain").GetComponent<MapPathfind> ().devSurroundingSpots = getSurroundingSpots ();
	}

	//get the spots around Dev's neighbors where enemies can gather and wait their turn to attack
	public mapNode[] getSurroundingSpots(){

		//get the neighbors of the neighbors
		int count = 0;
		List<mapNode> outerCircle = new List<mapNode>();
		foreach (mapNode neighbor in neighbors) {
			mapNode[] neighborsSquared = neighbor.getNeighbors ();
			outerCircle.AddRange(neighborsSquared);
			count += neighborsSquared.Length;
		}

		//remove all of the direct neighbors of Dev from outerCircle
		//so that we are only left with the indirect neighbors
		foreach(mapNode directNeighbor in neighbors){
			int idx = -1;
			while ((idx = outerCircle.IndexOf (directNeighbor)) >= 0) {
				outerCircle.RemoveAt(idx);
				idx = -1;
				--count;
			}
		}


//		mapNode[] outerCircleArr = new mapNode[outerCircle.Capacity];
//		for(int idx = 0; idx < count; ++idx) {
//			outerCircleArr [idx] = (mapNode) outerCircle [idx];
//		}
//
//		return outerCircleArr;

		return outerCircle.ToArray ();
	}

	public mapNode getEmptySurroundingSpot(int yourEnemyID){
		mapNode[] outerCircle = GameObject.Find ("Terrain").GetComponent<MapPathfind> ().devSurroundingSpots;
//		string s = "";
//		foreach (mapNode node in outerCircle) {
//			s += (node.toString() + " ");
//		}
//		Debug.LogError (s);
		foreach (mapNode node in outerCircle) {
			if (!node.hasOtherOwner (yourEnemyID)) {
				return node;
			}
		}
		return null;
	}

	private object[] getEmptyNeighbors(int yourEnemyID){
		ArrayList emptyNeighbors = new ArrayList ();
		foreach (mapNode n in neighbors) {
			if (!n.hasOtherOwner (yourEnemyID))
				emptyNeighbors.Add (n);
		}
		return emptyNeighbors.ToArray ();
	}

	public mapNode getClosestNeighbor(mapNode other, int yourEnemyID){
		object[] emptyNeighbors = getEmptyNeighbors (yourEnemyID);

		float[] distances = new float[emptyNeighbors.Length];
		int idx = 0;
		foreach (object n in emptyNeighbors) {
			distances [idx++] = Vector3.Distance (other.center, ((mapNode)(n)).center);
		}
		float minDist = distances[0];
		int minIdx = 0;
		for (idx = 1; idx < distances.Length; ++idx) {
			if (distances[idx] < minDist) {
				minDist = distances[idx];
				minIdx = idx;
			}
		}
		return neighbors [minIdx];
	}

	public Vector3 getCenter(){
		return center;
	}

	public KeyValuePair<int, int> getIndices(){
		return new KeyValuePair<int, int> (zIndex, xIndex);
	}

	public string toString(){
		return getIndices ().ToString ();
	}

//	public Vector3[] getPoints(){
//		return points;
//	}


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
