using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientMovement : NetworkBehaviour {

	private GameObject cardboard;
	private GameObject camera;
	private float moveScale = 0.1f;

	// Use this for initialization
	void Start () {
		cardboard = GameObject.Find ("Scene_Cardboard");
		camera = cardboard.transform.GetChild (0).gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) {
			return;
		}

		//var x = Input.GetAxis ("Horizontal") * moveScale;
		//var z = Input.GetAxis ("Vertical") * moveScale;

		Vector3 LeftRight		= Input.GetAxis("Vertical") * Vector3.Normalize(camera.transform.forward) * moveScale;
		Vector3 ForwardBack 	= Input.GetAxis("Horizontal") * Vector3.Normalize(camera.transform.right) * moveScale;


		transform.Translate (LeftRight);
		transform.Translate (ForwardBack);

		cardboard.transform.Translate (LeftRight);
		cardboard.transform.Translate (ForwardBack);
	}
}
