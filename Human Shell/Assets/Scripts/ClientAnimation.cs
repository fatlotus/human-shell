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

	// Use this for initialization
	void Start () {
		print ("Attached script.");
		mySocket 	= new TcpClient ("linux3.cs.uchicago.edu", 9000);
		theStream 	= mySocket.GetStream();
		theWriter 	= new StreamWriter(theStream);
		theReader 	= new StreamReader(theStream);

		clientAnim = GetComponent<Animator>();
	}

	// Update is called once per frame
	void OnAnimatorIK() {
		while (theStream.DataAvailable) {
			string reading = theReader.ReadLine ();
			print (reading);
			string[] parts = reading.Split (new Char[] { ' ' });
			Vector3 pos = new Vector3(float.Parse(parts [2]), float.Parse(parts [3]), float.Parse(parts [4]))/1000.0f;


			switch (parts [0]) {
				case "head":
					break;
				case "lefthand":
					clientAnim.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1);
					clientAnim.SetIKPosition(AvatarIKGoal.LeftHand, pos);
					break;
				case "righthand":
					clientAnim.SetIKPositionWeight (AvatarIKGoal.RightHand, 1);
					clientAnim.SetIKPosition(AvatarIKGoal.RightHand, pos);
					break;
				case "leftshoulder":
					break;
				case "rightshoulder":
					break;
			}
		}

		if (!isLocalPlayer)
			return;
	}
}
