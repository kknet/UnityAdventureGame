using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

	#region imports
	public int enemyID;
	public mapNode finalDest;
	public bool doneStarting;
	public bool inPosition = false;
	public GameObject terrain;
	public mapNode start;
	public mapNode nextDest = null;
	public Queue<mapNode> path = null;
	public bool inPathGen = false;
	public float pathGenTime = 0f;
	public float moveTime = 0f;
	public bool moving = false;
	public bool gotPath = false;

	private float nextDestWaitTime = 0f;
	private float finalDestWaitTime = 0f;
	private Animator enemyAnim;
	private GameObject Dev;
	private float rotSpeed;
	private float moveSpeed;
	private Vector3 dif;
	private Vector3 oldDev;
	private float restStartTime;
	private bool resting;
	private mapNode oldDevCell;
	private GameObject[] enemies;
	private bool allReady = false;
	#endregion

	#region major methods

	public void Init () {
		doneStarting = false;
		terrain = GameObject.Find ("Terrain");
		inPosition = false;
		enemyAnim = GetComponent<Animator> ();
		Dev = GameObject.Find ("DevDrake");
		rotSpeed = 12f;
		moveSpeed = 4f;

		resting = false;
		restStartTime = Time.time;
		enemies = GameObject.FindGameObjectsWithTag("Enemy");
	}

	public void initCell(){
		start = terrain.GetComponent<MapPathfind> ().containingCell (transform.position);
	}

	public void FrameUpdate () {
		checkIfAllReady ();
		updateYourCell ();
		moveToDev ();
	}

	private bool checkIfAllReady(){
		if (allReady)
			return true;
		foreach(GameObject enemy in enemies){
			if(!enemy.GetComponent<EnemyAI>().doneStarting)
				return false;
		}
		allReady = true;
		return true;
	}

	//keep track of this agent's current location
	public void updateYourCell() {
		mapNode oldStart = start;
		start = terrain.GetComponent<MapPathfind> ().containingCell (transform.position);
		if (!oldStart.equalTo (start)) {
			oldStart.setEmpty ();
		}
		start.setFull (enemyID);
	}


	public void moveToDev() {

		if (hasGoalNode() && (path == null || nextDest == null)) {
			if (Time.realtimeSinceStartup >= pathGenTime) {
				terrain.GetComponent<ClosestNodes> ().markAllPositionedEnemies ();
				gotPath = setNewPath ();
				if (!gotPath) {
					terrain.GetComponent<ClosestNodes> ().assignTimes ();
					return;
				}
			} else {
				stop ();
				return;
			}
		} else if (!hasGoalNode()) {
			/*ONLY TIME THAT WE REPATH ALL*/
			repathAll ();
			return;
		}


		if (nextSpotIsFull()) {
			stop ();
			inPosition = false;
			moving = false;
			if (nextDestWaitTime == 0f)
				nextDestWaitTime = Time.realtimeSinceStartup;
			else if (Time.realtimeSinceStartup - nextDestWaitTime >= 1f) {
				gotPath = false;
				terrain.GetComponent<ClosestNodes> ().assignTimes ();
				nextDestWaitTime = 0f;
				nextDest = null;
			}
			return;
		} else if(nextDest!=null && nextDestWaitTime > 0f){
			nextDestWaitTime = 0f;
		}

		if (goalNodeIsFull()) {
			if (finalDestWaitTime == 0f)
				finalDestWaitTime = Time.realtimeSinceStartup;
			else if (Time.realtimeSinceStartup - finalDestWaitTime >= 3f) {
				gotPath = false;
				finalDestWaitTime = 0f;
				mapNode[] options = terrain.GetComponent<MapPathfind> ().getEmptySpacedDevCombatCircle (3, enemyID, finalDest, 0);
				finalDest = terrain.GetComponent<MapPathfind> ().findClosestNode (options, start);
				path = null;
				terrain.GetComponent<ClosestNodes> ().assignTimes ();
				moving = false;
				inPosition = false;
				return;
			}
		} 

		if (gotPath && (start.equalTo (finalDest) || nextDest == null)) {
			inPosition = true;
			moving = false;
			stop ();
			rotateToTarget (Dev.transform.position);
			return;
		}

		//reached intermediate destination
		if (path!= null && (nextDest == null || start.equalTo(nextDest))) {
			nextDest = path.Dequeue ();

			if (nextDest.hasOtherOwner (enemyID)) {
				stop ();
				moving = false;
				return;
			}
			if (nextDest == null || nextDest.getCenter () == null) {
				Debug.LogAssertion ("nextDest is messed up");
				return;
			}
		}
		//rotate towards nextDest
		rotateToTarget(nextDest.getCenter());

		//move towards nextDest
		moveToTarget();
		moving = true;
	}
		
	#endregion

	#region helpers

	void moveToTarget(){
		if (finalDest == null || start.equalTo(finalDest)) {
			stop ();
		} else {

			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 1f, 5f * Time.deltaTime));
			transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
		}
	}

	void rotateToTarget(Vector3 targ){
		dif = targ - transform.position;
		dif.x = clampAngle (dif.x);
		dif.z = clampAngle (dif.z);
		dif = new Vector3 (dif.x, 0f, dif.z);
		transform.forward = Vector3.RotateTowards (transform.forward, dif, rotSpeed * Time.deltaTime, 0.0f); 
	}


	public mapNode getDevCell(){
		mapNode ret = terrain.GetComponent<MapPathfind> ().devCell;
		if (ret == null) {
			Debug.LogAssertion ("Why doesn't it already have the value of dev cell?");
			ret = terrain.GetComponent<MapPathfind> ().devCell;
		}
		return ret;
	}

	public bool canPathGen(){
		inPathGen = false;
		foreach (GameObject enemy in enemies) {
			if (enemy.GetComponent<EnemyAI>().inPathGen){
					return false;
			}
		}
		return true;
	}

	public void cleanOldPath(){
		updateYourCell ();
		while (path !=null && path.Count > 0) {
			mapNode trashNode = path.Dequeue ();
			trashNode.setEmpty ();
		}
		inPosition = false;
		finalDest = null;
		nextDest = null;
		path = null;
	}

	public bool setNewPath(){
		if (!canPathGen ()) {//only one enemy can generate a path at a time, to reduce lag in game
			inPathGen = false;
			return false;
		}

		inPathGen = true;
		mapNode goal = GetComponent<AStarMovement> ().shortestPath (start, finalDest);
		if (goal == null || !goal.equalTo (finalDest)) {//means that the path gen failed!
			inPathGen = false;
			return false;
		}
		path = GetComponent<AStarMovement> ().traceBackFromGoal(start, finalDest);
		inPathGen = false;

		if (path == null) {
			return false;
		}
		if (path.Count == 0) {
			path = null;
			return false;
		}
		nextDest = path.Dequeue ();
		return true;
	}

	public void repathAll(){
		terrain.GetComponent<ClosestNodes> ().regenPathsLongQuick ();
	}

	private bool hasGoalNode(){
		return finalDest != null;
	}

	private bool nextSpotIsFull(){
		return nextDest != null && nextDest.hasOtherOwner (enemyID);
	}

	private bool goalNodeIsFull(){
		return finalDest != null && finalDest.hasOtherOwner (enemyID);
	}

	void stop(){
		enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards (enemyAnim.GetFloat ("enemySpeed"), 0f, 2f * Time.deltaTime));
	}

	//	public bool isEnemyAttacking() {
	//		AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo (0);
	//		return info.IsName ("QUICK1") || info.IsName ("QUICK2") || info.IsName ("QUICK3") || info.IsName ("QUICK4") || info.IsName ("QUICK5");
	//	}

	#endregion

	#region util

	float clampAngle(float orig){
		while (orig > 180f)
			orig -= 360f;
		while (orig < -180f)
			orig += 360f;
		return orig;
	}

	private float rand(float a, float b){
		return UnityEngine.Random.Range (a, b);
	}

	private bool Approx(Vector3 a, Vector3 b){
		return Mathf.Approximately (a.x, b.x) && Mathf.Approximately (a.y, b.y) && Mathf.Approximately (a.z, b.z);
	}

	#endregion
}