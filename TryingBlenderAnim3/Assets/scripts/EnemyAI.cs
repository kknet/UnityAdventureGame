using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

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
	public bool gotPath = false;

	private float nextDestWaitTime = 0f;
	private float finalDestWaitTime = 0f;
	private Animator enemyAnim;
	private GameObject Dev;
	private float rotSpeed;
	private float moveSpeed;
	private Vector3 dif;
	private Vector3 oldDev;
	//	private Vector3 target;
	//	private Vector3 devTarget;
	private float restStartTime;
	private bool resting;
	private mapNode oldDevCell;
	private GameObject[] enemies;
	private bool allReady = false;
	public bool moving = false;

	public bool doPathfinding;

	// Use this for initialization
	public void Init () {
		doneStarting = false;
		terrain = GameObject.Find ("Terrain");
		inPosition = false;
		enemyAnim = GetComponent<Animator> ();
		Dev = GameObject.Find ("DevDrake");
		rotSpeed = 12f;
		moveSpeed = 4f;

		start = terrain.GetComponent<MapPathfind> ().containingCell (transform.position);
		resting = false;
		restStartTime = Time.time;
		enemies = GameObject.FindGameObjectsWithTag("Enemy");
//		bool allDone = true;
//		foreach(GameObject enemy in enemies){
//			if(!enemy.Equals(this)){
//				if(!enemy.GetComponent<EnemyAI>().doneStarting){
//					allDone = false;
//					break;
//				}
//			}
//		}
//		if (allDone)
//			repathAll ();

//		doneStarting = true;
		repathAll();
		doPathfinding = Dev.GetComponent<DevMovement> ().doPathfinding;
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

	// Update is called once per frame
	public void FrameUpdate () {
		if (!doPathfinding)
			return;


		checkIfAllReady ();
		updateYourCell ();

		//--------CHECKING IF DEV IS NEAR ENOUGH FOR ENEMIES TO NOTICE HIM--------//
		//		if (!Camera.main.GetComponent<MouseMovement> ().inCombatZone) {
		//			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 0f, 5f * Time.deltaTime));
		//			return;
		//		}

		//--------CHECKING IF THIS ENEMY IS DEAD--------------//
		//		if (GetComponent<ManageHealth> ().isDead ())
		//			this.gameObject.SetActive (false);
		moveToDev ();
		Debug.Log ("Got here");
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
//		path = null;
//		nextDest = null;
//		inPosition = false;
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

	void moveBack(){
		enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), -1f, 5f * Time.deltaTime));
		transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
	}

	void stop(){
		enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards (enemyAnim.GetFloat ("enemySpeed"), 0f, 2f * Time.deltaTime));
	}
		

	void moveToTarget(){
		if (finalDest == null || start.equalTo(finalDest) || !isEnemyRunning()) {
			stop ();
		} else {

			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 1f, 5f * Time.deltaTime));
			transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
		}
	}

	float clampAngle(float orig){
		while (orig > 180f)
			orig -= 360f;
		while (orig < -180f)
			orig += 360f;
		return orig;
	}

	void rotateToTarget(Vector3 targ){
		dif = targ - transform.position;
		dif.x = clampAngle (dif.x);
		dif.z = clampAngle (dif.z);
		dif = new Vector3 (dif.x, 0f, dif.z);
		transform.forward = Vector3.RotateTowards (transform.forward, dif, rotSpeed * Time.deltaTime, 0.0f); 
	}

	public bool isEnemyAttacking() {
		AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo (0);
		return info.IsName ("QUICK1") || info.IsName ("QUICK2") || info.IsName ("QUICK3") || info.IsName ("QUICK4") || info.IsName ("QUICK5");
	}

	public bool isEnemyRunning(){
		AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo (0);
		return info.IsTag ("enemyRun");
	}

	private void attack() {
		enemyAnim.SetBool ("enemyAttack", true);
		Invoke ("switchAttack", 0.5f);
	}

	private void switchAttack(){
		switch (enemyAnim.GetInteger ("enemyQuick")) {
		case 1:
			enemyAnim.SetInteger ("enemyQuick", 2);
			break;
		case 2:
			enemyAnim.SetInteger ("enemyQuick", 3);
			break;
		case 3:
			enemyAnim.SetInteger ("enemyQuick", 4);
			break;
		case 4:
			enemyAnim.SetInteger ("enemyQuick", 5);
			break;
		case 5:
			enemyAnim.SetInteger ("enemyQuick", 1);
			break;
		default:
			Debug.LogAssertion ("quickAttack is not set to 1-5, look at EnemyAI.cs script (switchAttack method)");
			break;
		}
	}
		
	private float rand(float a, float b){
		return UnityEngine.Random.Range (a, b);
	}

	private bool Approx(Vector3 a, Vector3 b){
		return Mathf.Approximately (a.x, b.x) && Mathf.Approximately (a.y, b.y) && Mathf.Approximately (a.z, b.z);
	}

	//	private void applyRotation() {
	//		//if not moving
	//		if (Mathf.Approximately (enemyAnim.GetFloat ("enemySpeed"), 0f)) {
	//			if ((oldDev == Vector3.zero) || (Vector3.Magnitude (oldDev - Dev.transform.position) > 0.7f && Vector3.Magnitude (Dev.transform.position - transform.position) > 0.7f)) {
	//				dif = Dev.transform.position - transform.position;
	//				dif = new Vector3 (dif.x, 0f, dif.z);
	//				Vector3 perpenDif = Vector3.Normalize (Vector3.Cross (dif, -1.0f * dif)) * rand (1f, -1f);
	//				devTarget = Dev.transform.position;
	//				target = Dev.transform.position + perpenDif;
	//				dif = target - transform.position;
	//				dif = new Vector3 (dif.x, 0f, dif.z);
	//			}		
	//		//if moving
	//		} else {
	//			if ((Vector3.Magnitude (oldDev - Dev.transform.position) > 0.7f && Vector3.Magnitude (Dev.transform.position - transform.position) > 0.7f)) {
	//				dif = Dev.transform.position - transform.position;
	//				dif = new Vector3 (dif.x, 0f, dif.z);
	//				Vector3 perpenDif = Vector3.Normalize (Vector3.Cross (dif, -1.0f * dif)) * rand (1f, -1f);
	//				devTarget = Dev.transform.position;
	//				target = Dev.transform.position + perpenDif;
	//				dif = target - transform.position;
	//				dif = new Vector3 (dif.x, 0f, dif.z);
	//			} else {
	//				dif = target - transform.position;
	//				dif = new Vector3 (dif.x, 0f, dif.z);
	//			}
	//		}
	//
	//		transform.forward = Vector3.RotateTowards (transform.forward, dif, rotSpeed * Time.deltaTime, 0.0f); 
	//	}
	//
	//	private void applySpeed() {
	//		if (isEnemyAttacking ()) {
	//			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 0f, 5f * Time.deltaTime));
	//			return;
	//		}
	//		if (Vector3.Magnitude (dif) < 2f) {
	//			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards (enemyAnim.GetFloat ("enemySpeed"), 0f, 5f * Time.deltaTime));
	//		}
	////		else if (Vector3.Magnitude (dif) < 1f) {
	////			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards (enemyAnim.GetFloat ("enemySpeed"), -1f, 5f * Time.deltaTime));
	////			transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
	////		}
	//		else {
	//			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 1f, 2f * Time.deltaTime));
	//			transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
	//		}
	//	}

	//	private void reduceSpeed(){
	//		enemyAnim.SetFloat ("enemySpeed", Mathf.Max (0f, enemyAnim.GetFloat ("enemySpeed") - 0.03f));
	//	}


}