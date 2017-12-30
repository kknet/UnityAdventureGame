using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMain : MonoBehaviour {

	private bool doCombat = false, doPathfinding = false;

	EnemyAI enemyAI;
	ManageHealth manageHealth;
	AStarMovement aStarMovement;
	EnemyCombatAI enemyCombatAI;
	EnemyCombatReactions enemyCombatReactions;

	// Use this for initialization
	public void Init () {
		enemyAI = GetComponent<EnemyAI> ();
		manageHealth = GetComponent<ManageHealth> ();
		aStarMovement = GetComponent<AStarMovement> ();
		enemyCombatAI = GetComponent<EnemyCombatAI> ();
		enemyCombatReactions = GetComponent<EnemyCombatReactions> ();

		enemyAI.Init ();
		aStarMovement.Init ();
		enemyCombatAI.Init ();
		enemyCombatReactions.Init ();
	}
	
	// Update is called once per frame
	public void FrameUpdate () {

		if (doPathfinding) {
			enemyAI.FrameUpdate ();
		}

		if (doCombat) {
			enemyCombatAI.FrameUpdate ();
			enemyCombatReactions.FrameUpdate();
		}
	}

	public void setCombat(bool _doCombat){
		doCombat = _doCombat;
	}

	public void setPathfinding(bool _doPathfinding){
		doPathfinding = _doPathfinding;
	}

}
