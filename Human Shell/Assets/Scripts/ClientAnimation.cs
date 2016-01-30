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
	private Animator clientAnim;
	private Vector3 leftHand, rightHand;

	// Use this for initialization
	void Start () {
		mySocket 	= new TcpClient ("linux3.cs.uchicago.edu", 9000);
		theStream 	= mySocket.GetStream();
		theWriter 	= new StreamWriter(theStream);
		theReader 	= new StreamReader(theStream);

		clientAnim = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update() {
		while (theStream.DataAvailable) {
			string reading = theReader.ReadLine ();
			string[] parts = reading.Split (new Char[] { ' ' });
			Vector3 pos = new Vector3(float.Parse(parts [2]), float.Parse(parts [3]), float.Parse(parts [4]))/1000.0f;

			switch (parts [0]) {
			case "head":
				break;
			case "lefthand":
				leftHand = pos;
				break;
			case "righthand":
				rightHand = pos;
				break;
			case "leftshoulder":
				break;
			case "rightshoulder":
				break;
			}
		}
	}

	void OnAnimatorIK(int layerIndex) {
		clientAnim.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1);
		clientAnim.SetIKPosition(AvatarIKGoal.LeftHand, leftHand);

		clientAnim.SetIKPositionWeight (AvatarIKGoal.RightHand, 1);
		clientAnim.SetIKPosition(AvatarIKGoal.RightHand, rightHand);
	}
}
