using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangDropScript : StateMachineBehaviour {

	const float numFrames = 72.0f;
	int frameNum = 0;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
//	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
//		animator.transform.position += new Vector3(0.338f, -0.788f, 0.150f);
//	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
//	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
//		++frameNum;
//		if(frameNum < 50)
//			GameObject.Find("DevDrake").transform.Translate(new Vector3(0.615f, -1.311f, 0.259f)/numFrames);
//	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
//	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//		GameObject.Find ("DevDrake").transform.position += animator.transform.position;
		//		animator.transform.position = Vector3.zero;
		//		GameObject.Find("DevDrake").transform.rotation = animator.bodyRotation;
		//		animator.transform.parent.gameObject.transform.position = animator.transform.position;
//	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
//	override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
//		GameObject.Find("DevDrake").transform.Translate(new Vector3(0.615f, -1.311f, 0.259f) * Time.deltaTime);
//	}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}

//0.338, -0.788, 0.150
//rotate 180 degrees around y