using UnityEngine;
using System.Collections;

public class ClientSelection : MonoBehaviour {

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