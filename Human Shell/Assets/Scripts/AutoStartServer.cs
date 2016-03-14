using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class AutoStartServer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (Application.platform == RuntimePlatform.Android) {
			GetComponent<NetworkManager> ().networkAddress = "spearow.cs.uchicago.edu";
			GetComponent<NetworkManager> ().StartClient ();
		} else {
			GetComponent<NetworkManager> ().StartHost ();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
