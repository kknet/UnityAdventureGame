﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;


//The AStar algorithm finds the shortest path on the basis of finding
//nodes with a minimal f cost, where the f cost = g cost + h cost
//See mapNode.cs for descriptions of g cost & h cost.
//Unlike Dijkstra's Algorithm, AStar does NOT attempt to find the shortest
//path to every node in the graph!
public class AStarMovement : MonoBehaviour {
	 
	int numNodesPerSide;
	int enemyID;
	IDictionary<mapNode, mapNode> nodeParents;
	public IList<mapNode> path;
	GameObject Dev;
	GameObject terrain;
	public bool doneStarting = false;

	public void Init(){
		terrain = GameObject.Find ("Terrain");
		numNodesPerSide = terrain.GetComponent<MapPathfind> ().nodesPerSide;
		enemyID = GetComponent<EnemyAI> ().enemyID;
		nodeParents = new Dictionary<mapNode, mapNode>();
		Dev = GameObject.Find ("DevDrake");
		doneStarting = true;
	}

	public Queue<mapNode> traceBackFromGoal(mapNode start, mapNode goal) {
		path = new List<mapNode> ();
		mapNode curNode = goal;
		while (!curNode.equalTo(start)) {
			path.Add (curNode);
			curNode = nodeParents [curNode];
			if (curNode.hasOtherOwner (enemyID))
				Debug.Log ("Doesn't work!");
			curNode.setFull (enemyID);
		}
		return new Queue<mapNode> (path);
	}

	private List<mapNode> getWalkableNodes() {
		terrain.GetComponent<MapPathfind> ().markSpots ();

		List<mapNode> walkableNodes = new List<mapNode> ();
		if (terrain == null) {
			Debug.LogAssertion ("Messed up");
//			Init ();
		}
//		else if (terrain.GetComponent<MapPathfind> () == null)
//			Debug.LogError ("mapPathfind is null");
//		else if (terrain.GetComponent<MapPathfind>().grid == null)
//			Debug.LogError ("grid is null");
//		else
//			Debug.LogError ("NOTHING WRONG");
		int zMax = terrain.GetComponent<MapPathfind> ().grid.Length;	
		int xMax = terrain.GetComponent<MapPathfind> ().grid[0].Length;
//		int zMax = numNodesPerSide;
//		int xMax = numNodesPerSide;
		for (int z = 0; z < zMax; ++z) {
			for (int x = 0; x < xMax; ++x) {
				mapNode curNode = terrain.GetComponent<MapPathfind> ().grid [z] [x];
				if (!curNode.hasOtherOwner(enemyID)) {
					walkableNodes.Add (curNode);
				}
			}
		}
		return walkableNodes;
	}

	public mapNode shortestPath (mapNode start, mapNode goal) {
		//list of nodes that are walkable (nodes that are empty or have this owner)
		List<mapNode> walkables = getWalkableNodes();

		//for storing scores of mapNodes
		IDictionary<mapNode, int> gScores = new Dictionary<mapNode, int> ();//one for g(x)
		IDictionary<mapNode, int> fScores = new Dictionary<mapNode, int> ();//one for h(x)

		//initialize dictionary entries by setting g(x) and f(x) to INFINITY for all walkable nodes
		foreach(mapNode node in walkables) {
			gScores.Add(new KeyValuePair<mapNode, int>(node, int.MaxValue));
			fScores.Add(new KeyValuePair<mapNode, int>(node, int.MaxValue));
		}

		//initialize the g and f scores for start
		gScores [start] = 0; //the distance from start to start is 0, obviously
		fScores [start] = 0 + heuristicWeight(start, goal); //the estimated distance from start to goal

		//set of exploredNodes
		HashSet<mapNode> exploredNodes = new HashSet<mapNode>();

		//create priority queue to keep track of nodes and f-costs
		//initialize with start node
		SimplePriorityQueue<mapNode, int> nodeQueue = new SimplePriorityQueue<mapNode, int>();
		nodeQueue.Enqueue (start, fScores [start]);

		while (nodeQueue.Count > 0) {
			//get next node with lowest f-cost
			mapNode curNode = nodeQueue.Dequeue ();

			//arrived at goal node, so stop 
			if (curNode.equalTo (goal)) {
				return goal;
			}

			exploredNodes.Add (curNode);

			List<mapNode> neighbors = new List<mapNode>(curNode.getNeighbors ());
//			neighbors = terrain.GetComponent<MapPathfind> ().extractEmptyNodes (neighbors, enemyID);
			foreach (mapNode node in neighbors) {

				if (!fScores.ContainsKey (node))
					continue;

				if (node.hasOtherOwner (enemyID))
					continue;

				if (exploredNodes.Contains (node)) //already looked over this node, don't look over it again
					continue;

				//add the total weight/distance so far to the weight of this neighbor
				int curScore = gScores [curNode] + node.Weight ();

				//we haven't explored this node, so add it to the queue
				if (!nodeQueue.Contains (node)){
					if (!fScores.ContainsKey (node))
						Debug.LogAssertion ("fscores doesn't have node as KEY!");
					else
						nodeQueue.Enqueue (node, fScores [node]);
				}

				//its current score is greater than the shortest path to it
				//we have already calculated, so don't consider it
				else if (curScore >= gScores [node])
					continue;

				//on the other hand, if the score is lower, record these scores as the current best for this path
				//and record the node's parent
				nodeParents [node] = curNode;
				gScores [node] = curScore;
				fScores [node] = gScores [node] + heuristicWeight (node, goal);
			}
		}

		return start;
	}

	int heuristicWeight(mapNode curNode, mapNode goalNode) {
		KeyValuePair<int, int> curIndices = curNode.getIndices ();
		KeyValuePair<int, int> goalIndices = goalNode.getIndices ();
		return Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow (curIndices.Key - goalIndices.Key, 2f) + Mathf.Pow (curIndices.Value - goalIndices.Value, 2f)));
	}


}
