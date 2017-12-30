using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevMain : MonoBehaviour {

	#region imports to set in the inspector
	public bool doCombat, doPathfinding;
	public GameObject terrain;
	#endregion

	#region script imports
	MouseMovement mouseMovement;

	DevMovement devMovement;
	DevCellTracking devCellTracking;
	DevCombat devCombat;
	DevCombatReactions devCombatReactions;

	EnemyMain[] enemyMains;

	TrackObstacles trackObstacles;
	MapPathfind mapPathfind;
	ClosestNodes closestNodes;
	#endregion

	void Start () {
		devMovement = GetComponent<DevMovement> ();
		mouseMovement = Camera.main.GetComponent<MouseMovement> ();
		enemyMains = GameObject.Find ("Enemies").GetComponentsInChildren<EnemyMain> ();
			
		devMovement.Init ();
		mouseMovement.Init ();

		foreach(EnemyMain curEnemyMain in enemyMains){
			if (doCombat)
				curEnemyMain.setCombat (true);
			if (doPathfinding)
				curEnemyMain.setPathfinding (true);
			curEnemyMain.Init ();
		}


		if (doCombat) {
			devCombat = GetComponent<DevCombat> ();
			devCombatReactions = GetComponent<DevCombatReactions> ();
			devCombat.Init ();
			devCombatReactions.Init ();
		}

		if (doPathfinding) {
			if (terrain == null) {
				Debug.LogAssertion ("Need to specify terrain in Dev Main Script Public Objects");
			}
			trackObstacles = terrain.GetComponent<TrackObstacles> ();
			mapPathfind = terrain.GetComponent<MapPathfind> ();
			closestNodes = terrain.GetComponent<ClosestNodes> ();
			devCellTracking = GetComponent<DevCellTracking> ();

			mapPathfind.Init ();
			closestNodes.Init ();
			trackObstacles.Init ();
			devCellTracking.Init ();


			foreach (EnemyMain curEnemyMain in enemyMains) {
				curEnemyMain.initCell ();
			}
			enemyMains [0].repathAll ();
		}
	}
	
	void Update () {		
		devMovement.FrameUpdate ();
		mouseMovement.FrameUpdate ();
		foreach(EnemyMain curEnemyMain in enemyMains){
			curEnemyMain.FrameUpdate ();
		}

		if (doCombat) {
			devCombat.FrameUpdate ();
			devCombatReactions.FrameUpdate ();
		}

		if (doPathfinding) {
			devCellTracking.FrameUpdate ();
		}

	}
}
