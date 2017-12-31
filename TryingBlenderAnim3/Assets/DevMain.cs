using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevMain : MonoBehaviour {

	#region imports to set in the inspector
	public bool doCombat, doPathfinding, doEnemies;
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
		if (doPathfinding || doCombat)
			doEnemies = true;
		devMovement = GetComponent<DevMovement> ();
		mouseMovement = Camera.main.GetComponent<MouseMovement> ();

		devMovement.doCombat = doCombat;
		devMovement.Init ();
		mouseMovement.Init ();

		if (doEnemies) {
			enemyMains = GameObject.Find ("Enemies").GetComponentsInChildren<EnemyMain> ();
			foreach(EnemyMain curEnemyMain in enemyMains){
				if (doCombat)
					curEnemyMain.setCombat (true);
				if (doPathfinding)
					curEnemyMain.setPathfinding (true);
				curEnemyMain.Init ();
			}
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

		if (doEnemies) {
			foreach(EnemyMain curEnemyMain in enemyMains){
				curEnemyMain.FrameUpdate ();
			}
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
