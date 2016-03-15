using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Net.Sockets;
using System;
using System.Collections.Generic;


public class ClientAnimation : NetworkBehaviour {
	TcpClient 		mySocket;
	NetworkStream 	theStream;
	StreamWriter 	theWriter;
	StreamReader 	theReader;

	private GameObject cardboard;
	private GameObject camera;
	private TextMesh helpfulText;

	private Animator clientAnim;
	private Vector3 leftHand, rightHand, torso, leftFoot, rightFoot;
	private Vector3 leftShoulder, rightShoulder, leftHip, rightHip;

	[SyncVar]
	private Vector3 velocity;
	[SyncVar]
	private Vector3 acceleration;
	private Vector3 previousPos;

	private List<String> kinects;

	[SyncVar]
	private String myKinect = "";
	private float lastGood = 0;

	private CharacterController characterController;

	private float moveScale = 0.1f;

	// Use this for initialization
	void Start () {
		mySocket 	= new TcpClient ("linux2.cs.uchicago.edu", 9000);
		theStream 	= mySocket.GetStream();
		theWriter 	= new StreamWriter(theStream);
		theReader 	= new StreamReader(theStream);

		clientAnim = GetComponent<Animator>();
		characterController = GetComponent<CharacterController> ();

		cardboard = GameObject.Find ("Scene_Cardboard");
		camera = cardboard.transform.GetChild (0).gameObject;

		helpfulText = GameObject.Find ("HUD").GetComponent<TextMesh> ();

		velocity = new Vector3 (0, 0, 0);
		acceleration = new Vector3 (0, 0, 0);

		kinects = new List<String> ();
	}

