using UnityEngine;
using System.Collections;

public class AnimatorGestures : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	struct ClientData {
		int kinect_id;
		Vector3 head,
			lefthand, righthand,
			leftshoulder, rightshoulder,
			leftelbow, rightelbow,
			leftknee, rightknee,
			lefthip, righthip,
			torso,
			leftfoot, rightfoot;
	}

	bool ClickPosition (ClientData kin) {
		if ((kin.lefthand - kin.head).magnitude <= 0.5) {
			return true;
		}

		return false;
	}
}
