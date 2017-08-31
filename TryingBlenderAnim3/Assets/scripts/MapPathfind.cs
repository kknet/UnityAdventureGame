using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPathfind : MonoBehaviour {

	public mapNode[][] grid;
	public float cellSize;
	public Vector3 min;
	public Vector3 max;
	public mapNode devCell;
	public bool doneBuilding;

	private float len;
	private float wid;
	private Terrain ter;
	private int numNodes;
	private int nodesPerSide;

	// Use this for initialization
	public void Start () {
		doneBuilding = false;
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
		doneBuilding = true;
		GameObject.Find ("DevDrake").GetComponent<DevMovement> ().Start ();
	}

	//return the mapNode cell in the grid that contains the given pt, or NULL if out of bounds
	public mapNode containingCell(Vector3 givenPt) {
		int zIndex = Mathf.RoundToInt((givenPt.z - transform.position.z) / cellSize);
		int xIndex = Mathf.RoundToInt((givenPt.x - transform.position.x) / cellSize);

		if (zIndex < 0 || zIndex >= nodesPerSide || xIndex < 0 || xIndex >= nodesPerSide) {
			Debug.LogAssertion ("the givenPt is outside of the terrain: " + zIndex + ", " + xIndex + ", nodesPerSide: " + nodesPerSide);
			return null;
		}

		return grid [zIndex] [xIndex];
	}

	//Finding the node out of a list that is nearest to your node.
	//Based on a naive computation of distance between the centers of two nodes
	//A better computation would compute distance on the basis of 
	//the shortest available path between the nodes.
	public mapNode findClosestNode(mapNode[] list, mapNode yourNode){
		float minDistance = Vector3.Distance (list [0].getCenter (), yourNode.getCenter ());	
		int closestIdx = 0;
		for (int idx = 1; idx < list.Length; ++idx) {
			float thisDistance = Vector3.Distance (list [idx].getCenter (), yourNode.getCenter ());
			if (thisDistance < minDistance) {
				minDistance = thisDistance;
				closestIdx = idx;
			}
		}
		return list [closestIdx];
	}

	//Extracts all empty nodes from given list.
	//'Empty' nodes include those nodes which are not occupied by anyone
	//and those nodes which are occupied by the caller itself. 
	//The caller is described by 'yourEnemyID'
	private mapNode[] extractEmptyNodes(mapNode[] list, int yourEnemyID){
		List<mapNode> empties = new List<mapNode> ();
		foreach (mapNode node in list) {
			if (!node.hasOtherOwner (yourEnemyID)) {
				empties.Add (node);
			}
		}
		return empties.ToArray();
	}

	//returns the empty version of the spacedDevCombatCircle
	public mapNode[] getEmptySpacedDevCombatCircle(int stepsOut, int yourEnemyID){
		mapNode[] spacedCombatCircle = getSpacedDevCombatCircle (stepsOut);
		return extractEmptyNodes (spacedCombatCircle, yourEnemyID); 
	}

	//returns a list of nodes with the maximum spacing in between the nodes to ensure
	//that each enemy gets its own space. The nodes may not be empty!
	public mapNode[] getSpacedDevCombatCircle(int stepsOut){
		mapNode[] combatCircle = calculateDevCombatCircle (stepsOut);
		GameObject[] enemies = GameObject.Find ("DevDrake").GetComponent<DevMovement> ().getEnemies ();
		int spacing = Mathf.FloorToInt(combatCircle.Length / enemies.Length);
		mapNode[] spacedCombatCircle = new mapNode[enemies.Length];
		for (int idx = 0; idx < enemies.Length; ++idx) {
			spacedCombatCircle [idx] = combatCircle [idx * spacing];
		}
		return spacedCombatCircle;
	}

	//returns the list of empty nodes of the devCombatCircle
	public mapNode[] getEmptyDevCombatCircle(int stepsOut, int yourEnemyID){
		mapNode[] combatCircle = calculateDevCombatCircle (stepsOut);
		return extractEmptyNodes (combatCircle, yourEnemyID); 
	}


	//Calculates the box-shaped list of cells that are STEPSOUT steps 
	//out from dev's cell (only supported for 1, 2, or 3 stepsOut) 
	public mapNode[] calculateDevCombatCircle(int stepsOut) {
		KeyValuePair<int, int> devInd = devCell.getIndices ();
		int z = devInd.Key;
		int x = devInd.Value;
		switch (stepsOut) {
		case 1:
			{
				mapNode[] combatCircle = new mapNode[8];
				combatCircle [0] = grid [z+1] [x-1];
				combatCircle [1] = grid [z+1] [x];
				combatCircle [2] = grid [z+1] [x+1];
				combatCircle [3] = grid [z] [x+1];
				combatCircle [4] = grid [z-1] [x+1];
				combatCircle [5] = grid [z-1] [x];
				combatCircle [6] = grid [z-1] [x-1];
				combatCircle [7] = grid [z] [x-1];
				return combatCircle;
			}
		case 2:
			{
				mapNode[] combatCircle = new mapNode[16];
				combatCircle [0] = grid [z+2] [x-2];
				combatCircle [1] = grid [z+2] [x-1];
				combatCircle [2] = grid [z+2] [x];
				combatCircle [3] = grid [z+2] [x+1];
				combatCircle [4] = grid [z+2] [x+2];
				combatCircle [5] = grid [z+1] [x+2];
				combatCircle [6] = grid [z] [x+2];
				combatCircle [7] = grid [z-1] [x+2];
				combatCircle [8] = grid [z-2] [x+2];
				combatCircle [9] = grid [z-2] [x+1];
				combatCircle [10] = grid [z-2] [x];
				combatCircle [11] = grid [z-2] [x-1];
				combatCircle [12] = grid [z-2] [x-2];
				combatCircle [13] = grid [z-1] [x-2];
				combatCircle [14] = grid [z] [x-2];
				combatCircle [15] = grid [z+1] [x-2];
				return combatCircle;
			}
		case 3:
			{
				mapNode[] combatCircle = new mapNode[24];
				combatCircle [0] = grid [z+3] [x-3];
				combatCircle [1] = grid [z+3] [x-2];
				combatCircle [2] = grid [z+3] [x-1];
				combatCircle [3] = grid [z+3] [x];
				combatCircle [4] = grid [z+3] [x+1];
				combatCircle [5] = grid [z+3] [x+2];
				combatCircle [6] = grid [z+3] [x+3];
				combatCircle [7] = grid [z+2] [x+3];
				combatCircle [8] = grid [z+1] [x+3];
				combatCircle [9] = grid [z] [x+3];
				combatCircle [10] = grid [z-1] [x+3];
				combatCircle [11] = grid [z-2] [x+3];
				combatCircle [12] = grid [z-3] [x+3];
				combatCircle [13] = grid [z-3] [x+2];
				combatCircle [14] = grid [z-3] [x+1];
				combatCircle [15] = grid [z-3] [x];
				combatCircle [16] = grid [z-3] [x-1];
				combatCircle [17] = grid [z-3] [x-2];
				combatCircle [18] = grid [z-3] [x-3];
				combatCircle [19] = grid [z-2] [x-3];
				combatCircle [20] = grid [z-1] [x-3];
				combatCircle [21] = grid [z] [x-3];
				combatCircle [22] = grid [z+1] [x-3];
				combatCircle [23] = grid [z+2] [x-3];
				return combatCircle;				
			}
		default:
			{
				Debug.LogAssertion("input 'stepsOut' needs to 1, 2 or 3. You input " + stepsOut + " !");
				return null;
			}
		}
	}

	//finds a path from START to DEST with no 'empty' nodes in between
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
			grid [0] [0].setNeighbors (neighbors);
		}
		{
			mapNode[] neighbors = new mapNode[3];
			neighbors [0] = grid [0] [nodesPerSide - 2];
			neighbors [1] = grid [1] [nodesPerSide - 1];
			neighbors [2] = grid [1] [nodesPerSide - 2];
			grid [0] [nodesPerSide - 1].setNeighbors (neighbors);
		}
		{
			mapNode[] neighbors = new mapNode[3];
			neighbors [0] = grid [nodesPerSide - 2] [nodesPerSide - 1];
			neighbors [1] = grid [nodesPerSide - 1] [nodesPerSide - 2];
			neighbors [2] = grid [nodesPerSide - 2] [nodesPerSide - 2];
			grid [nodesPerSide - 1] [nodesPerSide - 1].setNeighbors (neighbors);
		}
		{
			mapNode[] neighbors = new mapNode[3];
			neighbors [0] = grid [nodesPerSide - 2] [0];
			neighbors [1] = grid [nodesPerSide - 1] [1];
			neighbors [2] = grid [nodesPerSide - 2] [1];
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
				grid [z] [x].setNeighbors(neighbors);
			}
		}
	}

	public float length(){
		return len;
	}
	public float width(){
		return wid;
	}
}



