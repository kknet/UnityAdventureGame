using UnityEngine;

public class MouseMovement : MonoBehaviour {


	public Animator myAnimator;
	public float sensitivityX;
	public float sensitivityY;
	public GameObject player;
	public GameObject devHair;
	public bool inCombat;

	private bool firstTimeAdjust;
	private float dif;
	private float goal;
	private float distance;
	private Vector3 oldEnemy;
	private Vector3 displacement;
	private Vector3 rollAngle;

	[SerializeField][HideInInspector]
	private Vector3 initialOffset;
	private Vector3 currentOffset;

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
				
	private void Start () {
		if(player == null) {
			Debug.LogError ("Assign a player for the camera in Unity's inspector");
		}
		currentOffset = initialOffset;
		firstTimeAdjust = false;
		dif = 0f;
		goal = player.transform.rotation.eulerAngles.y;
		distance = initialOffset.magnitude;
		oldEnemy = Vector3.zero;
		displacement = Vector3.zero;
		inCombat = false;
		rollAngle = Vector3.zero;
	}

	private void VerticalRotation()  {
		float movementY = Input.GetAxisRaw ("Mouse Y") * sensitivityY * Time.deltaTime;
		if (movementY > 180f)
			movementY -= 360f;
		else if (movementY < -180f)
			movementY += 360f;

		float total = movementY + transform.rotation.eulerAngles.x;
		if (total > 30f) {
			movementY = 30f - transform.rotation.eulerAngles.x;
			total = movementY + transform.rotation.eulerAngles.x;
		} else if (total < 10f) {
			movementY = 10f - transform.rotation.eulerAngles.x;
			total = movementY + transform.rotation.eulerAngles.x;
		}
		Vector3 axis = Vector3.Cross (transform.position - devHair.transform.position, Vector3.up);
		transform.RotateAround (devHair.transform.position, axis, movementY);
	
		distance = initialOffset.magnitude * (25f + total) / 75f;
	}

	private float rand(float a, float b){
		return UnityEngine.Random.Range (a, b);
	}

	private bool Approx(Vector3 a, Vector3 b){
		return Mathf.Approximately (a.x, b.x) && Mathf.Approximately (a.z, b.z);
	}


