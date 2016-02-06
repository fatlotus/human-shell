using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {

	void OnEnable () {
		Cardboard.SDK.OnTrigger += TriggerPulled;
	}

	void OnDisable () {
		Cardboard.SDK.OnTrigger -= TriggerPulled;
	}

	void TriggerPulled () {
		Debug.Log("The trigger was pulled!");
	}
}