using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCellTracking : MonoBehaviour {

	// Use this for initialization

	private GameObject dev, terrain;
	private mapNode lastRegenNode;
	MapPathfind mapPathfind;
	ClosestNodes closestNodes;


	public void Init () {
		dev = GameObject.Find ("DevDrake");
		terrain = GameObject.Find ("Terrain");
		mapPathfind = terrain.GetComponent<MapPathfind> ();
		closestNodes = terrain.GetComponent<ClosestNodes> ();
		mapPathfind.devCell = mapPathfind.containingCell (dev.transform.position);
		markNeighbors ();
	}
	
	public void FrameUpdate () {
		setDevCell ();
	}

	#region methods
	private mapNode getDevCell(){
		return mapPathfind.devCell;
	}

	private void markNeighbors(){
		mapNode[] neighbors = mapPathfind.devCell.getNeighbors ();
		foreach (mapNode node in neighbors) {
			node.setFull (-3);		
		}
	}

	private void clearNeighbors(){
		mapNode[] neighbors = mapPathfind.devCell.getNeighbors ();
		foreach (mapNode node in neighbors)
			node.setEmpty ();
	}


	public void setDevCellNoRepath(){
		//dev's current location
		mapNode newDevCell = mapPathfind.containingCell (transform.position);
		if (newDevCell == null) {
			Debug.LogAssertion ("bad");
		}
		else if (!newDevCell.equalTo (mapPathfind.devCell)) {

			clearNeighbors ();
			mapPathfind.devCell.setEmpty ();
			mapPathfind.devCell = newDevCell;
			mapPathfind.devCell.setFull (-3);
			markNeighbors ();
		}
	}

	public void setDevCell() {
		//dev's current location
		mapNode newDevCell = mapPathfind.containingCell (transform.position);
		if (newDevCell == null) {
			Debug.LogAssertion ("bad");
		}
		//			if (GameObject.Find ("Enemy") != null && !newDevCell.equalTo (mapPathfind.devCell)) {
		else if (!newDevCell.equalTo (mapPathfind.devCell)) {

			mapPathfind.devCell.setEmpty ();
			mapPathfind.devCell = newDevCell;
			mapPathfind.devCell.setFull (-3);
			if (lastRegenNode == null || lastRegenNode.distance (newDevCell) >= 3f) {
				closestNodes.regenPathsLongQuick();
				lastRegenNode = newDevCell;
			}
		}
	}
	#endregion
}
