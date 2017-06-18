//
//
//
//
//using UnityEngine;
//
//public class Orbit : MonoBehaviour {
//
//	public Transform target;
//	public float angularSpeed;
//	public GameObject player;
//
//	[SerializeField][HideInInspector] 
//	private Vector3 initialOffset;
//	private Vector3 currentOffset;
//
//	[ContextMenu("Set Current Offset")]
//	private void SetCurrentOffset () {
//		if(target == null) {
//			return; 
//		}
//		initialOffset = transform.position - target.position;
//	}
//
//	private void Start () {
//		if(target == null) {
//			Debug.LogError ("Assign a target for the camera in Unity's inspector");
//		}
//		currentOffset = initialOffset;
//	}
//
//	private void LateUpdate () {
//		Debug.Log (player.ToString ());
//
//		transform.position = target.position + currentOffset;
//		float movement = Input.GetAxis ("Mouse X") * angularSpeed * Time.deltaTime;
//		if(!Mathf.Approximately (movement, 0f)) {
//			transform.RotateAround (target.position, Vector3.up, movement);
//			currentOffset = transform.position - target.position;
//		}
//	}
//}