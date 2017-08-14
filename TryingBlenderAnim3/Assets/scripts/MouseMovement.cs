using UnityEngine;

public class MouseMovement : MonoBehaviour {


	public Animator myAnimator;
	public float sensitivityX;
	public float sensitivityY;
	public GameObject player;
	public GameObject devHair;

	private bool firstTimeAdjust;
	private Vector3 closePos;
	private float dif;
	private float goal;
	private float distance;

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


	//	private bool Approx(Vector3 a, Vector3 b){
	//		return Mathf.Approximately (a.x, b.x) && Mathf.Approximately (a.y, b.y) && Mathf.Approximately (a.z, b.z);
	//	}
	//
	//	public void ZoomIn() {
	////		Vector3 dir = transform.position - closePos;
	//		transform.position.Set (closePos.x, closePos.y, closePos.z);
	//		currentOffset = transform.position - player.transform.position;
	//	}
	//
	//	public void ZoomOut(Vector3 direction) {
	//		Vector3 current = transform.position - player.transform.position;
	//		if (!Approx (current, initialOffset)) {
	//			transform.Translate (direction);		
	//		}
	//	}

	private void Start () {
		if(player == null) {
			Debug.LogError ("Assign a player for the camera in Unity's inspector");
		}
		currentOffset = initialOffset;
		firstTimeAdjust = false;
		closePos = new Vector3 (0f, 1.54f, -1.425f);
		dif = 0f;
		goal = player.transform.rotation.eulerAngles.y;
		distance = initialOffset.magnitude;
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
				transform.RotateAround (player.transform.position, Vector3.up, movementX);
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



	private void LateUpdate () {
		transform.position = player.transform.position + currentOffset;
		VerticalRotation ();
		HorizontalRotation ();
		currentOffset = (transform.position - player.transform.position).normalized * distance;
	}
}