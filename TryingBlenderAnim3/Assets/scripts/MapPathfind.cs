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
	public int nodesPerSide;
//	public IDictionary<KeyValuePair<mapNode, mapNode>, Queue<mapNode>>


	private float len;
	private float wid;
	private Terrain ter;
	private int numNodes;
	private GameObject Dev;
	public SortedList<int, GameObject> enemies;

	// Use this for initialization
	public void Start () {
		doneBuilding = false;
		Dev = GameObject.Find ("DevDrake");
		ter = this.GetComponent<Terrain> ();
		Vector3 dimensions = ter.terrainData.size;
		len = dimensions [2];
		wid = dimensions [0];
		cellSize = 1.3f;
		numNodes = ((int)(len * wid/cellSize));
		nodesPerSide = (int) Mathf.Sqrt (numNodes);
		min = transform.position;
		max = new Vector3 (transform.position.x + wid, transform.position.y, transform.position.z + len);
		buildGridGraph ();
		sortEnemiesByID ();
		GetComponent<TrackObstacles> ().markTreesAsFull ();
		doneBuilding = true;
	}

//	void Update(){
//		markSpots ();
//	}

	public void markSpots(){
		GameObject[] enemies = getEnemies ();
		foreach (GameObject enemy in enemies) {
			enemy.GetComponent<EnemyAI> ().updateYourCell ();
		}
		containingCell (Dev.transform.position).setFull (-3);
	}

	private void fillEnemiesList(){
		enemies = new SortedList<int, GameObject> ();
		GameObject[] unsorted = getEnemies ();
		foreach (GameObject enemy in unsorted) {
			enemies.Add (enemy.GetComponent<EnemyAI> ().enemyID, enemy);
		}
	}

	private void sortEnemiesByID() {
		fillEnemiesList ();
//		GameObject[] temp = getEnemies ();
//		List<GameObject> unsorted = new List<GameObject>(temp);
//		GameObject[] sorted = new GameObject[temp.Length];
//		int curID = 1;
//		while(curID!=temp.Length+1) {
//			foreach (GameObject enemy in unsorted) {
//				if (enemy.GetComponent<EnemyAI> ().enemyID==curID) {
//					sorted [curID - 1] = enemy;
//					break;
//				}
//			}
//			++curID;
//			unsorted.Remove (sorted[curID-2]);
//		}
//		enemies = sorted;
	}

	public GameObject getEnemyByID(int enemyID){
		return enemies [enemyID];
	}

	public mapNode[] removeFromList(mapNode trashItem, mapNode[] list){
		List<mapNode> newList = new List<mapNode>(list);
		newList.Remove (trashItem);
//		foreach (mapNode item in list) {
//			if (!trashItem.equalTo (item)) {
//				newList.Add(item);
//			}
//		}
		return newList.ToArray();
	}

	public GameObject[] getEnemies(){
		return GameObject.FindGameObjectsWithTag ("Enemy");
	}

