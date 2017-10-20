using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDuringJumpAttack : StateMachineBehaviour {

	private GameObject dev;
	private GameObject currentEnemy;
	private float lerpT;
	private float desiredOffset;
	private bool doneLerping;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
//	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
//		lerpT = 0f;
//		dev = GameObject.Find ("DevDrake");
//		currentEnemy = dev.GetComponent<DevCombat> ().getCurrentEnemy ();
//		desiredOffset = 1.0f;
//		doneLerping = false;
//	}
//	
//	private Vector3 getEnemyPos(){
//		return currentEnemy.transform.position;
//	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
//	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
//	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
//	override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
//		lerpT += (Time.deltaTime * 0.5f);
//
//		if (doneLerping)
//			return;
//
//		if (lerpT >= 1.0f){
//			doneLerping = true;
//			return;
//		}
//
//		Vector3 totalVectorOffset = getEnemyPos() - dev.transform.position;
//		totalVectorOffset = new Vector3 (totalVectorOffset.x, 0f, totalVectorOffset.z);
//		float totalOffset = totalVectorOffset.magnitude;
//		float remaining = totalOffset - desiredOffset;
//		if (Mathf.Abs (remaining) < 0.01f) {
//			doneLerping = true;
//		}
//		else {
//			Vector3 deltaPos = totalVectorOffset.normalized * remaining;
//			dev.transform.position = Vector3.Lerp (dev.transform.position, dev.transform.position + deltaPos, lerpT);
//			//			myAnimator.SetFloat ("VSpeed", remaining); 
//		}	
//	}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
