using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestNodes : MonoBehaviour {

	GameObject terrain;
	GameObject Dev;

	public bool makingNewPaths = false;
	public bool doneStarting = false;

	public void Start(){
		terrain = GameObject.Find ("Terrain");
		Dev = GameObject.Find ("DevDrake");
		doneStarting = true;
	}

	public GameObject[] enemiesClosestToDev() {
		GameObject[] enemies = getEnemies ();
		List<KeyValuePair<GameObject, float>> enemiesByDistance = new List<KeyValuePair<GameObject, float>> (enemies.Length);
		int idx = 0;
		for(; idx < enemies.Length; ++idx) {
			GameObject enemy = enemies [idx];
			float distance = GetComponent<MapPathfind> ().devCell.distance (enemy.GetComponent<EnemyAI> ().start);
			enemiesByDistance[idx] = new KeyValuePair<GameObject, float> (enemy, distance);
		}

		enemyDistComparer comparer = new enemyDistComparer ();

		//sort enemies by distance to dev
		enemiesByDistance.Sort(comparer);
		idx = 0;
		foreach (KeyValuePair<GameObject, float> pair in enemiesByDistance) {
			enemies[idx++] = pair.Key;
		}
		return enemies;
	}

	public void assignTimes(){
		GameObject[] enemies = getEnemies ();
		int increment = 0;
		for(int idx = 0; idx < enemies.Length; ++idx){ 
			GameObject enemy = enemies [idx];
			if (enemy.GetComponent<EnemyAI> ().gotPath)
				continue;
			enemy.GetComponent<EnemyAI> ().pathGenTime = Time.realtimeSinceStartup + (0.5f * increment);
			enemy.GetComponent<EnemyAI> ().moveTime = Time.realtimeSinceStartup + (0.5f * increment);
			++increment;
		}
	}

	public GameObject[] getEnemies(){
		return GameObject.FindGameObjectsWithTag ("Enemy");
	}

	private float rand(float a, float b){
		return UnityEngine.Random.Range (a, b);
	}

	private void clearAllNodes () {
		int zMax = terrain.GetComponent<MapPathfind> ().grid.Length;
		int xMax = terrain.GetComponent<MapPathfind> ().grid[0].Length;
		for (int z = 0; z < zMax; ++z) {
			for (int x = 0; x < xMax; ++x) {
				terrain.GetComponent<MapPathfind> ().grid [z] [x].setEmpty ();
			}
		}
	}

//	public void regenPaths(GameObject[] enemies){
//		foreach (GameObject enemy in enemies) {
//			terrain.GetComponent<MapPathfind> ().containingCell (enemy.transform.position).setFull(enemy.GetComponent<EnemyAI>().enemyID);
//		}
//		foreach (GameObject enemy in enemies) {
//			enemy.GetComponent<EnemyAI> ().plotNewPath ();
//		}
//		//		foreach (GameObject enemy in enemies) {
//		//			enemy.GetComponent<EnemyAI> ().moveToDev();
//		//		}
//		makingNewPaths = false;
//	}
//
//	public void regenClosestPathsShort(){
//		//SHORTER, MORE EFFICIENT, MORE ESTIMATED APPROACH ---//
//
//		//calculate distances to dev for each enemy
//		GameObject[] enemies = getEnemies ();
//		float[] distToDev = new float[enemies.Length];
//		for(int idx = 0; idx < enemies.Length; ++idx) {
//			distToDev [idx] = Vector3.Distance (enemies [idx].transform.position, transform.position);
//		}
//
//		//sort enemies by distance to dev
//		enemies = sortByDistance(enemies, distToDev);
//
//		//for each enemy, get closest empty surrounding neighbor
//		regenPaths(enemies);
//	}

	private GameObject[] sortByDistance (GameObject[] obj, float[] dist) {
		int exploredIdx = 0;
		int minIdx = 0;
		float min = dist [0];
		while (exploredIdx != dist.Length) {
			for (int idx = exploredIdx; idx < dist.Length; ++idx) {
				if (dist [idx] < min) {
					minIdx = idx;
					min = dist [idx];
				}
			}
			float temp = dist [exploredIdx];
			dist [exploredIdx] = dist [minIdx];
			dist [minIdx] = temp;

			GameObject tempObj = obj [exploredIdx];
			obj [exploredIdx] = obj [minIdx];
			obj [minIdx] = tempObj;

			++exploredIdx;
		}
		return obj;
	}

	//for each enemy calculate distance to each surroundingNeighbor
	//return a list containing the list for each enemy
	private List<nodeList> calculateEnemyDistances() {
		List<nodeList> enemyNodeLists = new List<nodeList> ();
		mapNode[] neighborCircle = terrain.GetComponent<MapPathfind> ().getSpacedDevCombatCircle (2, 0);
		SortedList<int, GameObject> enemies = terrain.GetComponent<MapPathfind> ().enemies;
		foreach (KeyValuePair<int, GameObject> pair in enemies) {
			GameObject enemy = pair.Value;
			nodeList list = new nodeList (enemy.GetComponent<EnemyAI>().enemyID);
			foreach (mapNode neighbor in neighborCircle) {
				mapNode enemyNode = enemy.GetComponent<EnemyAI> ().start;
				list.Add(neighbor, enemyNode.distance(neighbor));
			}
			enemyNodeLists.Add (list);
		}
		return enemyNodeLists;
	}

	private KeyValuePair<mapNode, float> findClosestNode(nodeList nodePairs){
		List<KeyValuePair<mapNode, float>> list = nodePairs.getList ();
		KeyValuePair<mapNode, float> min = list [0];
		for (int idx = 1; idx < list.Count; ++idx) {
			KeyValuePair<mapNode, float> thisPair = list [idx];
			if (thisPair.Value < min.Value)
				min = thisPair;
		}
		return min;
	}

	//sort mapNodes in accordance to their corresponding float values, using a sortedlist
	private nodeList sortNodesByDistance(nodeList unsorted) {
		SortedList<float, mapNode> sorter = new SortedList<float, mapNode> ();
		foreach(KeyValuePair<mapNode, float> pair in unsorted.getList()){
			sorter.Add (pair.Value, pair.Key);
		}
		unsorted.clearList ();
		foreach (KeyValuePair<float, mapNode> pair in sorter) {
			unsorted.Add (pair.Value, pair.Key);
		}
		return unsorted;
	}

	//Sort enemies by the distance to their closest node
	//the enemies that are closest to their respective nodes get to go first
	//return a list of enemyID & mapNode pairs, representing which enemy goes to which grid node
	private List<KeyValuePair<int, mapNode>> assignClosestNeighbors (List<nodeList> enemyNodeLists){
		List<KeyValuePair<int, mapNode>> finalDests = new List<KeyValuePair<int, mapNode>> ();
		int numEnemies = enemyNodeLists.Count;
		nodeListComparer comparer = new nodeListComparer ();
		while (finalDests.Count < numEnemies) {

			//sort remaining enemies by their distance to their closest node
//			SortedList<float, nodeList> sorted = new SortedList<float, nodeList> ();
//			foreach (nodeList list in enemyNodeLists) {
//				sorted.Add (list.at (0).Value, list);
//			}
			enemyNodeLists.Sort(comparer);
			//the enemy at sorted[0] is the enemy closest to its closest node
			//assign this enemy a finalDest
			//and remove this enemy from enemyNodeLists since it has been assigned a finalDest
			mapNode thisNode = enemyNodeLists[0].at(0).Key;
			finalDests.Add(new KeyValuePair<int, mapNode>(enemyNodeLists[0].getID(), thisNode));
			enemyNodeLists.RemoveAt(0);

			//also remove all occurrences of the finalDest node which was just assigned
			//from lists in enemyNodeLists, so that no other enemy can be assigned this node
			foreach (nodeList nodelist in enemyNodeLists) {
				List<KeyValuePair<mapNode, float>> list = nodelist.getList (); 
				for(int idx = 0; idx < list.Count; ++idx) {
					KeyValuePair<mapNode, float> pair = list [idx];
					if(pair.Key.equalTo(thisNode)) {
						nodelist.remove (pair);
					}
				}
			}
		}

		return finalDests;
	}


	public void regenClosestPathsLong () {
		if (makingNewPaths)
			return;
		
		GameObject[] enemies = getEnemies ();
		foreach (GameObject enemy in enemies) {
			enemy.GetComponent<EnemyAI> ().start.setEmpty ();
			enemy.GetComponent<EnemyAI> ().cleanOldPath ();
		}

		//for every single enemy: figure out the distance from the enemy to each surroundingNeighbor
		List<nodeList> enemyNodeLists = calculateEnemyDistances();
		nodeDistComparer comparer = new nodeDistComparer ();
		for (int idx = 0; idx < enemyNodeLists.Count; ++idx) {
			enemyNodeLists [idx].sort ();
		}
			
		//assign neighbors to enemies, prioritizing enemies that are closest to their chosen neighbors
		List<KeyValuePair<int, mapNode>> finalDests = assignClosestNeighbors (enemyNodeLists);

		foreach (KeyValuePair<int, mapNode> enemyNodePair in finalDests) {
			GameObject thisEnemy = terrain.GetComponent<MapPathfind> ().getEnemyByID (enemyNodePair.Key);
			thisEnemy.GetComponent<EnemyAI> ().finalDest = enemyNodePair.Value;
			thisEnemy.GetComponent<EnemyAI> ().setNewPath ();
		}
	}

	public void regenPathsLongQuick()
	{
		Debug.Log ("got before!");
		if (makingNewPaths)
			return;

		Debug.Log ("got past!");

		makingNewPaths = true;

		List<KeyValuePair<GameObject, mapNode>> enemyDests = new List<KeyValuePair<GameObject, mapNode>> ();
		List<mapNode> neighborCircle = new List<mapNode>(terrain.GetComponent<MapPathfind> ().getSpacedDevCombatCircle (3, 0));
		List<GameObject> enemies = new List<GameObject>(terrain.GetComponent<MapPathfind> ().enemies.Values);

//		foreach (GameObject enemy in enemies) {
//		}

		while (neighborCircle.Count > 0) {
			for (int enemyIdx = 0; enemyIdx < enemies.Count; ++enemyIdx) {
				mapNode chosenNode = null;
				float minDist = float.MaxValue;
				GameObject thisEnemy = enemies [enemyIdx];
				for (int nodeIdx = 0; nodeIdx < neighborCircle.Count; ++nodeIdx) {
					mapNode thisNode = neighborCircle [nodeIdx];
					float thisDist = thisEnemy.GetComponent<EnemyAI> ().start.distance (thisNode);
					if (thisDist < minDist) {
						minDist = thisDist;
						chosenNode = thisNode;
					}
				}
				enemyDests.Add (new KeyValuePair<GameObject, mapNode> (thisEnemy, chosenNode));
				enemies.Remove (thisEnemy);
				neighborCircle.Remove (chosenNode);
			}
		}

		for(int idx = 0; idx < enemyDests.Count; ++idx){
			KeyValuePair<GameObject, mapNode> pair = enemyDests [idx];
			assignDest (pair.Key, pair.Value, idx);
		}

		makingNewPaths = false;
	}

	public void assignDest(GameObject enemy, mapNode dest, float timeIncrement){
		
		enemy.GetComponent<EnemyAI> ().cleanOldPath ();
		enemy.GetComponent<EnemyAI> ().finalDest = dest;
		enemy.GetComponent<EnemyAI> ().pathGenTime = Time.realtimeSinceStartup + (0.5f * timeIncrement);
		enemy.GetComponent<EnemyAI> ().moveTime = Time.realtimeSinceStartup + (0.5f * timeIncrement);

	}



}




