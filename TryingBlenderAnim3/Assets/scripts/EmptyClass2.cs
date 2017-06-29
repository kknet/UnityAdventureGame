//using UnityEngine;
//
//public class Orbit : MonoBehaviour {
//
//	private bool firstTimeAdjust;
//	public float sensitivityX;
//	public float sensitivityY;
//	public GameObject player;
//
//	[SerializeField][HideInInspector]
//	private Vector3 initialOffset;
//	private Vector3 currentOffset;
//
//	[ContextMenu("Set Current Offset")]
//	private void SetCurrentOffset () {
//		initialOffset = transform.position - player.transform.position;
//	}
//
//	private void Start () {
//		if(player == null) {
//			Debug.LogError ("Assign a player for the camera in Unity's inspector");
//		}
//		currentOffset = initialOffset;
//		firstTimeAdjust = false;
//	}
//
//	private void VerticalRotation()  {
//		float movementY = Input.GetAxis ("Mouse Y") * sensitivityY * Time.deltaTime;
//		if (!Mathf.Approximately (movementY, 0f)) {
//			float total = movementY + transform.rotation.eulerAngles.x;
//			if (total > 180f)
//				movementY -= 360f;
//			else if (total < -180f)
//				movementY += 360f;
//			total = movementY + transform.rotation.eulerAngles.x;//recalculate total		
//			total = Mathf.Clamp (total, -30f, 50f);//clamp it with limits
//			transform.rotation = Quaternion.Euler (total, 0f, 0f);//calculate resulting quaternion
//		}
//	}
//
//	private void HorizontalRotation(){
//		bool idle = player.GetComponent<DevAnimScript>().isIdle ();
//		float movementX = Input.GetAxis ("Mouse X") * sensitivityX * Time.deltaTime;
//		if (!Mathf.Approximately (movementX, 0f)) {
//			if (idle) {
//				//Debug.Log ("In Idle");
//				transform.RotateAround (player.transform.position, Vector3.up, movementX);
//				firstTimeAdjust = true;
//			} else {
//				//Debug.Log ("Not In Idle");
//				if (player.GetComponent<DevAnimScript> ().adjustCounter == 0) {
//					if (transform.rotation.eulerAngles.y != player.transform.rotation.eulerAngles.y) {
//						float speed = (player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);
//						transform.RotateAround (player.transform.position, Vector3.up, speed);	
//					}
//				}
//			}
//		}
//		else if(!idle && (firstTimeAdjust || player.GetComponent<DevAnimScript> ().adjustCounter > 0))
//		{
//			//Debug.Log ("Need To Adjust");
//			float dif = transform.rotation.eulerAngles.y - player.transform.rotation.eulerAngles.y;
//			if (!Mathf.Approximately(dif,0)) {
//				player.GetComponent<DevAnimScript>().adjustToCam (dif, firstTimeAdjust);
//				firstTimeAdjust = false;
//			}
//		}
//	}
//
//
//	private void LateUpdate () {
//		transform.position = player.transform.position + currentOffset;
//		VerticalRotation ();
//		HorizontalRotation ();
//		currentOffset = transform.position - player.transform.position;
//	}
//}