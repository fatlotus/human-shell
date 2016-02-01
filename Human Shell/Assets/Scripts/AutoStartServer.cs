using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class AutoStartServer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<NetworkManager> ().StartHost ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