public class nodeList {
	int enemyID;
	List<KeyValuePair<mapNode, float>> distList;

	public nodeList (int _enemyID, List<KeyValuePair<mapNode, float>> _distList){
		enemyID = _enemyID;
		distList = _distList;
	}

	public nodeList (int _enemyID){
		enemyID = _enemyID;
		distList = new List<KeyValuePair<mapNode, float>>();
	}

	public void Add (mapNode node, float dist){
		distList.Add (new KeyValuePair<mapNode, float> (node, dist));
	}

	public void sort(){
		nodeDistComparer comparer = new nodeDistComparer ();
		distList.Sort (comparer);
	}

	public List<KeyValuePair<mapNode, float>> getList(){
		return distList;
	}

	public void clearList(){
		distList.Clear ();
	}

	public KeyValuePair<mapNode, float> at(int idx){
		return distList[idx];
	}

	public void setList(List<KeyValuePair<mapNode, float>> _distList){
		distList = _distList;
	}

	public int getID(){
		return enemyID;
	}

	public void remove(KeyValuePair<mapNode, float> pair){
		distList.Remove (pair);
	}
}

public class nodeDistComparer: IComparer<KeyValuePair<mapNode, float>>{
	public int Compare(KeyValuePair<mapNode, float> a, KeyValuePair<mapNode, float> b){
		if (a.Value > b.Value)
			return 1;
		if (a.Value < b.Value)
			return -1;
		return 0;
	} 
}

public class nodeListComparer: IComparer<nodeList>
{
	public int Compare(nodeList a, nodeList b){
		if (a.at (0).Value > b.at (0).Value)
			return 1;
		if (a.at (0).Value < b.at (0).Value)
			return -1;
		return 0;
	}
}

public class enemyDistComparer: IComparer<KeyValuePair<GameObject, float>>{
	public int Compare(KeyValuePair<GameObject, float> a, KeyValuePair<GameObject, float> b){
		if (a.Value > b.Value)
			return 1;
		if (a.Value < b.Value)
			return -1;
		return 0;
	}
}