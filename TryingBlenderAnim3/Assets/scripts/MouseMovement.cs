using UnityEngine;

public class MouseMovement : MonoBehaviour {

	#region globals
	public float sensitivityX, 
				 sensitivityY,
				 movementY,
				 movementX;

	public GameObject devHair;

	private bool firstTimeAdjust, 
				 jumping, 
				 triggeredDraw,
				 inCombatZone,
				 wepIsOut,
				 oldInCombatZone,
				 haveDevCombatOffset;

	private Vector3 closestEnemy,
					displacement,
					rollAngle;

	private float dif, 
				  goal,
   				  distance,
				  combatExitTime,
				  enemyLockOnStart,
				  lastCombatTime,
				  lastEnemyCheckTime,
				  lastLineCastTime,
				  lineCastPeriod;

	private GameObject player, 
					   closestEnemyObject;

	private Animator myAnimator;

	private DevMovement devMovementScript;
	private DevCombat devCombatScript;

	[SerializeField][HideInInspector]
	private Vector3 initialOffset;
	private Vector3 currentOffset;
	#endregion

	#region Start and Update
	private void Start () {
		player = GameObject.Find ("DevDrake");
		if(player == null) {
			Debug.LogError ("Assign a player for the camera in Unity's inspector");
		}
		myAnimator = player.GetComponent<Animator> ();
		currentOffset = initialOffset;
		goal = player.transform.rotation.eulerAngles.y;
		distance = initialOffset.magnitude;
		lastEnemyCheckTime = Time.realtimeSinceStartup;
		devMovementScript = player.GetComponent<DevMovement>();
		devCombatScript = player.GetComponent<DevCombat>();
		closestEnemyObject = GameObject.Find ("Brute2");
		haveDevCombatOffset = true;
		lineCastPeriod = 1f;
	}

	private void Update(){
		transform.position = player.transform.position + currentOffset;

		//GET NEAREST ENEMY DOESN'T EXACTLY WORK!
		//		getNearestEnemy (); 


		if(closestEnemyObject)
			closestEnemy = closestEnemyObject.transform.position;

		if ((Time.realtimeSinceStartup - lastEnemyCheckTime) > 1.0f) {
			inCombatZone = (closestEnemy != Vector3.zero);
			lastEnemyCheckTime = Time.realtimeSinceStartup;
		}

		if(inCombatZone && myAnimator.GetBool("WeaponDrawn")){
			HorizontalCombatRotation ();
			VerticalCombatRotation ();
			lastCombatTime = Time.time;
		} else {
			VerticalRotation ();
			HorizontalRotation ();		
		}
		currentOffset = (transform.position - devHair.transform.position).normalized * distance;
	}
	#endregion

	#region miscellaneous helper functions

	[ContextMenu("Set Current Offset")]
	private void SetCurrentOffset () {
		initialOffset = transform.position - player.transform.position;
	}

