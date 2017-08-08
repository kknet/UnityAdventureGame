using UnityEngine;

public class MouseMovement : MonoBehaviour {
	
	private bool firstTimeAdjust;
//	private float deltaVert;
//	private float deltaHoriz;

	public float sensitivityX;
	public float sensitivityY;
	public GameObject player;

	private Vector3 closePos;

	[SerializeField][HideInInspector]
	private Vector3 initialOffset;
	private Vector3 currentOffset;

	[ContextMenu("Set Current Offset")]
	private void SetCurrentOffset () {
		initialOffset = transform.position - player.transform.position;
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
	}

	private void VerticalRotation()  {
		float movementY = Input.GetAxis ("Mouse Y") * sensitivityY * Time.deltaTime;
		if (!Mathf.Approximately (movementY, 0f)) {
			float total = movementY + transform.rotation.eulerAngles.x;
			if (total > 180f)
				movementY -= 360f;
			else if (total < -180f)
				movementY += 360f;
			total = movementY + transform.rotation.eulerAngles.x;//recalculate total		
			total = Mathf.Clamp (total, -30f, 50f);//clamp it with limits
//			deltaVert = total;
			transform.rotation = Quaternion.Euler (total, 0f, 0f);//calculate resulting quaternion
		}
	}

//	private void HorizontalRotation(){
//		bool idle = player.GetComponent<DevMovement>().isIdle ();
//		float movementX = Input.GetAxis ("Mouse X") * sensitivityX * Time.deltaTime;
//		bool combating = !player.GetComponent<DevCombat> ().notInCombatMove ();
////		bool turning = player.GetComponent<DevMovement> ().turning ();
//		if (combating) {
//			if (!Mathf.Approximately (movementX, 0f)) {
//				transform.RotateAround (player.transform.position, Vector3.up, movementX);
//				firstTimeAdjust = true;
//			}
//			return;
//		}
//		if (!Mathf.Approximately (movementX, 0f)) {
//			if (idle) {
////				deltaHoriz = movementX;
//				transform.RotateAround (player.transform.position, Vector3.up, movementX);
//				firstTimeAdjust = true;
//			} else {
//				if (player.GetComponent<DevMovement> ().adjustCounter == 0) {
//						float speed = 0;
//						bool vert = !Mathf.Approximately (Input.GetAxis ("Vertical"), 0f); 
//						bool horiz = !Mathf.Approximately (Input.GetAxis ("Horizontal"), 0f); 
//						if (vert && (Input.GetKey (KeyCode.W) || (Input.GetKey (KeyCode.UpArrow)))) {
//							speed = (player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);						
//						}
//						else if (vert && (Input.GetKey (KeyCode.S) || (Input.GetKey (KeyCode.DownArrow)))) {
//							speed = (180f + player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);						
//						}
//						else if (horiz && (Input.GetKey (KeyCode.A) || (Input.GetKey (KeyCode.LeftArrow)))) {
//							speed = (90f + player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);						
//						}
//						else if (horiz && (Input.GetKey (KeyCode.D) || (Input.GetKey (KeyCode.RightArrow)))) {
//							speed = (270f + player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);						
//						}
//						transform.RotateAround (player.transform.position, Vector3.up, speed);
//				}
//			}
//		}
//		else if(!idle && (firstTimeAdjust || player.GetComponent<DevMovement> ().adjustCounter > 0))
//		{
//			float dif = transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y;
//			bool vert = !Mathf.Approximately (Input.GetAxis ("Vertical"), 0f); 
//			bool horiz = !Mathf.Approximately (Input.GetAxis ("Horizontal"), 0f); 
//			if (vert && (Input.GetKey (KeyCode.W) || (Input.GetKey (KeyCode.UpArrow)))) {
//				dif = (transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y);						
//			}
//			else if (vert && (Input.GetKey (KeyCode.S) || (Input.GetKey (KeyCode.DownArrow)))) {
//				dif = (transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y + 180f);						
//			}
//			else if (horiz && (Input.GetKey (KeyCode.A) || (Input.GetKey (KeyCode.LeftArrow)))) {
//				dif = (transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y - 90f);						
//			}
//			else if (horiz && (Input.GetKey (KeyCode.D) || (Input.GetKey (KeyCode.RightArrow)))) {
//				dif = (transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y + 90f);						
//			}
//
//			if (!Mathf.Approximately(dif,0)) {
//				player.GetComponent<DevMovement>().adjustToCam (dif, firstTimeAdjust);
//				firstTimeAdjust = false;
//			}
//		}
//	}

