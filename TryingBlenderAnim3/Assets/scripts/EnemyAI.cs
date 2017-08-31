using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

	public int enemyID;

	private Animator enemyAnim;
	private GameObject Dev;
	private float rotSpeed;
	private float moveSpeed;
	private Vector3 dif;
	private Vector3 oldDev;
	//	private Vector3 target;
	//	private Vector3 devTarget;
	private Queue<mapNode> path;
	private GameObject terrain;
	private mapNode nextDest;
	private mapNode finalDest;
	private mapNode start;
	private float restStartTime;
	private bool resting;
	private mapNode oldDevCell;

	// Use this for initialization
	void Start () {
		terrain = GameObject.Find ("Terrain");
//		while (!terrain.GetComponent<MapPathfind> ().doneBuilding) {}
		enemyAnim = GetComponent<Animator> ();
		Dev = GameObject.Find ("DevDrake");
		rotSpeed = 5f;
		moveSpeed = 5f;
		if (!GameObject.Find("Terrain").GetComponent<MapPathfind>().doneBuilding) {
			GameObject.Find ("Terrain").GetComponent<MapPathfind> ().Start ();
		}
		initPathToDev ();
		resting = false;
		restStartTime = Time.time;
	}

	public mapNode getDevCell(){
		mapNode ret = terrain.GetComponent<MapPathfind> ().devCell;
		if (ret == null) {
			Dev.GetComponent<DevMovement> ().initDevCell ();
			ret = terrain.GetComponent<MapPathfind> ().devCell;
		}
		return ret;
	}

	// Update is called once per frame
	void Update () {

		//--------CHECKING IF DEV IS NEAR ENOUGH FOR ENEMIES TO NOTICE HIM--------//
		//		if (!Camera.main.GetComponent<MouseMovement> ().inCombatZone) {
		//			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 0f, 5f * Time.deltaTime));
		//			return;
		//		}

		//--------CHECKING IF THIS ENEMY IS DEAD--------------//
		//		if (GetComponent<ManageHealth> ().isDead ())
		//			this.gameObject.SetActive (false);

		moveToDev ();

		//-------------- ALL FOR DEBUGGING-------------//
		//		mapNode[] arr = path.ToArray ();
		//		string s = "";
		//		foreach(mapNode node in arr){
		//			KeyValuePair<int, int> coords = node.getIndices ();
		//			s += (coords.Key + "_" + coords.Value + ", " );
		//			s += (node.getCenter() + " ");
		//		}
		//		Debug.Log (s + "nextDest: " + nextDest.getIndices() + "start: " + start.getIndices() + "dev: " + finalDest.getIndices());
	}

	void initPathToDev(){
		start = terrain.GetComponent<MapPathfind> ().containingCell (transform.position);
		plotNewPath ();
	}

	//keep track of this agent's current location
	void updateYourCell() {
		mapNode oldStart = start;
		start = terrain.GetComponent<MapPathfind> ().containingCell (transform.position);
		if (!oldStart.equalTo (start)) {
			oldStart.setEmpty ();
		}
		start.setFull (enemyID);
	}

	public void plotNewPath(){
		getDevCell ().setFull (0);

		//finalDest = getDevCell().getClosestNeighbor (start, enemyID);
		//finalDest = getDevCell().getEmptySurroundingSpot(enemyID);
		//finalDest = circle [Mathf.RoundToInt (rand (0, circle.Length-1))];
//		mapNode[] circle = terrain.GetComponent<MapPathfind>().getEmptyDevCombatCircle(3, enemyID);

		mapNode[] circle = terrain.GetComponent<MapPathfind>().getEmptySpacedDevCombatCircle(3, enemyID);
		finalDest = terrain.GetComponent<MapPathfind> ().findClosestNode (circle, start);
			
		if (finalDest == null) {
			Debug.LogError ("finalDest = null");
			stop ();
			return;
		}
		finalDest.setFull (enemyID);
		while (path !=null && path.Count > 0) {
			mapNode trashNode = path.Dequeue ();
			trashNode.setEmpty ();
		}

		path = terrain.GetComponent<MapPathfind> ().findPath (start, finalDest, enemyID);

		if (path.Count == 0)
			nextDest = null;
		else
			nextDest = path.Dequeue ();
	}

	void moveToDev(){
		updateYourCell ();

		//		if dev's location changed or the current path is blocked
		//		if (finalDest == null || didDevCellChange() || (nextDest!= null && nextDest.hasOtherOwner(enemyID))) {
		if (finalDest == null || (nextDest!= null && nextDest.hasOtherOwner(enemyID)) || finalDest!= null && finalDest.hasOtherOwner(enemyID)) {
//		if (finalDest == null) {
			plotNewPath ();
		}

		if (start.equalTo (finalDest) || nextDest == null) {
			if (Vector3.Distance (Dev.transform.position, transform.position) < 1f) {
				rotateToTarget (Dev.transform.position);
			}
			//rotate towards player
			rotateToTarget (Dev.transform.position);
			//attack
			attack ();
			return;
		} else {
			stopEnemyAttack ();
		}

		//reached intermediate destination
		if (nextDest == null || start.equalTo(nextDest)) {
			nextDest = path.Dequeue ();
			if (nextDest == null || nextDest.getCenter () == null) {
				Debug.LogAssertion ("nextDest is messed up");
				return;
			}
		}
		//rotate towards nextDest
		rotateToTarget(nextDest.getCenter());

		//move towards nextDest
		moveToTarget();
	}

	void moveBack(){
		enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), -1f, 5f * Time.deltaTime));
		transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
	}

	void stop(){
		enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards (enemyAnim.GetFloat ("enemySpeed"), 0f, 5f * Time.deltaTime));
	}

	void moveToTarget(){
		if (finalDest == null || start.equalTo(finalDest) || !isEnemyRunning()) {
			stop ();
		} else {
			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 1f, 5f * Time.deltaTime));
			transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
		}
	}

	void rotateToTarget(Vector3 targ){
		dif = targ - transform.position;
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

	public void stopEnemyAttack(){
		enemyAnim.SetBool ("enemyAttack", false);
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