	private bool movementButtonPressed(){
		return Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.A) 
			|| Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.D)
			|| Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.LeftArrow) 
			|| Input.GetKeyDown (KeyCode.RightArrow) || Input.GetKeyDown (KeyCode.DownArrow);
	}

	private float rand(float a, float b){
		return UnityEngine.Random.Range (a, b);
	}

	private bool Approx(Vector3 a, Vector3 b){
		return Mathf.Approximately (a.x, b.x) && Mathf.Approximately (a.z, b.z);
	}

	private void difClamp(){
		if (dif > 180f)
			dif = dif - 360f;
		else if (dif < -180f)
			dif = dif + 360f;
	}

	private bool difBig(){
		return Mathf.Abs(dif) > 2f;
	}

	class tuple {
		Vector3 a;
		float b;
		bool c;
		public tuple(Vector3 A, float B, bool C) {
			a = A;
			b = B;
			c = C;
		}
		public Vector3 first() { return a; }
		public float second() { return b; }
		public bool third() { return c; }
	}

	private tuple closer(float dist, Vector3 oldGuy, Vector3 newGuy){
		float difference = (newGuy - player.transform.position).magnitude;
		if (difference + 2f <= dist)
			return new tuple (newGuy, difference, true);
		return new tuple (oldGuy, dist, false);
	}
	#endregion

	#region NonCombatCamera

	private bool adjustToWalls(float total, float movementY){
		float desiredDist = (initialOffset.magnitude * (35f + total) / 85f);
		Vector3 desiredOffset = (transform.position - player.transform.position).normalized * desiredDist;
		Vector3 desiredCamPos = player.transform.position + desiredOffset;


		RaycastHit hitInfo;
		bool didHit = Physics.Linecast (desiredCamPos, player.transform.position, out hitInfo) && !hitInfo.collider.transform.root.gameObject.name.Equals ("DevDrake");
		float unblockedDist = didHit ? desiredDist - hitInfo.distance : 0f;
		if (didHit) {
			Debug.Log ("Collided");
			if (Time.time - lastLineCastTime > lineCastPeriod) {
				lastLineCastTime = Time.time;
				distance = unblockedDist;
			}		
			return true;
		}
		return false;
	}
		
	private void HorizontalRotation(){
		bool idle = devMovementScript.isIdle ();
		movementX = Input.GetAxisRaw ("Mouse X") * sensitivityX * Time.deltaTime;
		bool combating = !devCombatScript.notInCombatMove ();
		bool counterZero = (devMovementScript.getAdjustCounter() == 0);
		bool camMoved = !Mathf.Approximately (movementX, 0f);
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
		bool jumping = anim.IsTag("Jumps");

		if (combating || idle || jumping) {
			transform.RotateAround (player.transform.position + new Vector3(0.0f, 3.0f, 0.0f), Vector3.up, movementX);
			firstTimeAdjust = true;
			return;
		}

		bool vert = !Mathf.Approximately (Input.GetAxisRaw ("Vertical"), 0f); 
		bool horiz = !Mathf.Approximately (Input.GetAxisRaw ("Horizontal"), 0f); 

		bool W = (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) || (Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.UpArrow));
		bool A = (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) || (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.LeftArrow));
		bool S = (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) || (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow));
		bool D = (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) || (Input.GetKeyDown (KeyCode.D) || Input.GetKeyDown (KeyCode.RightArrow));

		if (vert && horiz && W && A) {
			goal = transform.rotation.eulerAngles.y - 45f;
		}
		else if (vert && horiz && W && D) {
			goal = transform.rotation.eulerAngles.y + 45f;
		}
		else if (vert && horiz && S && A) {
			goal = transform.rotation.eulerAngles.y - 135f;
		}
		else if (vert && horiz && S && D) {
			goal = transform.rotation.eulerAngles.y + 135f;
		}
		else if (vert && W && !S) {
			goal = transform.rotation.eulerAngles.y;
		}
		else if (horiz && A && !D) {
			goal = transform.rotation.eulerAngles.y - 90f;
		}
		else if (vert && S && !W) {
			goal = transform.rotation.eulerAngles.y + 180f;
		}
		else if (horiz && D && !A) {
			goal = transform.rotation.eulerAngles.y + 90f;
		}

		dif = goal - player.transform.rotation.eulerAngles.y;
		difClamp ();
		firstTimeAdjust = (!Mathf.Approximately (dif, 0f)) && (counterZero);
		if (difBig () && (firstTimeAdjust || !counterZero)) {
			if (!movementButtonPressed ()) {
				myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), 1f, 0.1f));
				devMovementScript.setHorizRot(true);
			} else {
				devMovementScript.setHorizRot(false);
			}

			devMovementScript.adjustToCam (dif, firstTimeAdjust);
			firstTimeAdjust = false;
		} else {
			devMovementScript.setHorizRot(false);
			devMovementScript.setAdjustCounter(0);
		}

		transform.RotateAround (player.transform.position + new Vector3(0.0f, 3.0f, 0.0f), Vector3.up, movementX);
	}

	private void VerticalRotation()  {
		Vector3 axis;

		if (adjustToWalls (transform.rotation.eulerAngles.x + movementY, movementY)) {
			axis = Vector3.Cross (transform.position - devHair.transform.position, Vector3.up);
			transform.RotateAround (devHair.transform.position, axis, 40f-transform.eulerAngles.x);
			return;
		}

		movementY = Mathf.MoveTowards(movementY, Input.GetAxisRaw ("Mouse Y") * sensitivityY * Time.deltaTime, 1.0f);
		if (movementY > 180f)
			movementY -= 360f;
		else if (movementY < -180f)
			movementY += 360f;

		float total = movementY + transform.rotation.eulerAngles.x;
		if (total > 40f) {
			movementY = 40f - transform.rotation.eulerAngles.x;
			total = movementY + transform.rotation.eulerAngles.x;
		} else if (total < 2f) {
			movementY = 2f - transform.rotation.eulerAngles.x;
			total = movementY + transform.rotation.eulerAngles.x;
		}

		axis = Vector3.Cross (transform.position - devHair.transform.position, Vector3.up);
		transform.RotateAround (devHair.transform.position, axis, movementY);
		distance = Mathf.MoveTowards (distance, (initialOffset.magnitude * (35f + total) / 85f), 0.1f);
	}
	
	#endregion

	#region Combat Camera
	private void VerticalCombatRotation(){

		if (Mathf.Approximately (transform.rotation.eulerAngles.x, 30f))
			return;
		
		float movementY = 0.2f;
		float total = movementY + transform.rotation.eulerAngles.x;
		if (total > 30f) {
			movementY = 30f - transform.rotation.eulerAngles.x;
			total = movementY + transform.rotation.eulerAngles.x;
		} else if (total < 2f) {
			movementY = 2f - transform.rotation.eulerAngles.x;
			total = movementY + transform.rotation.eulerAngles.x;
		}
		Vector3 axis = Vector3.Cross (transform.position - devHair.transform.position, Vector3.up);
		transform.RotateAround (devHair.transform.position, axis, movementY);
//		distance = initialOffset.magnitude * (25f + total) / 55f;
		distance = initialOffset.magnitude * (35f + total) / 85f;
	}
		
	private void HorizontalCombatRotation()
	{
		displacement = closestEnemy - player.transform.position;
		displacement = new Vector3 (displacement.x, 0f, displacement.z);
		Vector3 perpenDif = Vector3.Normalize (Vector3.Cross (displacement, -1.0f * displacement)) * rand (1f, 0f);
		Vector3 target = closestEnemy + perpenDif;
		displacement = target - player.transform.position;
		displacement = new Vector3 (displacement.x, 0f, displacement.z);

		if (player.gameObject.GetComponent<DevMovement> ().rolling ()) {
			bool W = (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) || (Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.UpArrow));
			bool A = (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) || (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.LeftArrow));
			bool S = (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) || (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow));
			bool D = (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) || (Input.GetKeyDown (KeyCode.D) || Input.GetKeyDown (KeyCode.RightArrow));

			W = W && !S;
			S = S && !W;
			A = A && !D;
			D = D && !A;

			if (W)
				rollAngle += transform.forward;
			else if (S)
				rollAngle -= transform.forward;
			if (D)
				rollAngle += transform.right;
			else if (A)
				rollAngle -= transform.right;

			if (rollAngle == Vector3.zero)
				rollAngle = transform.forward;

			rollAngle = new Vector3 (rollAngle.x, 0f, rollAngle.z);

			player.transform.forward = Vector3.RotateTowards (player.transform.forward, rollAngle - player.transform.forward, 15f * Time.deltaTime, 0.0f); 
		}