	private void HorizontalCombatRotation(Vector3 closestEnemy)
	{

		//if the position of the closest enemy changed
		if (!Approx(closestEnemy, oldEnemy))
		{
			//choose a random position that is near the enemy
			//and using this random position, and the player's position
			//calculate the desired rotation of the player
			displacement = closestEnemy - player.transform.position;
			displacement = new Vector3 (displacement.x, 0f, displacement.z);
			Vector3 perpenDif = Vector3.Normalize (Vector3.Cross (displacement, -1.0f * displacement)) * rand (1f, -1f);
			Vector3 target = closestEnemy + perpenDif;
			displacement = target - player.transform.position;
			displacement = new Vector3 (displacement.x, 0f, displacement.z);
		}


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
				rollAngle += Vector3.forward;
			else if (S)
				rollAngle -= Vector3.forward;
			if (D)
				rollAngle += Vector3.right;
			else if (A)
				rollAngle -= Vector3.right;

			if (rollAngle == Vector3.zero)
				rollAngle = Vector3.forward;

			player.transform.forward = Vector3.RotateTowards (player.transform.forward, rollAngle - player.transform.forward, 15f * Time.deltaTime, 0.0f); 
		}
		else {
			rollAngle = Vector3.zero;
			//rotate character towards closest enemy
			Invoke("adjustToEnemy", 1f);
		}
		//rotate camera around character according to mouse input
		float movementX = Input.GetAxisRaw ("Mouse X") * sensitivityX * Time.deltaTime;
		transform.RotateAround (player.transform.position, Vector3.up, movementX);
	}

	private void adjustToEnemy(){
		if (player.gameObject.GetComponent<DevMovement> ().rolling ())
			return;
		Vector3 oldForward = player.transform.forward;
		player.transform.forward = Vector3.RotateTowards (player.transform.forward, displacement, 10f * Time.deltaTime, 0.0f); 
		if ((oldForward-player.transform.forward).magnitude > 0.1f) {
			myAnimator.SetFloat ("VSpeed", Mathf.MoveTowards (myAnimator.GetFloat ("VSpeed"), 1f, Time.deltaTime*2f));
			player.transform.Translate (player.transform.forward * Time.deltaTime * 2f);
			player.GetComponent<DevMovement> ().horizRot = true;
		} else {
			player.GetComponent<DevMovement> ().horizRot = false;
		}
	}

	private void HorizontalRotation(){
		bool idle = player.GetComponent<DevMovement>().isIdle ();
		float movementX = Input.GetAxisRaw ("Mouse X") * sensitivityX * Time.deltaTime;
		bool combating = !player.GetComponent<DevCombat> ().notInCombatMove ();
		bool counterZero = (player.GetComponent<DevMovement> ().adjustCounter == 0);
		bool camMoved = !Mathf.Approximately (movementX, 0f);
		AnimatorStateInfo anim = myAnimator.GetCurrentAnimatorStateInfo (0);
		bool jumping = anim.IsTag("Jumps");

		if (counterZero && camMoved) {
			if (combating || idle) {
				transform.RotateAround (player.transform.position + new Vector3(0.0f, 3.0f, 0.0f), Vector3.up, movementX);
				firstTimeAdjust = true;
				return;
			}
		}

//		if ((counterZero) && combating) {
//			if (camMoved) {
//
//			}
//			return;
//		}
//		if (camMoved) {
//			if (counterZero && idle) {
//				transform.RotateAround (player.transform.position, Vector3.up, movementX);
//				firstTimeAdjust = true;
//				return;
//			}
//		}
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
				player.GetComponent<DevMovement> ().horizRot = true;
			} else {
				player.GetComponent<DevMovement> ().horizRot = false;
			}

			player.GetComponent<DevMovement> ().adjustToCam (dif, firstTimeAdjust);
			firstTimeAdjust = false;
		} else {
			player.GetComponent<DevMovement> ().horizRot = false;
			player.GetComponent<DevMovement> ().adjustCounter = 0;
		}

		transform.RotateAround (player.transform.position + new Vector3(0.0f, 3.0f, 0.0f), Vector3.up, movementX);
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

	//return position of nearest enemy
	public Vector3 nearestEnemy(){
		Vector3 pos = player.transform.position;
		Vector3 center = new Vector3 (pos.x, 0f, pos.z);
		Vector3 halfExtents = new Vector3 (20f, 5f, 20f);
		Collider[] hits = Physics.OverlapBox (center, halfExtents);
		if (hits.Length == 0)
			return Vector3.zero;

		Vector3 closestEnemy = Vector3.zero;
		float dist = 0f;
		foreach(Collider col in hits)
		{
			if (col.gameObject.CompareTag ("Enemy")) {
				if (closestEnemy == Vector3.zero) {
					closestEnemy = col.gameObject.transform.position;
					dist = (closestEnemy - player.transform.position).magnitude;
				} else {
					tuple t = closer (dist, closestEnemy, col.gameObject.transform.position);
					dist = t.second ();
					closestEnemy = t.first ();
				}					
			}
		}
		return closestEnemy;
	}

	class tuple {
		Vector3 a;
		float b;
		public tuple(Vector3 A, float B) {
			a = A;
			b = B;
		}
		public Vector3 first() { return a; }
		public float second() { return b; }
	}

	private tuple closer(float dist, Vector3 oldGuy, Vector3 newGuy){
		float difference = (newGuy - player.transform.position).magnitude;
		if (difference < dist)
			return new tuple (newGuy, difference);
		return new tuple (oldGuy, dist);
	}

	private void Update(){
		transform.position = player.transform.position + currentOffset;
		Vector3 enemy = nearestEnemy (); 
		inCombat = (enemy != Vector3.zero);
		if (inCombat) {
			HorizontalCombatRotation (enemy);				
		} else {
			VerticalRotation ();
			HorizontalRotation ();		
		}
		currentOffset = (transform.position - player.transform.position).normalized * distance;
	}

//	private void LateUpdate () {
//		transform.position = player.transform.position + currentOffset;
//		VerticalRotation ();
//		HorizontalRotation ();
//		currentOffset = (transform.position - player.transform.position).normalized * distance;
//	}
}