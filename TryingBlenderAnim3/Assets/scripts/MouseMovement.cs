using UnityEngine;

public class MouseMovement : MonoBehaviour {
	
	private bool firstTimeAdjust;
//	private float deltaVert;
//	private float deltaHoriz;

	public float sensitivityX;
	public float sensitivityY;
	public GameObject player;

	[SerializeField][HideInInspector]
	private Vector3 initialOffset;
	private Vector3 currentOffset;

	[ContextMenu("Set Current Offset")]
	private void SetCurrentOffset () {
		initialOffset = transform.position - player.transform.position;
	}

	private void Start () {
		if(player == null) {
			Debug.LogError ("Assign a player for the camera in Unity's inspector");
		}
		currentOffset = initialOffset;
		firstTimeAdjust = false;
//		deltaVert = 0f;
//		deltaVert = 0f;
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

	private void HorizontalRotation(){
		bool idle = player.GetComponent<DevMovement>().isIdle ();
		bool combating = !player.GetComponent<DevCombat>().notInCombatMove();
		float movementX = Input.GetAxis ("Mouse X") * sensitivityX * Time.deltaTime;
//		if (combating) {
//			if (!Mathf.Approximately (movementX, 0f)) {
//				transform.RotateAround (player.transform.position, Vector3.up, movementX);
//				firstTimeAdjust = true;
//			}
//			return;
//		}
		if (!Mathf.Approximately (movementX, 0f)) {
			if (idle) {
//				deltaHoriz = movementX;
				transform.RotateAround (player.transform.position, Vector3.up, movementX);
				firstTimeAdjust = true;
			} else {
				if (player.GetComponent<DevMovement> ().adjustCounter == 0) {
					if (transform.rotation.eulerAngles.y != player.transform.rotation.eulerAngles.y) {
						float speed = (player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);
//						deltaHoriz = speed;
						transform.RotateAround (player.transform.position, Vector3.up, speed);	
					}
				}
			}
		}
		else if(!idle && (firstTimeAdjust || player.GetComponent<DevMovement> ().adjustCounter > 0))
		{
			//Debug.Log ("Need To Adjust");
			float dif = transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y;
			if (!Mathf.Approximately(dif,0)) {
				player.GetComponent<DevMovement>().adjustToCam (dif, firstTimeAdjust);
				firstTimeAdjust = false;
			}
		}
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