	private void HorizontalRotation(){
		Debug.Log (firstTimeAdjust);

		bool idle = player.GetComponent<DevMovement>().isIdle ();
		float movementX = Input.GetAxis ("Mouse X") * sensitivityX * Time.deltaTime;
		bool combating = !player.GetComponent<DevCombat> ().notInCombatMove ();
		//		bool turning = player.GetComponent<DevMovement> ().turning ();
		if (combating) {
			if (!Mathf.Approximately (movementX, 0f)) {
				transform.RotateAround (player.transform.position, Vector3.up, movementX);
				firstTimeAdjust = true;
			}
			return;
		}
		if (!Mathf.Approximately (movementX, 0f)) {
			if (idle) {
				//				deltaHoriz = movementX;
				transform.RotateAround (player.transform.position, Vector3.up, movementX);
				firstTimeAdjust = true;
			}
		}
		bool vert = !Mathf.Approximately (Input.GetAxis ("Vertical"), 0f); 
		bool horiz = !Mathf.Approximately (Input.GetAxis ("Horizontal"), 0f); 
		float dif = 0f;
		if (vert && (Input.GetKey (KeyCode.W) || (Input.GetKey (KeyCode.UpArrow)))) {
			dif = (transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y);						
			firstTimeAdjust = (!Mathf.Approximately (dif, 0f)) && (player.GetComponent<DevMovement> ().adjustCounter == 0);
		}
		else if (vert && (Input.GetKey (KeyCode.S) || (Input.GetKey (KeyCode.DownArrow)))) {
			dif = (transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y + 180f);						
			firstTimeAdjust = (!Mathf.Approximately (dif, 0f)) && (player.GetComponent<DevMovement> ().adjustCounter == 0);
		}
		else if (horiz && (Input.GetKey (KeyCode.A) || (Input.GetKey (KeyCode.LeftArrow)))) {
			dif = (transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y - 90f);						
			firstTimeAdjust = (!Mathf.Approximately (dif, 0f)) && (player.GetComponent<DevMovement> ().adjustCounter == 0);
		}
		else if (horiz && (Input.GetKey (KeyCode.D) || (Input.GetKey (KeyCode.RightArrow)))) {
			dif = (transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y + 90f);						
			firstTimeAdjust = (!Mathf.Approximately (dif, 0f)) && (player.GetComponent<DevMovement> ().adjustCounter == 0);
		}
		if (!Mathf.Approximately(dif,0)) {
			player.GetComponent<DevMovement>().adjustToCam (dif, firstTimeAdjust);
			firstTimeAdjust = false;
		}

//		if (player.GetComponent<DevMovement> ().adjustCounter == 0) {
//			float speed = 0;
//			if (vert && (Input.GetKey (KeyCode.W) || (Input.GetKey (KeyCode.UpArrow)))) {
//				speed = (player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);						
//			}
//			else if (vert && (Input.GetKey (KeyCode.S) || (Input.GetKey (KeyCode.DownArrow)))) {
//				speed = (180f + player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);						
//			}
//			else if (horiz && (Input.GetKey (KeyCode.A) || (Input.GetKey (KeyCode.LeftArrow)))) {
//				speed = (90f + player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);						
//			}
//			else if (horiz && (Input.GetKey (KeyCode.D) || (Input.GetKey (KeyCode.RightArrow)))) {
//				speed = (270f + player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);						
//			}
//			transform.RotateAround (player.transform.position, Vector3.up, speed);
			transform.RotateAround (player.transform.position, Vector3.up, movementX);
//		}
	}





	private void LateUpdate () {
		transform.position = player.transform.position + currentOffset;

//		VerticalRotation ();
		HorizontalRotation ();

//		transform.RotateAround (player.transform.position, Vector3.up, deltaHoriz);
//		transform.RotateAround (transform.position, Vector3.right, deltaVert * 0.1f);
//
//		deltaVert = 0f;
//		deltaHoriz = 0f;

		currentOffset = transform.position - player.transform.position;
	}
}