//	public void fixAllOverlaps(){
//		GameObject[] enemies = getEnemies();
//
//		foreach (GameObject enemy in enemies) {
//			if (!enemy.GetComponent<EnemyAI> ().doneStarting)
//				return;
//		}
//
//		foreach (GameObject enemy in enemies) {
//			Debug.LogError (enemy.GetComponent<EnemyAI> ().finalDest.getIndices () + " " + enemy.GetComponent<EnemyAI> ().enemyID);
//		}
//
//
//		GameObject overlapper = overlappingAgent ();
//		if (overlapper != null) {
//			Debug.Log ("overlapper: " + overlapper.GetComponent<EnemyAI> ().enemyID);
//			overlapper.GetComponent<EnemyAI> ().plotNewPath ();
//			Debug.LogError ("Worked!");
//		}
//	}
//
//	public GameObject overlappingAgent(){
//		//find out what cells the enemies' finalDests are
//		GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");
//		mapNode[] locs = new mapNode[enemies.Length];
//		for (int idx = 0; idx < enemies.Length; ++idx) {
//			locs [idx] = containingCell (enemies [idx].GetComponent<EnemyAI> ().finalDest.getCenter());
//		}
//			
//		//if there is an enemy that shares a finalDest with another enemy, return that enemy
//		for(int checkIdx = 0; checkIdx < enemies.Length; ++checkIdx) {
//			mapNode checkNode = locs [checkIdx];
//			for (int idx = 0; idx < enemies.Length; ++idx) {
//				if (idx == checkIdx)
//					continue;
//
//				if (checkNode == locs [idx])
//					return enemies [idx];
//			}
//		}
//
//		//if no two enemies are in the same finalDest, return null
//		return null;
//	}

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

	//Finding the node out of a list that is nearest to your node.
	//Based on a naive computation of distance between the centers of two nodes
	//A better computation would compute distance on the basis of 
	//the shortest available path between the nodes.
	public KeyValuePair<mapNode, float> findClosestNodeDist(mapNode[] list, mapNode yourNode){
		float minDistance = Vector3.Distance (list [0].getCenter (), yourNode.getCenter ());	
		int closestIdx = 0;
		for (int idx = 1; idx < list.Length; ++idx) {
			float thisDistance = Vector3.Distance (list [idx].getCenter (), yourNode.getCenter ());
			if (thisDistance < minDistance) {
				minDistance = thisDistance;
				closestIdx = idx;
			}
		}
		return new KeyValuePair<mapNode, float>(list [closestIdx], minDistance);
	}


	//Extracts all empty nodes from given list.
	//'Empty' nodes include those nodes which are not occupied by anyone
	//and those nodes which are occupied by the caller itself. 
	//The caller is described by 'yourEnemyID'
	public mapNode[] extractEmptyNodes(mapNode[] list, int yourEnemyID){
		List<mapNode> empties = new List<mapNode> ();
		foreach (mapNode node in list) {
			if (node!=null && !node.hasOtherOwner (yourEnemyID)) {
				empties.Add (node);
			}
		}
		return empties.ToArray();
	}

	//Extracts all empty nodes from given list.
	//'Empty' nodes include those nodes which are not occupied by anyone
	//and those nodes which are occupied by the caller itself. 
	//The caller is described by 'yourEnemyID'
	public List<mapNode> extractEmptyNodes(List<mapNode> list, int yourEnemyID){
		List<mapNode> empties = new List<mapNode> ();
		foreach (mapNode node in list) {
			if (node!=null && !node.hasOtherOwner (yourEnemyID)) {
				empties.Add (node);
			}
		}
		return empties;
	}


	IEnumerator goToSleep(){
		yield return new WaitForSeconds (0.1f);
	}

	//returns the empty version of the spacedDevCombatCircle
	public mapNode[] getEmptySpacedDevCombatCircle(int stepsOut, int yourEnemyID, mapNode oldfinalDest, int offset) {
		if(oldfinalDest!=null)
			oldfinalDest.setEmpty ();
		mapNode[] spacedCombatCircle = getSpacedDevCombatCircle (stepsOut, offset);
		mapNode[] empties =  extractEmptyNodes (spacedCombatCircle, yourEnemyID); 
		if (oldfinalDest!=null)
			empties = removeFromList (oldfinalDest, empties);
		if (empties.Length == 0) {
//			StartCoroutine(goToSleep ());
//			return getEmptySpacedDevCombatCircle (stepsOut, yourEnemyID, oldfinalDest);
			return null;
		}
		return empties;
	}

	//returns a list of nodes with the maximum spacing in between the nodes to ensure
	//that each enemy gets its own space. The nodes may not be empty!
	public mapNode[] getSpacedDevCombatCircle(int stepsOut, int offset){
		mapNode[] combatCircle = calculateDevCombatCircle (stepsOut);
		GameObject[] enemies = GameObject.Find ("DevDrake").GetComponent<DevMovement> ().getEnemies ();
		int spacing = Mathf.FloorToInt(combatCircle.Length / enemies.Length);
		mapNode[] spacedCombatCircle = new mapNode[enemies.Length];
		for (int idx = 0; idx < enemies.Length-offset; ++idx) {
			spacedCombatCircle [idx] = combatCircle [(idx * spacing) + offset];
		}
		return spacedCombatCircle;
	}

	//returns the list of empty nodes of the devCombatCircle
	public mapNode[] getEmptyDevCombatCircle(int stepsOut, int yourEnemyID){
		mapNode[] combatCircle = calculateDevCombatCircle (stepsOut);
		return extractEmptyNodes (combatCircle, yourEnemyID); 
	}


