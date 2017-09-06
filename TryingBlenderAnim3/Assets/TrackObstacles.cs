using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackObstacles : MonoBehaviour {
	public void markTreesAsFull(){
		Terrain ter = GetComponent<Terrain> ();
		TerrainData data = ter.terrainData;
		TreeInstance[] allTrees = data.treeInstances;
		foreach(TreeInstance tree in allTrees){
			GetComponent<MapPathfind> ().containingCell (tree.position).setFull (-2);
		}
	}
}