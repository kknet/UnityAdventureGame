//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
//public class CameraAdjust : MonoBehaviour {
//
//	// The target we are following
//	public Transform target;
//	//the position the player looks at (like, on a wall some distance away, or on the ground next to him)
//	public Vector3 lookAtTarget;
//	public float defaultMinDistance = 0.5f;
//	public float defaultMaxDistance = 5f;
//	public float defaultMaxHeight = 3f;
//	// The distance in the x-z plane to the target
//	public float distance = 6.0f;
//	public float minDistance = 1.0f;
//	public float maxDistance = 6.0f;
//	// the height we want the camera to be above the target
//	public float maxHeight = 3.0f;
//	public float height = 3.0f;
//
//	public float dampingFactor = 3.0f;
//	public bool collision = false;
//	public LayerMask mask;
//	private float maximumY;
//	private ValueWrapper<float> camAnchorRotX = new ValueWrapper<float>(0f);
//	public float horizontalOffsetDefault = 0.22f;
//	public float horizontalOffset;
//	public bool damping = false;
//
//
//	void Start() {
//		//CreateFrustrumBox();
//		maximumY = Manager.player.GetComponent<PlayerControl>().maximumY;
//	}
//
//	void Update() {
//		camAnchorRotX.Value = target.localEulerAngles.x;
//		if(camAnchorRotX.Value > 180) {
//			camAnchorRotX.Value -= 360;
//		}
//		CastFrustrumRays();
//	}
//
//	void FixedUpdate () {
//		// Early out if we don't have a target
//		if (target == null) {
//			return;
//		}
//
//		//height = maxHeight * (1 - (Mathf.Abs(camAnchorRotX.Value) / maximumY));
//		lookAtTarget = target.position + Vector3.up * height + target.right * horizontalOffset;
//		Vector3 wantedPosition = lookAtTarget - target.forward * distance;
//		if(damping == true) {
//			transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * dampingFactor);
//		}
//		else {
//			transform.position = wantedPosition;
//		}
//
//		// Always look at the target
//		transform.LookAt (lookAtTarget);
//
//	}
//
//	void CreateFrustrumBox() {
//
//		float h = Mathf.Tan(GetComponent<Camera>().fov * Mathf.Deg2Rad * 0.5f) * GetComponent<Camera>().nearClipPlane * 2f;
//		GetComponent<Collider>().transform.localScale = new Vector3(h * GetComponent<Camera>().aspect, h, 0.01f);
//		GetComponent<BoxCollider>().center = new Vector3(0, 0, 100);
//	}
//
//	void CastFrustrumRays() {
//
//		float distanceToCamera = Vector3.Distance(lookAtTarget, transform.position);
//
//		Vector3 pos00_Cam = GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, GetComponent<Camera>().nearClipPlane));
//		Vector3 pos01_Cam = GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 1, GetComponent<Camera>().nearClipPlane));
//		Vector3 pos11_Cam = GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 1, GetComponent<Camera>().nearClipPlane));
//		Vector3 pos10_Cam = GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 0, GetComponent<Camera>().nearClipPlane));
//
//		//float forwardOffset = maxDistance - camera.nearClipPlane - (maxHeight - maxHeight * (1 - (Mathf.Abs(camAnchorRotX.Value) / maximumY)));
//		float forwardOffset = Mathf.Clamp(distanceToCamera, minDistance, maxDistance);
//
//		if(forwardOffset <= 0) {
//			forwardOffset = 0.1f;
//		}
//
//		Vector3 pos00_Target = pos00_Cam + transform.forward * forwardOffset;
//		Vector3 pos01_Target = pos01_Cam + transform.forward * forwardOffset;
//		Vector3 pos11_Target = pos11_Cam + transform.forward * forwardOffset;
//		Vector3 pos10_Target = pos10_Cam + transform.forward * forwardOffset;
//
//		Ray ray00 = new Ray(pos00_Target, -transform.forward);
//		Ray ray01 = new Ray(pos01_Target, -transform.forward);
//		Ray ray11 = new Ray(pos11_Target, -transform.forward);
//		Ray ray10 = new Ray(pos10_Target, -transform.forward);
//
//		RaycastHit hit00 = new RaycastHit();
//		RaycastHit hit01 = new RaycastHit();
//		RaycastHit hit11 = new RaycastHit();
//		RaycastHit hit10 = new RaycastHit();
//
//		Debug.DrawLine(pos00_Target, pos01_Target);
//		Debug.DrawLine(pos01_Target, pos11_Target);
//		Debug.DrawLine(pos11_Target, pos10_Target);
//		Debug.DrawLine(pos10_Target, pos00_Target);
//
//		Ray[] rays = new Ray[] {ray00, ray01, ray11, ray10};
//		RaycastHit[] hits = new RaycastHit[] {hit00, hit01, hit11, hit10};
//		Vector3[] positions_Target = new Vector3[] {pos00_Target, pos01_Target, pos11_Target, pos10_Target};
//		Vector3[] positions_Cam = new Vector3[] {pos00_Cam, pos01_Cam, pos11_Cam, pos10_Cam};
//
//		int i = 0;
//
//		if(Physics.Raycast(ray00, out hit00, maxDistance, mask)) {
//			collision = true;
//			Debug.DrawRay(pos00_Target, pos00_Cam - pos00_Target, Color.red);
//		}
//		else if(Physics.Raycast(ray01, out hit01, maxDistance, mask)) {
//			collision = true;
//			Debug.DrawRay(pos01_Target, pos01_Cam - pos01_Target, Color.red);
//		}
//		else if(Physics.Raycast(ray11, out hit11, maxDistance, mask)) {
//			collision = true;
//			Debug.DrawRay(pos11_Target, pos11_Cam - pos11_Target, Color.red);
//		}
//		else if(Physics.Raycast(ray10, out hit10, maxDistance, mask)) {
//			collision = true;
//			Debug.DrawRay(pos10_Target, pos10_Cam - pos10_Target, Color.red);
//		}
//		else {
//			collision = false;
//		}
//
//		if(collision == false) {
//			for(i = 0; i < 4; i++) {
//				Debug.DrawRay(positions_Target[i], positions_Cam[i] - positions_Target[i], Color.green);
//			}
//		}
//
//		float minCollisionDistance = maxDistance;
//
//		if(collision == true) {
//			float distance00 = Vector3.Distance(lookAtTarget, hit00.point);
//			float distance01 = Vector3.Distance(lookAtTarget, hit01.point);
//			float distance11 = Vector3.Distance(lookAtTarget, hit11.point);
//			float distance10 = Vector3.Distance(lookAtTarget, hit10.point);
//			minCollisionDistance = Mathf.Min(distance00, distance01, distance11, distance10);
//			minCollisionDistance = Mathf.Clamp(minCollisionDistance, minDistance, maxDistance);
//		}
//		else {
//			minCollisionDistance = maxDistance;
//		}
//
//		distance = minCollisionDistance - Mathf.Abs(0 + horizontalOffset) * 2;
//	}
//}