using UnityEngine;

public class Orbit : MonoBehaviour {

	private float adjustSpeed;
	private bool firstTimeAdjust;

	public Transform target;
	public float angularSpeed;
	public GameObject player;

	[SerializeField][HideInInspector]
	private Vector3 initialOffset;
	private Vector3 currentOffset;

	[ContextMenu("Set Current Offset")]
	private void SetCurrentOffset () {
		if(target == null) {
			return; 
		}
		initialOffset = transform.position - target.position;
	}

	private void Start () {
		if(target == null) {
			Debug.LogError ("Assign a target for the camera in Unity's inspector");
		}
		currentOffset = initialOffset;
		adjustSpeed = 500.0f;
		firstTimeAdjust = false;
	}

	private void LateUpdate () {
		transform.position = target.position + currentOffset;
		bool idle = player.GetComponent<DevAnimScript>().isIdle ();
		float movementX = Input.GetAxis ("Mouse X") * angularSpeed * 0.5f * Time.deltaTime;


		float movementY = Input.GetAxis ("Mouse Y") * angularSpeed * 0.1f * Time.deltaTime * -1;
		if (!Mathf.Approximately (movementY, 0f)) {
			if (movementY + transform.eulerAngles.y >= 45f)
				transform.eulerAngles.Set (transform.eulerAngles.x, 45f, transform.eulerAngles.z);
			else if (movementY + transform.eulerAngles.y <= -45f)
				transform.eulerAngles.Set (transform.eulerAngles.x, -45f, transform.eulerAngles.z);
			else if(movementY + transform.eulerAngles.y > -45f && movementY + transform.eulerAngles.y < 45f) 
				transform.Rotate (Vector3.left * movementY); 
			else
				Debug.LogError("Y rotation messed up");
		}

//		if (!Mathf.Approximately (movementY, 0f)) {
//			if (movementY + transform.eulerAngles.y > 90f)
//				movementY = 90f - transform.eulerAngles.y;
//			else if (movementY + transform.eulerAngles.y < -90f)
//				movementY = -90f - transform.eulerAngles.y;
//			transform.Rotate (Vector3.left * movementY); 
//		}

		if (!Mathf.Approximately (movementX, 0f)) {
			if (idle) {
				//Debug.Log ("In Idle");
				transform.RotateAround (target.position, Vector3.up, movementX);
				firstTimeAdjust = true;
			} else {
				//Debug.Log ("Not In Idle");
				if (player.GetComponent<DevAnimScript> ().adjustCounter == 0) {
					if (transform.rotation.eulerAngles.y != target.rotation.eulerAngles.y) {
						float speed = (target.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);
						transform.RotateAround (target.position, Vector3.up, speed);	
					}
				}
			}
				
		}
		else if(!idle && (firstTimeAdjust || player.GetComponent<DevAnimScript> ().adjustCounter > 0))
		{
			//Debug.Log ("Need To Adjust");
			float dif = transform.eulerAngles.y - player.transform.eulerAngles.y;
			if (!Mathf.Approximately(dif,0)) {
				player.GetComponent<DevAnimScript>().adjustToCam (dif, firstTimeAdjust);
				firstTimeAdjust = false;
			}
		}
		currentOffset = transform.position - target.position;
	}
}