using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientMovement : NetworkBehaviour {

	private GameObject camera;
	private float moveScale = 0.1f;

	// Use this for initialization
	void Start () {
		camera = GameObject.Find ("Scene_Cardboard");
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) {
			return;
		}

		var x = Input.GetAxis ("Horizontal") * moveScale;
		var z = Input.GetAxis ("Vertical") * moveScale;

		transform.Translate (x, 0, z);
		camera.transform.Translate (x, 0, z);
	}
}
