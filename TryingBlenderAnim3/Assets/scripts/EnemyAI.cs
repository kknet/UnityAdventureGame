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
	private mapNode devCell;
	private Queue<mapNode> path;
	private GameObject terrain;
	private mapNode nextDest;
	private mapNode finalDest;
	private mapNode start;

	// Use this for initialization
	void Start () {
		enemyAnim = GetComponent<Animator> ();
		Dev = GameObject.Find ("DevDrake");
		rotSpeed = 5f;
		moveSpeed = 5f;
//		target = Vector3.zero;
		terrain = GameObject.Find ("Terrain");
		initPathToDev ();
	}

	// Update is called once per frame
	void Update () {
//		if (!Camera.main.GetComponent<MouseMovement> ().inCombatZone) {
//			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), 0f, 5f * Time.deltaTime));
//			return;
//		}
//		if (GetComponent<ManageHealth> ().isDead ())
//			this.gameObject.SetActive (false);

		moveToDev ();

		mapNode[] arr = path.ToArray ();
		string s = "";
		foreach(mapNode node in arr){
//			KeyValuePair<int, int> coords = node.getIndices ();
//			s += (coords.Key + "_" + coords.Value + ", " );
			s += (node.getCenter() + " ");
		}
//		Debug.Log (s + "nextDest: " + nextDest.getIndices() + "start: " + start.getIndices() + "dev: " + finalDest.getIndices());
	}

	void initPathToDev(){
		start = terrain.GetComponent<MapPathfind> ().containingCell (transform.position);
		devCell = terrain.GetComponent<MapPathfind> ().containingCell (Dev.transform.position);
		finalDest = devCell.getClosestNeighbor(start, enemyID);
		path = terrain.GetComponent<MapPathfind> ().findPath (start, finalDest, enemyID);
		nextDest = path.Dequeue ();
	}

	void moveToDev(){
		//enemy's current location
		mapNode oldStart = start;
		start = terrain.GetComponent<MapPathfind> ().containingCell (transform.position);
		if (!oldStart.equalTo (start)) {
			oldStart.setEmpty ();
		}
		start.setFull (enemyID);

		//dev's current location
		mapNode temp = terrain.GetComponent<MapPathfind> ().containingCell (Dev.transform.position);
		if (temp == null) {
			Debug.LogAssertion ("bad");
			return;
		}

//		if dev's location changed or the current path is blocked
		if (!temp.equalTo(devCell) || (nextDest!= null && nextDest.hasOtherOwner(enemyID))) {

			devCell.setEmpty ();
			devCell = temp;
			devCell.setFull (0);



			//randomly choose one of the neighboring cells of dev's cell as your destination
			//and create a path to this neighboring cell
			finalDest = devCell.getClosestNeighbor (start, enemyID);

			finalDest.setFull (enemyID);
			while (path.Count > 0) {
				mapNode trashNode = path.Dequeue ();
				trashNode.setEmpty ();
			}

			path = terrain.GetComponent<MapPathfind> ().findPath (start, finalDest, enemyID);

			if (path.Count == 0)
				nextDest = null;
			else
				nextDest = path.Dequeue ();
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
		moveToTarget(nextDest.getCenter());

//
//		//move backwards
//		if (Vector3.Distance (Dev.transform.position, transform.position) < 1f)
//			moveBack();
//		
//		//move towards nextDest
//		else
//			moveToTarget(nextDest.getCenter());
	}

	void moveBack(){
		enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards(enemyAnim.GetFloat ("enemySpeed"), -1f, 5f * Time.deltaTime));
		transform.Translate(Vector3.forward * enemyAnim.GetFloat("enemySpeed") * Time.deltaTime * moveSpeed);
	}

	void moveToTarget(Vector3 targ){
		if (start.equalTo(finalDest) || !isEnemyRunning()) {
			enemyAnim.SetFloat ("enemySpeed", Mathf.MoveTowards (enemyAnim.GetFloat ("enemySpeed"), 0f, 5f * Time.deltaTime));
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