	// Update is called once per frame
	void Update() {
		if (!isLocalPlayer)
			return;

		while (theStream.DataAvailable) {
			string reading = theReader.ReadLine ();
			string[] parts = reading.Split (new Char[] { ' ' });

			Vector3 pos = new Vector3(float.Parse(parts [2]), float.Parse(parts [3]), float.Parse(parts [4]))/1000.0f;
			pos.x *= -1;
			pos.z *= -1;

			if (pos.magnitude == 0) {
				kinects.Remove (parts [1]);

				if (myKinect == parts [1]) {
					myKinect = "";
				}
			} else {
				if (!kinects.Contains (parts [1]))
					kinects.Add (parts [1]);

				if (myKinect == parts [1])
					lastGood = Time.time;
				else if (myKinect == "" && (Time.time - lastGood) > 5)
					myKinect = parts [1];
			}

			if (parts[1] == myKinect)
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

		Vector3 LeftRight	= Input.GetAxis("Vertical") * Vector3.Normalize(camera.transform.forward);
		Vector3 ForwardBack = Input.GetAxis("Horizontal") * Vector3.Normalize(camera.transform.right);

		// <Shamelessly stolen from Unity Standard Assets.>
		Vector3 midFeet 	= (leftFoot + rightFoot) / 2;
		Vector3 shoulders 	= (leftShoulder + rightShoulder) / 2;
		Vector3 hips 		= (leftHip + rightHip) / 2;
		Vector3 motion 		= (shoulders - hips) * 0.8f;
		motion.y = 0;

		if (motion.magnitude <= 0.05)
			motion = Vector3.zero;

		velocity += (ForwardBack + LeftRight).normalized * 8f * Time.deltaTime + motion * 50f * Time.deltaTime;

//		Vector3 friction = velocity.normalized;
//		friction.y = 0;
//
//		velocity -= friction.normalized * Time.deltaTime * 0.1f;

		// get a normal for the surface that is being touched to move along it
		RaycastHit hitInfo;
		if (Physics.SphereCast (transform.position, characterController.radius, Vector3.down, out hitInfo,
			characterController.height / 2f, ~0, QueryTriggerInteraction.Ignore)) {

			/* Turn */
			Vector3 turn = (torso - midFeet) * 0.2f;
			turn.y = 0;

			if (turn.magnitude <= 0.05) {
				turn = Vector3.zero;
			} else {
				float Rt = 40.0f;
				Vector3 centralVec = (Vector3.Project(turn, Vector3.Cross (velocity, hitInfo.normal))).normalized;
				Vector3 centAcc = (Mathf.Pow (velocity.magnitude, 2.0f) / Rt) * centralVec;
				Vector3 prevV = velocity;
				velocity += centAcc * Time.deltaTime;
				//				camera.transform.rotation += Quaternion.Angle

				float angleSign = (turn.x < 0) ? -1 : 1;

				cardboard.transform.rotation *= Quaternion.AngleAxis(angleSign*Vector3.Angle(prevV, velocity), hitInfo.normal);
				transform.rotation *= Quaternion.AngleAxis(angleSign*Vector3.Angle(prevV, velocity), hitInfo.normal);
			}
				
			/* Gravity */
//			Vector3 gravity = hitInfo.normal;
//			gravity.y = 0;
//			velocity += gravity * 0.02f;

			/* Calculate net acceleration */
			acceleration = new Vector3 (0, 0, 0);
			acceleration += Drag (1.0f);//(shoulders - midFeet).magnitude);
//			acceleration -= 9.8f * new Vector3(0.0f, 9.8f, 0.0f);
			acceleration -= 0.1f * velocity.normalized;
			Vector3 slopeGravity = new Vector3 (0, -9.8f, 0) + Vector3.Project (new Vector3 (0, -9.8f, 0), Vector3.Normalize (transform.position - previousPos));
			acceleration += 10.0f*slopeGravity;
		} else {
			acceleration = new Vector3 (0, -9.8f, 0);
		}

		velocity += acceleration * Time.deltaTime;

		Vector3 desiredMove = velocity * Time.deltaTime;

//		Vector3 desiredMove = (acceleration * Time.deltaTime * Time.deltaTime + Vector3.ProjectOnPlane(velocity, hitInfo.normal) * Time.deltaTime);


//		Vector3 desiredMove = Vector3.ProjectOnPlane(velocity, hitInfo.normal) * moveScale ;

		characterController.Move (desiredMove);

		if (transform.position.z > 1967 || transform.position.z < -20) {
			transform.position = new Vector3(0.0f, 40.0f, 0.0f);
			//velocity = Vector3.zero;
			cardboard.transform.rotation = Quaternion.identity;
		}

		cardboard.transform.position = transform.position - new Vector3(0, -1.3f, 0.2f);

		// </ShamelesslyStolen>

		//print (motion);

		kinects.Sort ();

		int index = kinects.FindIndex ((string p) => {
			return p == myKinect;
		});

		if (index < 0)
			helpfulText.text = "Kinect ??? of " + kinects.Count;
		else
			helpfulText.text = "Kinect " + (index + 1) + " of " + kinects.Count;

		previousPos = transform.position;
	}

	Vector3 Drag (float h) {
		Vector3 directionWind = new Vector3 (Mathf.Sin (10.0f*Time.time), 0, Mathf.Cos (5.0f*Time.time)).normalized;

		float characterArea = h * characterController.radius - 2.0f * characterController.radius + Mathf.PI * Mathf.Pow (characterController.radius, 2.0f);
		float drag = 5f * characterArea * 0.04f * Mathf.Pow(0.5f, 2.0f) * 0.42f;

		return drag * directionWind;
	}

	void OnEnable(){
		Cardboard.SDK.OnTrigger += TriggerPulled;
	}

	void OnDisable(){
		Cardboard.SDK.OnTrigger -= TriggerPulled;
	}

	void TriggerPulled() {
		if (kinects.Count == 0)
			return;

		myKinect = kinects[(kinects.FindIndex ((string p) => { return p == myKinect; }) + 1) % kinects.Count];
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