//		else if(devCombatScript.notInCombatMove()){
		else if(!devCombatScript.isAttacking()){
			rollAngle = Vector3.zero;
			//rotate character towards closest enemy
			Invoke("adjustToEnemy", 0.1f);
		}
		//rotate camera around character according to mouse input
		movementX = Input.GetAxisRaw ("Mouse X") * sensitivityX * Time.deltaTime;
		transform.RotateAround (player.transform.position, Vector3.up, movementX);
	}

	public void doCombatRotationOffset(bool shouldHaveOffset){
		haveDevCombatOffset = shouldHaveOffset;
		adjustToEnemy ();
	}

	private void adjustToEnemy(){
		if (player.gameObject.GetComponent<DevMovement> ().rolling ())
			return;
		Vector3 oldForward = player.transform.forward;
		Vector3 offset = haveDevCombatOffset ? (player.transform.right * 0.7f) : (player.transform.right * -0.5f);

		player.transform.forward = Vector3.RotateTowards (player.transform.forward, displacement + offset, 20f * Time.deltaTime, 0.0f); 
	}

	//return position of nearest enemy
	public void getNearestEnemy(){
		if (closestEnemy != Vector3.zero && (Time.time - enemyLockOnStart) < 0.5f)
			return;
		
		Vector3 pos = player.transform.position;
		Vector3 center = new Vector3 (pos.x, 0f, pos.z);
		Vector3 halfExtents = new Vector3 (10f, 5f, 10f);
		Collider[] hits = Physics.OverlapBox (center, halfExtents);
		if (hits.Length == 0) {
			closestEnemy = Vector3.zero;
			closestEnemyObject = null;
			return;
		}
			
		bool enemyChanged = false;
		float dist = 0f;
		foreach(Collider col in hits)
		{
			if (col.gameObject.CompareTag ("Enemy")) {
				if (closestEnemy == Vector3.zero) {
					closestEnemy = col.gameObject.transform.position;
					closestEnemyObject = col.gameObject;
					dist = (closestEnemy - player.transform.position).magnitude;
				} else {
					tuple t = closer (dist, closestEnemy, col.gameObject.transform.position);
					enemyChanged = t.third ();
					dist = t.second ();
					closestEnemy = t.first ();

					if (enemyChanged)
						closestEnemyObject = col.gameObject;
				}					
			} else {
//				closestEnemy = Vector3.zero;
//				closestEnemyObject = null;
//				return;
				continue;
			}
		}
		if (enemyChanged)
			enemyLockOnStart = Time.time;

	}
	#endregion

	#region getters
	public GameObject getClosestEnemyObject(){
		return closestEnemyObject;
	}

	public Vector3 getClosestEnemy(){
		return closestEnemy;
	}

	public bool getInCombatZone(){
		return inCombatZone;
	}

	public float getCombatExitTime() {
		return combatExitTime;
	}

	#endregion

	#region setters
	public void setInCombatZone(bool _inCombatZone){
		inCombatZone = _inCombatZone;
	}
	#endregion
}