using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevMovement : MonoBehaviour {

#region imports you don't need to worry about
public bool doPathfinding,
			hangDrop,
			isInHangDrop;

public int hangDropStage;

public GameObject footDust;
public Transform leftFoot, rightFoot;

#region AudioSource Imports
public AudioSource footstep1,
				   footstep2,
				   footstep3,
				   footstep4,
				   land,
				   flipJump;
#endregion

#region other imports (scripts, gameobjs, etc)
private MouseMovement mouseMovementScript;
private DevCombat devCombatScript;
private MapPathfind gridGraphScript;
private ClosestNodes closestNodesScript;

private GameObject player, terrain;
private Transform CamTransform;
private Animator myAnimator;
private mapNode lastRegenNode;

private bool movingVert, 
			 movingHoriz,
			 inCombatZone,
			 horizRot,
			 applyJumpTrans;

private float needToRot, desiredRot;
private int runCounter, adjustCounter;

#endregion
#endregion

#region imports you can tweak / need to worry about
private const float rollDistanceMultiplier = 0.0f;
private Quaternion dustRot = Quaternion.Euler (-30f, -180f, 0f);
#endregion

#region Start and Update
public void Start () {
	player = GameObject.Find ("DevDrake");
	terrain = GameObject.Find ("Terrain");
	CamTransform = Camera.main.transform;
	myAnimator = GetComponent<Animator> ();

	mouseMovementScript = Camera.main.GetComponent<MouseMovement>();
	devCombatScript = player.GetComponent<DevCombat> ();
	desiredRot = Camera.main.transform.eulerAngles.y;
	if (doPathfinding) {
		closestNodesScript = terrain.GetComponent<ClosestNodes> ();
		gridGraphScript = terrain.GetComponent<MapPathfind> ();
		initDevCell ();
	}
}

void Update () {
		if (isInHangDrop) {
			if (hangDropStage == 0) {
//				myAnimator.SetFloat ("VSpeed", 0f); 
//				myAnimator.SetFloat ("HorizSpeed", 0f); 
//				++hangDropStage;

				myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), 0f, 4.0f*Time.deltaTime)); 
				myAnimator.SetFloat ("HorizSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("HorizSpeed"), 0f, 4.0f*Time.deltaTime)); 
				if (Mathf.Approximately (myAnimator.GetFloat ("VSpeed"), 0f) && Mathf.Approximately (myAnimator.GetFloat ("HorizSpeed"), 0f)) {
					++hangDropStage;
				}
			}
			if (hangDropStage == 1){	
				myAnimator.CrossFade ("Drop To Freehang Start", 0.1f);
				++hangDropStage;
			}
			return;
		}

	if(doPathfinding)
		setDevCell ();
	
	AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
	if (anim.IsTag ("equip"))
		return;
	if (anim.IsTag ("impact"))
		impactMoveBack ();
	if (rolling ())
		transform.Translate (Vector3.forward * Time.deltaTime * rollDistanceMultiplier);


	moveCharacter ();

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

	if (anim.IsName ("quick_roll_to_run")) {
		transform.Translate (Vector3.forward * Time.deltaTime * 2f);
	}

	if(Input.GetButtonDown("Jump") && movingVert && adjustCounter == 0 
		&& devCombatScript.notInCombatMove())
	{
		myAnimator.SetBool("Jumping", true);
		Invoke ("stopJumping", 0.8f);
	}
	else if(Input.GetButtonDown("FrontFlip") && movingVert && adjustCounter == 0
		&& devCombatScript.notInCombatMove())
	{
		myAnimator.SetBool ("shouldFrontFlip", true);
		Invoke ("stopFrontFlip", 2.1f);
	}

	//rotate dev horizontally IF dev is moving and in a non-combat zone, 
	//but is not currently adjusting to a camera shift, jumping, flipping, or attacking/blocking
	if(!inCombatZone && adjustCounter == 0 && (movingVert || movingHoriz)) {
		if (!anim.IsTag("Jumps") && !myAnimator.GetBool ("Jumping") && !myAnimator.GetBool ("shouldFrontFlip") && devCombatScript.notInCombatMove()) {
			transform.Rotate (Vector3.up * Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseMovementScript.sensitivityX);
		}
	}
}
#endregion

#region Non-combat and Combat Movement
private void moveCharacter(){
	bool inCombatZone = mouseMovementScript.getInCombatZone();
	bool weaponDrawn = myAnimator.GetBool ("WeaponDrawn");
	bool inCombatMove = !devCombatScript.notInCombatMove ();
	movingVert = !Mathf.Approximately (myAnimator.GetFloat ("VSpeed"), 0f);
	movingHoriz = !Mathf.Approximately (myAnimator.GetFloat ("HorizSpeed"), 0f);

	if (!devCombatScript.isAttacking ()) {
		if (inCombatZone && weaponDrawn && !jumping ()) {
			combatMovement ();
		} else {
			//			Debug.LogError ("Not in combat movement");
			//			mouseMovementScript.setInCombatZone(false);
			nonCombatMovement ();
		}
	}
}

