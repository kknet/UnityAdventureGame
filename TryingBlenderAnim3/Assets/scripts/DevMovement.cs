using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevMovement : MonoBehaviour {

	public GameObject player;
	public Transform CamTransform;
	public Animator myAnimator;
	public int adjustCounter;
	public AudioSource footstep1;
	public AudioSource footstep2;
	public AudioSource footstep3;
	public AudioSource footstep4;
	public AudioSource land;
	public AudioSource flipJump;
	public bool horizRot;
//	public bool devCellChanged;

	private GameObject terrain;
	private float desiredRot;
	private bool applyJumpTrans;
	private float needToRot;
	private int runCounter;
	//	private int turnCounter;
	//	private float turn;

	// Use this for initialization
	public void Start () {
		terrain = GameObject.Find ("Terrain");
		myAnimator = GetComponent<Animator>();
		needToRot = 0;
		adjustCounter = 0;
		runCounter = 0;
		applyJumpTrans = false;
		desiredRot = Camera.main.transform.eulerAngles.y;
		horizRot = false;
		initDevCell ();
	}

	private mapNode getDevCell(){
		return terrain.GetComponent<MapPathfind> ().devCell;
	}

	public void initDevCell(){
		if(terrain==null)
			terrain = GameObject.Find ("Terrain");
		terrain.GetComponent<MapPathfind> ().devCell = terrain.GetComponent<MapPathfind> ().containingCell (transform.position);
	}

	public GameObject[] getEnemies(){
		return GameObject.FindGameObjectsWithTag ("Enemy");
	}

	private float rand(float a, float b){
		return UnityEngine.Random.Range (a, b);
	}

	private void setDevCell() {
		//dev's current location
		mapNode newDevCell = terrain.GetComponent<MapPathfind> ().containingCell (transform.position);
		if (newDevCell == null) {
			Debug.LogAssertion ("bad");
		}
		else if (!newDevCell.equalTo (terrain.GetComponent<MapPathfind> ().devCell)) {
			terrain.GetComponent<MapPathfind> ().devCell.setEmpty ();
			terrain.GetComponent<MapPathfind> ().devCell = newDevCell;
			terrain.GetComponent<MapPathfind> ().devCell.setFull (-3);
			GameObject[] enemies = getEnemies();
			foreach (GameObject enemy in enemies) {
//				if(rand(0f, 1f) > 0.5f)
					enemy.GetComponent<EnemyAI> ().plotNewPath ();
			}
//			foreach (GameObject enemy in enemies) {
//				Debug.LogError (enemy.GetComponent<EnemyAI> ().finalDest.getIndices ());
//			}
//			GameObject overlapper = terrain.GetComponent<MapPathfind> ().overlappingAgent ();
//			if (overlapper != null) {
//				overlapper.GetComponent<EnemyAI> ().plotNewPath ();
//				Debug.LogError ("Worked!");
//			}
		}
	}

	private void nonCombatMovement(){
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
		if ((Time.time - Camera.main.GetComponent<MouseMovement> ().combatExitTime) < 1f) {
			myAnimator.SetFloat ("VSpeed", 0f);
			myAnimator.SetFloat ("HorizSpeed", 0f);
		}
		else if(anim.IsTag("Running")) {
			float doIt = 1f;
			if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) {
				myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), Input.GetAxisRaw ("Vertical"), 0.1f)); 
			} else if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {
				myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), -1.0f * Input.GetAxisRaw ("Vertical"), 0.1f)); 
			} else if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
				myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), -1.0f * Input.GetAxisRaw ("Horizontal"), 0.1f));
			} else if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
				myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), Input.GetAxisRaw ("Horizontal"), 0.1f));
			}  else {
				if(!horizRot)
					myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), Input.GetAxisRaw ("Vertical"), 0.2f)); 
				doIt = 0f;
			}
			transform.Translate (Vector3.forward * Time.deltaTime * 5f * myAnimator.GetFloat ("VSpeed") * doIt);
		}
	}

	private void combatMovement() {
		int dif = (int)(CamTransform.eulerAngles.y - transform.eulerAngles.y);
		if (dif < 0)
			dif += 360;

		bool W = (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow));
		bool A = (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow));
		bool S = (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow));
		bool D = (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow));
		W = W && !S;
		S = S && !W;
		A = A && !D;
		D = D && !A;

		int X = 0;
		int Y = 0;

		//round to the nearest 90 degrees
		int div = dif / 90;
		int rem = dif % 90;
		if (rem >= 45 || rem <= -45) {
			if (div < 0) {
				--div;
			} else {
				++div;
			}
		}
		dif = div * 90;
		if (W || A || S || D) {
			bool angle0 = dif == 0 || dif == 360 || dif == -360;
			bool angle90 = dif == 90 || dif == -270;
			bool angleN90 = dif == -90 || dif == 270;
			bool angle180 = dif == 180 || dif == -180;
			if(angle0) {
				if (W)	X=1;
				else if (S) X=-1;
				if (D) Y=1;
				else if (A) Y=-1;
			}
			else if (angle90) {
				if (W) Y=1;
				else if (S) Y=-1;
				if (D) X=-1;
				else if (A) X=1;
			}
			else if(angle180) {
				if (W) X=-1;
				else if (S) X=1;
				if (D) Y=-1;
				else if (A) Y=1;
			}
			else if(angleN90) {
				if (W) Y=-1;
				else if (S) Y=1;
				if (D) X=1;
				else if (A) X=-1;
			}
		}
		myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), X * 1f, 4f * Time.deltaTime));
		myAnimator.SetFloat ("HorizSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("HorizSpeed"), Y * 1f, 4f * Time.deltaTime));

		if(S && !D && !A)
			transform.Translate (((Vector3.forward * X) + (Vector3.right * Y)) * Time.deltaTime * 2f);
		else
			transform.Translate (((Vector3.forward * X) + (Vector3.right * Y)) * Time.deltaTime * 3f);
	}

	void Update () {
		setDevCell ();
		mapNode ourCell = GameObject.Find ("Terrain").GetComponent<MapPathfind> ().containingCell (transform.position);
		if (ourCell!=null) {
			KeyValuePair<int, int> coords = ourCell.getIndices ();
			Debug.Log (coords.Key + ", " + coords.Value);
		}
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
		bool inCombatZone = Camera.main.GetComponent<MouseMovement> ().inCombatZone;
		bool weaponDrawn = myAnimator.GetBool ("WeaponDrawn");
		bool inCombatMove = !GetComponent<DevCombat> ().notInCombatMove ();
		bool movingVert = !Mathf.Approximately (myAnimator.GetFloat ("VSpeed"), 0f);
		bool movingHoriz = !Mathf.Approximately (myAnimator.GetFloat ("HorizSpeed"), 0f);

		if (anim.IsTag ("equip"))
			return;
		if (anim.IsTag ("impact"))
			impactMoveBack ();
		if (rolling ())
			transform.Translate (Vector3.forward * Time.deltaTime * 0.5f);
						
		if (inCombatZone && weaponDrawn && !jumping() && !inCombatMove)
			combatMovement ();
		else
			nonCombatMovement ();

		if (!movingVert && !movingHoriz) {
			stopFootstepSound ();
		}

		if (applyJumpTrans && anim.IsTag("Jumps")) {
			if(anim.IsName("running_jump")) {
				transform.Translate (Vector3.forward * Time.deltaTime * 8f);
			} else {
				transform.Translate (Vector3.forward * Time.deltaTime * 12f);
			}
		}

		if (myAnimator.GetBool ("WeaponDrawn") && Input.GetButtonDown("Jump")) {
			myAnimator.SetBool ("roll", true);
			Invoke ("stopRolling", 1.0f);
		}

		if (anim.IsName ("quick_roll_to_run")) {
			transform.Translate (Vector3.forward * Time.deltaTime * 2f);
		}

		if(Input.GetButtonDown("Jump") && movingVert && adjustCounter == 0 
			&& player.GetComponent<DevCombat>().notInCombatMove())
		{
			myAnimator.SetBool("Jumping", true);
			Invoke ("stopJumping", 0.8f);
		}
		else if(Input.GetButtonDown("FrontFlip") && movingVert && adjustCounter == 0
			&& player.GetComponent<DevCombat>().notInCombatMove())
		{
			myAnimator.SetBool ("shouldFrontFlip", true);
			Invoke ("stopFrontFlip", 2.1f);
		}

		//rotate dev horizontally IF dev is moving and in a non-combat zone, 
		//but is not currently adjusting to a camera shift, jumping, flipping, or attacking/blocking
		if(!inCombatZone && adjustCounter == 0 && (movingVert || movingHoriz)) {
			if (!anim.IsTag("Jumps") && !myAnimator.GetBool ("Jumping") && !myAnimator.GetBool ("shouldFrontFlip") && player.GetComponent<DevCombat>().notInCombatMove()) {
				transform.Rotate (Vector3.up * Input.GetAxisRaw("Mouse X") * Time.deltaTime * Camera.main.GetComponent<MouseMovement>().sensitivityX);
			}
		}
	}

	public void impactMoveBack(){
		transform.Translate (Vector3.back * 0.5f * Time.deltaTime);
	}

	void rotateRight(){
		transform.Rotate(new Vector3(0f, 9f, 0f)); 
	}

	void rotateLeft(){
		transform.Rotate(new Vector3(0f, -9f, 0f));  
	}

	void runningSound(){
		if (runCounter == 0)
			footstep1.Play ();
		else if (runCounter == 1)
			footstep2.Play ();
		else if (runCounter == 2)
			footstep3.Play ();
		else if (runCounter == 3)
			footstep4.Play ();
		++runCounter;
		if (runCounter == 4)
			runCounter = 0;
	}

	void horizRunningSound(){
		if (!Mathf.Approximately(myAnimator.GetFloat ("VSpeed"), 0f))
			return;
		if (runCounter == 0)
			footstep1.Play ();
		else if (runCounter == 1)
			footstep2.Play ();
		else if (runCounter == 2)
			footstep3.Play ();
		else if (runCounter == 3)
			footstep4.Play ();
		++runCounter;
		if (runCounter == 4)
			runCounter = 0;
	}

	void onApplyTrans(){
		applyJumpTrans = true;
	}

	void offApplyTrans(){
		applyJumpTrans = false;
	}
		
	void flipTakeOffSound(){
		flipJump.Play ();
	}

	void landingSound(){
		land.Play ();
	}

	void stopRolling(){
		myAnimator.SetBool ("roll", false);
	}

	void stopJumping()
	{
		myAnimator.SetBool("Jumping", false);
		applyJumpTrans = false;
	}

	void stopFrontFlip()
	{
		myAnimator.SetBool ("shouldFrontFlip", false);
		applyJumpTrans = false;
	}

	void stopFootstepSound(){
		if (footstep1.isPlaying)
			footstep1.Stop ();
		if (footstep2.isPlaying)
			footstep2.Stop ();
		if (footstep3.isPlaying)
			footstep3.Stop ();
		if (footstep4.isPlaying)
			footstep4.Stop ();
	}


	void stopTurnRight(){
		myAnimator.SetBool ("TurnRight", false);
	}

	void stopTurnLeft(){
		myAnimator.SetBool ("TurnLeft", false);
	}

	public void adjustToCam(float dif, bool firstTimeAdjust)
	{
		if (!firstTimeAdjust && adjustCounter == 0)
			return;
		if(firstTimeAdjust)  {
			if (dif > 180f)
				dif = dif - 360f;
			else if (dif < -180f)
				dif = dif + 360f;

			needToRot = dif / 20.0f;
			adjustCounter = 20;
		}
		transform.Rotate (Vector3.up * needToRot);
		--adjustCounter;
	}

	private float clamp(float angle){
		if (angle < -180f) {
			angle += 360f;
			return clamp (angle);
		}
		else if (angle > 180f) {
			angle -= 360f;
			return clamp (angle);
		}

		return angle;
	}

	public bool isIdle()
	{
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);

		if (!anim.IsTag("Jumps") && myAnimator.GetFloat ("VSpeed") == 0 && myAnimator.GetFloat ("HorizSpeed") == 0 && myAnimator.GetBool ("shouldFrontFlip") == false && myAnimator.GetBool ("Jumping") == false)
		{
			return true;
		}
		return false;
	}

	public bool jumping(){
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
		return anim.IsTag ("Jumps");
	}

	public bool rolling(){
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
		return anim.IsTag ("roll");
	}

	public bool turning(){
		return !Mathf.Approximately(desiredRot, transform.eulerAngles.y);
	}

	private bool camRotChanged(){
		return !Mathf.Approximately (Camera.main.transform.eulerAngles.y, desiredRot);
	}

}