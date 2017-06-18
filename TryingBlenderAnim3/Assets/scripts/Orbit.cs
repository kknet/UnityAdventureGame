using UnityEngine;

public class Orbit : MonoBehaviour {

	private float adjustSpeed;
	private bool needToAdjust;

	public Transform target;
	public float angularSpeed;
	public GameObject player;

	[SerializeField][HideInInspector]
	private Vector3 initialOffset;
	private Vector3 currentOffset;

//	[SerializeField][HideInInspector]
//	private Quaternion initialRot;
//	private Quaternion currentRot;


	[ContextMenu("Set Current Offset")]
	private void SetCurrentOffset () {
		if(target == null) {
			return; 
		}
		initialOffset = transform.position - target.position;
		//initialRot = transform.rotation;
	}

	private void Start () {
		if(target == null) {
			Debug.LogError ("Assign a target for the camera in Unity's inspector");
		}
		currentOffset = initialOffset;
		//currentRot = initialRot;
		adjustSpeed = 500.0f;
		needToAdjust = false;
	}

	private void LateUpdate () {
		transform.position = target.position + currentOffset;
		bool idle = player.GetComponent<DevAnimScript>().isIdle ();
		float movementX = Input.GetAxis ("Mouse X") * angularSpeed * Time.deltaTime;


//		float movementY = Input.GetAxis ("Mouse Y") * angularSpeed * 0.2f * Time.deltaTime;
//		if (!Mathf.Approximately (movementY, 0f)) {
//			transform.RotateAround (target.position, Vector3.right, movementY); 
//		}

		if (!Mathf.Approximately (movementX, 0f)) {
			if (idle) {
				Debug.Log ("In Idle");
				transform.RotateAround (target.position, Vector3.up, movementX);
				needToAdjust = true;
			} else {
				Debug.Log ("Not In Idle");

				if (transform.rotation.eulerAngles.y != target.rotation.eulerAngles.y) {
					float speed = (target.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);
					transform.RotateAround (target.position, Vector3.up, speed);	
				}

//				if (transform.rotation.eulerAngles.y < target.rotation.eulerAngles.y) {
//					float speed = (target.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);
//					transform.RotateAround (target.position, Vector3.up, speed);	
//				}
//				else if (transform.rotation.eulerAngles.y > target.rotation.eulerAngles.y) {
//					float speed = (transform.rotation.eulerAngles.y - target.rotation.eulerAngles.y);
//					transform.RotateAround (target.position, Vector3.down, speed);	
//				}
				//transform.rotation.Set (player.transform.rotation.x, player.transform.rotation.y, player.transform.rotation.z, player.transform.rotation.w);
			}
		}
		else if(!idle && needToAdjust)
		{
			Debug.Log ("Need To Adjust");

			float dif = transform.rotation.eulerAngles.y - target.rotation.eulerAngles.y;
			if (dif != 0) {
				player.GetComponent<DevAnimScript>().adjustToCam (dif, needToAdjust);
				//player.transform.Rotate (Vector3.up * dif);
			}



//			if (transform.rotation.eulerAngles.y > target.rotation.eulerAngles.y) {
//				float speed = (target.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);
//				player.transform.Rotate (Vector3.up * adjustSpeed * Time.deltaTime);	
//			} else if (transform.rotation.eulerAngles.y < target.rotation.eulerAngles.y) {
//				player.transform.Rotate (Vector3.down * adjustSpeed * Time.deltaTime);
//			} else
//				needToAdjust = false;

			//transform.rotation.Set (player.transform.rotation.x, player.transform.rotation.y, player.transform.rotation.z, player.transform.rotation.w);			
		}
		currentOffset = transform.position - target.position;
	}
}