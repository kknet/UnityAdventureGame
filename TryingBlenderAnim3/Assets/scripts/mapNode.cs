using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public mapNode(){
	}

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

	//	public void setSurroundingSpots(){
	//		GameObject.Find ("Terrain").GetComponent<MapPathfind> ().devSurroundingSpots = getSurroundingSpots ();
	//	}
	//	//get the spots around Dev's neighbors where enemies can gather and wait their turn to attack
	//	public mapNode[] getSurroundingSpots(){
	//
	//		//get the neighbors of the neighbors
	//		int count = 0;
	//		List<mapNode> outerCircle = new List<mapNode>();
	//		foreach (mapNode neighbor in neighbors) {
	//			mapNode[] neighborsSquared = neighbor.getNeighbors ();
	//			outerCircle.AddRange(neighborsSquared);
	//			count += neighborsSquared.Length;
	//		}
	//
	//		//remove all of the direct neighbors of Dev from outerCircle
	//		//so that we are only left with the indirect neighbors
	//		bool directFound = false;
	//		do{
	//			directFound = false;
	//			foreach(mapNode directNeighbor in neighbors){
	//				int idx = outerCircle.IndexOf (directNeighbor);
	//				if (idx >= 0) {
	//					outerCircle.RemoveAt(idx);
	//					idx = -1;
	//					--count;
	//					directFound = true;
	//				}
	//			}
	//		} while(directFound == true);
	//
	//		//get the neighbors of the outercircle
	//		count = 0;
	//		List<mapNode> outerCircleSquared = new List<mapNode>();
	//		foreach (mapNode neighbor in outerCircle) {
	//			mapNode[] neighborsSquared = neighbor.getNeighbors ();
	//			outerCircleSquared.AddRange(neighborsSquared);
	//			count += neighborsSquared.Length;
	//		}
	//
	//		//remove all of the outerCircle from the outerCircleSquared
	//		//so that we are only left with the indirect neighbors of a degree of 2
	//		do{
	//			directFound = false;
	//			foreach(mapNode directNeighbor in outerCircle){
	//				int idx = outerCircleSquared.IndexOf (directNeighbor);
	//				if (idx >= 0) {
	//					outerCircleSquared.RemoveAt(idx);
	//					idx = -1;
	//					--count;
	//					directFound = true;
	//				}
	//			}
	//		} while(directFound == true);
	//		return outerCircleSquared.ToArray ();
	//	}
	//	public mapNode getEmptySurroundingSpot(int yourEnemyID){
	//		if (!GameObject.Find("Terrain").GetComponent<MapPathfind>().doneBuilding) {
	//			GameObject.Find ("Terrain").GetComponent<MapPathfind> ().Start ();
	//		}
	//		mapNode[] outerCircle = GameObject.Find ("Terrain").GetComponent<MapPathfind> ().devSurroundingSpots;
	//		foreach (mapNode node in outerCircle) {
	//			if (!node.hasOtherOwner (yourEnemyID))
	//				return node;
	//		}
	//		return null;
	//	}
	//

}