//	public mapNode[] calculateDevCombatCircle2(int stepsOut){
//		int max = stepsOut;
//		int min = -stepsOut;
//		mapNode[] combatCircle = new mapNode[8 * stepsOut];
//
//		int longSide = 2 * stepsOut + 1;
//		int shortSide = 2 * stepsOut - 1;
//
//		for (int idx = 0; idx < longSide; ++idx) {
//			combatCircle [idx] = grid[zmax;
//		}
//
//	}

	//Calculates the box-shaped list of cells that are STEPSOUT steps 
	//out from dev's cell (only supported for 1, 2, 3, or 4 stepsOut) 
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
		case 4:
			{
				mapNode[] combatCircle = new mapNode[32];
				combatCircle [0] = grid [z+4] [x-4];
				combatCircle [1] = grid [z+4] [x-3];
				combatCircle [2] = grid [z+4] [x-2];
				combatCircle [3] = grid [z+4] [x-1];
				combatCircle [4] = grid [z+4] [x];
				combatCircle [5] = grid [z+4] [x+1];
				combatCircle [6] = grid [z+4] [x+2];
				combatCircle [7] = grid [z+4] [x+3];
				combatCircle [8] = grid [z+4] [x+4];
				combatCircle [9] = grid [z+3] [x+4];
				combatCircle [10] = grid [z+2] [x+4];
				combatCircle [11] = grid [z+1] [x+4];
				combatCircle [12] = grid [z] [x+4];
				combatCircle [13] = grid [z-1] [x+4];
				combatCircle [14] = grid [z-2] [x+4];
				combatCircle [15] = grid [z-3] [x+4];
				combatCircle [16] = grid [z-4] [x+4];
				combatCircle [17] = grid [z-4] [x+3];
				combatCircle [18] = grid [z-4] [x+2];
				combatCircle [19] = grid [z-4] [x+1];
				combatCircle [20] = grid [z-4] [x];
				combatCircle [21] = grid [z-4] [x-1];
				combatCircle [22] = grid [z-4] [x-2];
				combatCircle [23] = grid [z-4] [x-3];
				combatCircle [24] = grid [z-4] [x-4];
				combatCircle [25] = grid [z-3] [x-4];
				combatCircle [26] = grid [z-2] [x-4];
				combatCircle [27] = grid [z-1] [x-4];
				combatCircle [28] = grid [z] [x-4];
				combatCircle [29] = grid [z+1] [x-4];
				combatCircle [30] = grid [z+2] [x-4];
				combatCircle [31] = grid [z+3] [x-4];
				return combatCircle;
			}
		default:
			{
				Debug.LogAssertion("input 'stepsOut' needs to 1, 2, 3, or 4. You input " + stepsOut + " !");
				return null;
			}
		}
	}

	//finds a path from START to DEST with no 'empty' nodes in between