private void nonCombatMovement(){
	AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
	if ((Time.time - mouseMovementScript.getCombatExitTime()) < 1f) {
		myAnimator.SetFloat ("VSpeed", 0f);
		myAnimator.SetFloat ("HorizSpeed", 0f);
	}
	if(anim.IsTag("Running")) {
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
				myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), Input.GetAxisRaw ("Vertical"), 0.1f)); 
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
	myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), X * 0.5f, 4f * Time.deltaTime));
	myAnimator.SetFloat ("HorizSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("HorizSpeed"), Y * 0.5f, 4f * Time.deltaTime));

	transform.Translate (((Vector3.forward * X) + (Vector3.right * Y * 0.75f)) * Time.deltaTime * 0.5f);
}
#endregion

#region Grid Graph Cell Functions
private mapNode getDevCell(){
	return gridGraphScript.devCell;
}

public void initDevCell(){
	if(terrain==null)
		terrain = GameObject.Find ("Terrain");
	gridGraphScript.devCell = gridGraphScript.containingCell (transform.position);
	markNeighbors ();
}

private void clearNeighbors(){
	mapNode[] neighbors = gridGraphScript.devCell.getNeighbors ();
	foreach (mapNode node in neighbors)
		node.setEmpty ();
}

private void markNeighbors(){
	mapNode[] neighbors = gridGraphScript.devCell.getNeighbors ();
	foreach (mapNode node in neighbors)
		node.setFull (-3);		
}

public void setDevCellNoRepath(){
	//dev's current location
	mapNode newDevCell = gridGraphScript.containingCell (transform.position);
	if (newDevCell == null) {
		Debug.LogAssertion ("bad");
	}
	else if (!newDevCell.equalTo (gridGraphScript.devCell)) {

		clearNeighbors ();
		gridGraphScript.devCell.setEmpty ();
		gridGraphScript.devCell = newDevCell;
		gridGraphScript.devCell.setFull (-3);
		markNeighbors ();
	}
}

public void setDevCell() {
	//dev's current location
	mapNode newDevCell = gridGraphScript.containingCell (transform.position);
	if (newDevCell == null) {
		Debug.LogAssertion ("bad");
	}
	//			if (GameObject.Find ("Enemy") != null && !newDevCell.equalTo (gridGraphScript.devCell)) {
	else if (!newDevCell.equalTo (gridGraphScript.devCell)) {

		gridGraphScript.devCell.setEmpty ();
		gridGraphScript.devCell = newDevCell;
		gridGraphScript.devCell.setFull (-3);
		if (lastRegenNode == null || lastRegenNode.distance (newDevCell) >= 3f) {
			closestNodesScript.regenPathsLongQuick();
			lastRegenNode = newDevCell;
		}
	}
}
#endregion



/*	void rotateRight(){
		transform.Rotate(new Vector3(0f, 9f, 0f)); 
	}
*/

/*	void rotateLeft(){
		transform.Rotate(new Vector3(0f, -9f, 0f));  
	}
*/

/*void stopTurnRight(){
myAnimator.SetBool ("TurnRight", false);
}*/

/*void stopTurnLeft(){
myAnimator.SetBool ("TurnLeft", false);
}*/


#region sounds
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

void flipTakeOffSound(){
	flipJump.Play ();
}

void landingSound(){
	land.Play ();
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

#endregion

#region methods called by animation events
//0:17 or 17
public void startHangDrop(){
	hangDrop = true;
}

//1:09 or 69
//69-17 = 52 = duration
public void finishHangDrop(){
//	hangDrop = false;
}


void spawnFootDust(int doLeftFoot){
	GameObject footDustClone = null;
	Vector3 dustPos = Vector3.zero;
	if (doLeftFoot==0) {
		dustPos = leftFoot.position - (0.25f * leftFoot.right) - (0.2f * transform.forward) - (0.1f * transform.up);
	} else {
		dustPos = rightFoot.position + (0.2f * rightFoot.right) - (0.25f * transform.forward) - (0.3f * transform.up);
	}
	footDustClone = Instantiate (footDust, dustPos, transform.rotation);
	footDustClone.GetComponent<ParticleSystem> ().Play ();

	Destroy (footDustClone, 2.0f);
}

void onApplyTrans(){
	applyJumpTrans = true;
}

void offApplyTrans(){
	applyJumpTrans = false;
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
#endregion

public void setHanging(){
	myAnimator.SetBool ("hanging", true);
}

public void impactMoveBack(){
	transform.Translate (Vector3.back * 0.5f * Time.deltaTime);
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

#region common helpers
public GameObject[] getEnemies(){
	return GameObject.FindGameObjectsWithTag ("Enemy");
}

private float rand(float a, float b){
	return UnityEngine.Random.Range (a, b);
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
#endregion

#region methods to get info about player state
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

public bool isDropToFreehang(){
	AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
	return anim.IsName ("Drop To Freehang");
}

public bool isHanging(){
	AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
	return anim.IsName ("Drop To Freehang Start") || anim.IsName ("Hanging Idle") || 
		anim.IsName ("Left Shimmy") || anim.IsName ("Right Shimmy") || anim.IsName ("Freehang Climb");
}
#endregion

#region getters
public bool getHorizRot(){ return horizRot; }
public int getAdjustCounter(){ return adjustCounter; }
#endregion

#region setters
public void setAdjustCounter(int _adjustCounter){
	adjustCounter = _adjustCounter;
}

public void setHorizRot(bool _horizRot){
horizRot = _horizRot;
}
#endregion
}