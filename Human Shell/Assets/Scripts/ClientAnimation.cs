using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Net.Sockets;
using System;

public class ClientAnimation : NetworkBehaviour {

	TcpClient 		mySocket;
	NetworkStream 	theStream;
	StreamWriter 	theWriter;
	StreamReader 	theReader;

	private GameObject cardboard;
	private GameObject camera;

	private Animator clientAnim;
	private Vector3 leftHand, rightHand, torso, leftFoot, rightFoot;
	private Vector3 leftShoulder, rightShoulder, leftHip, rightHip;

	private float moveScale = 0.1f;

	// Use this for initialization
	void Start () {
		mySocket 	= new TcpClient ("linux3.cs.uchicago.edu", 9000);
		theStream 	= mySocket.GetStream();
		theWriter 	= new StreamWriter(theStream);
		theReader 	= new StreamReader(theStream);

		clientAnim = GetComponent<Animator>();

		cardboard = GameObject.Find ("Scene_Cardboard");
		camera = cardboard.transform.GetChild (0).gameObject;
	}

	// Update is called once per frame
	void Update() {
		while (theStream.DataAvailable) {
			string reading = theReader.ReadLine ();
			string[] parts = reading.Split (new Char[] { ' ' });
			Vector3 pos = new Vector3(float.Parse(parts [2]), float.Parse(parts [3]), float.Parse(parts [4]))/1000.0f;
			pos.x *= -1;
			pos.z *= -1;

			switch (parts [0]) {
			case "head":
				break;
			case "lefthand":
				leftHand = pos * 0.5f + leftHand * 0.5f;
				break;
			case "righthand":
				rightHand = pos * 0.5f + rightHand * 0.5f;
				break;
			case "leftshoulder":
				leftShoulder = pos;
				break;
			case "rightshoulder":
				rightShoulder = pos;
				break;
			case "leftelbow":
				break;
			case "rightelbow":
				break;
			case "leftknee":
				break;
			case "rightknee":
				break;
			case "lefthip":
				leftHip = pos;
				break;
			case "righthip":
				rightHip = pos;
				break;
			case "torso":
				torso = pos;
				break;
			case "leftfoot":
				leftFoot = pos;
				break;
			case "rightfoot":
				rightFoot = pos;
				break;
			default:
				print (parts [0]);
				cardboard.transform.position = new Vector3 (100, 100, 100);
				break;
			}
		}

		if (!isLocalPlayer) {
			return;
		}

		//var x = Input.GetAxis ("Horizontal") * moveScale;
		//var z = Input.GetAxis ("Vertical") * moveScale;

		Vector3 LeftRight		= Input.GetAxis("Vertical") * Vector3.Normalize(camera.transform.forward) * moveScale;
		Vector3 ForwardBack 	= Input.GetAxis("Horizontal") * Vector3.Normalize(camera.transform.right) * moveScale;

		cardboard.transform.Translate (LeftRight);
		cardboard.transform.Translate (ForwardBack);

		Vector3 shoulders = (leftShoulder + rightShoulder) / 2;
		Vector3 hips = (leftHip + rightHip) / 2;
		Vector3 motion = (shoulders - hips) * 2.0f;
		motion.y = 0;

		print (motion);
	
		if (motion.magnitude >= 0.05)
			cardboard.transform.Translate (motion);

		transform.position = cardboard.transform.position + new Vector3(0, -1.3f, 0.2f);
	}

	void OnAnimatorIK(int layerIndex) {
		clientAnim.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1);
		clientAnim.SetIKPosition(AvatarIKGoal.LeftHand, clientAnim.bodyPosition + (rightHand - torso)); // <- intentional

		clientAnim.SetIKPositionWeight (AvatarIKGoal.RightHand, 1);
		clientAnim.SetIKPosition(AvatarIKGoal.RightHand, clientAnim.bodyPosition + (leftHand - torso));

		clientAnim.SetIKPositionWeight (AvatarIKGoal.LeftFoot, 1);
		clientAnim.SetIKPosition(AvatarIKGoal.LeftFoot, clientAnim.bodyPosition + (rightFoot - torso)); // <- intentional

		clientAnim.SetIKPositionWeight (AvatarIKGoal.RightFoot, 1);
		clientAnim.SetIKPosition(AvatarIKGoal.RightFoot, clientAnim.bodyPosition + (leftFoot - torso));

		if (!isLocalPlayer)
			return;
		
		clientAnim.bodyRotation = Quaternion.Euler (
			0,
			camera.transform.rotation.eulerAngles.y,
			0
			);

		clientAnim.SetLookAtPosition (clientAnim.bodyPosition + new Vector3(0f, 0.5f, 0f) + camera.transform.forward);
		clientAnim.SetLookAtWeight (1);
	}
}