//	public Queue<mapNode> findPath(mapNode start, mapNode dest, int enemyID)
//	{
//		Queue<mapNode> path = new Queue<mapNode>();
//		KeyValuePair<int,int> startCoords = start.getIndices ();
//		KeyValuePair<int,int> destCoords = dest.getIndices ();
//
//		//reached destination (current node = destination node)
//		if (startCoords.Key == destCoords.Key && startCoords.Value == destCoords.Value) {
//			return path;
//		}
//
//		bool goRight = startCoords.Value < destCoords.Value;
//		bool goUp = startCoords.Key < destCoords.Key;
//
//		//z coords match
//		if (startCoords.Key == destCoords.Key) {
//
//			//go to the right
//			if (goRight) {
//				path.Enqueue (grid [startCoords.Key] [startCoords.Value + 1]);
//				Queue<mapNode> continuation = findPath (grid [startCoords.Key] [startCoords.Value + 1], dest, enemyID);
//				while (continuation.Count != 0) {
//					path.Enqueue (continuation.Dequeue());
//				}
//				return path;
//			}
//
//			//go to the left
//			else {
//				path.Enqueue (grid [startCoords.Key] [startCoords.Value - 1]);
//				Queue<mapNode> continuation = findPath (grid [startCoords.Key] [startCoords.Value - 1], dest, enemyID);
//				while (continuation.Count != 0) {
//					path.Enqueue (continuation.Dequeue());
//				}
//				return path;
//			}
//		} 
//
//		//x coords match
//		else if (startCoords.Value == destCoords.Value) {
//
//			//go up
//			if (goUp) {
//				path.Enqueue (grid [startCoords.Key + 1] [startCoords.Value]);
//				Queue<mapNode> continuation = findPath (grid [startCoords.Key + 1] [startCoords.Value], dest, enemyID);
//				while (continuation.Count != 0) {
//					path.Enqueue (continuation.Dequeue());
//				}
//				return path;
//			}
//
//			//go down
//			else {
//				path.Enqueue (grid [startCoords.Key - 1] [startCoords.Value]);
//				Queue<mapNode> continuation = findPath (grid [startCoords.Key - 1] [startCoords.Value], dest, enemyID);
//				while (continuation.Count != 0) {
//					path.Enqueue (continuation.Dequeue ());
//				}
//				return path;
//			}
//
//		} else {
//			if (goUp) {
//
//				//go diagonal up and right
//				if (goRight) {
//					mapNode curNode = grid [startCoords.Key + 1] [startCoords.Value + 1];
//					if (curNode.hasOtherOwner(enemyID)) {
//						if (!grid [startCoords.Key] [startCoords.Value + 1].hasOtherOwner(enemyID)) {
//							curNode = grid [startCoords.Key] [startCoords.Value + 1];
//						} else if (!grid [startCoords.Key + 1] [startCoords.Value].hasOtherOwner(enemyID)) {
//							curNode = grid [startCoords.Key + 1] [startCoords.Value];
//						}
//					} else {
//						curNode.setFull (enemyID);
//					}
//
//					path.Enqueue (curNode);
//					Queue<mapNode> continuation = findPath (curNode, dest, enemyID);
//					while (continuation.Count != 0) {
//						path.Enqueue (continuation.Dequeue ());
//					}
//					return path;
//				} 
//
//				//go diagonal up and left
//				else {
//					mapNode curNode = grid [startCoords.Key + 1] [startCoords.Value - 1];
//					if (curNode.hasOtherOwner(enemyID)) {
//						if (!grid [startCoords.Key] [startCoords.Value - 1].hasOtherOwner(enemyID)) {
//							curNode = grid [startCoords.Key] [startCoords.Value - 1];
//						} else if (!grid [startCoords.Key + 1] [startCoords.Value].hasOtherOwner(enemyID)) {
//							curNode = grid [startCoords.Key + 1] [startCoords.Value];
//						}
//					} else {
//						curNode.setFull (enemyID);
//					}
//
//					path.Enqueue (curNode);
//					Queue<mapNode> continuation = findPath (curNode, dest, enemyID);
//					while (continuation.Count != 0) {
//						path.Enqueue (continuation.Dequeue ());
//					}
//					return path;
//				}
//			} else {
//
//				//go diagonal down and right
//				if (goRight) {
//					mapNode curNode = grid [startCoords.Key - 1] [startCoords.Value + 1];
//					if (curNode.hasOtherOwner(enemyID)) {
//						if (!grid [startCoords.Key] [startCoords.Value + 1].hasOtherOwner(enemyID)) {
//							curNode = grid [startCoords.Key] [startCoords.Value + 1];
//						} else if (!grid [startCoords.Key - 1] [startCoords.Value].hasOtherOwner(enemyID)) {
//							curNode = grid [startCoords.Key - 1] [startCoords.Value];
//						}
//					} else {
//						curNode.setFull (enemyID);
//					}
//
//					path.Enqueue (curNode);
//					Queue<mapNode> continuation = findPath (curNode, dest, enemyID);
//					while (continuation.Count != 0) {
//						path.Enqueue (continuation.Dequeue ());
//					}
//					return path;
//				} 
//
//				//go diagonal down and left
//				else {
//					mapNode curNode = grid [startCoords.Key - 1] [startCoords.Value - 1];
//					if (curNode.hasOtherOwner(enemyID)) {
//						if (!grid [startCoords.Key] [startCoords.Value - 1].hasOtherOwner(enemyID)) {
//							curNode = grid [startCoords.Key] [startCoords.Value - 1];
//						} else if (!grid [startCoords.Key - 1] [startCoords.Value].hasOtherOwner(enemyID)) {
//							curNode = grid [startCoords.Key - 1] [startCoords.Value];
//						}
//					} else {
//						curNode.setFull (enemyID);
//					}
//
//					path.Enqueue (curNode);
//					Queue<mapNode> continuation = findPath (curNode, dest, enemyID);
//					while (continuation.Count != 0) {
//						path.Enqueue (continuation.Dequeue ());
//					}
//					return path;
//				}
//			}
//		}
//	}

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



