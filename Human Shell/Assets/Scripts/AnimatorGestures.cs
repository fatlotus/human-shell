using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using System.Net.Sockets;
using System;

/*
 * GESTURES
 */
public enum GestureSecResult {
	Failed,
	Succeeded
}

public interface GestureSegment {
	GestureSecResult Update (KinectData client);
}

public class SkiPoleRaise : GestureSegment {
	public GestureSecResult Update (KinectData client) {
		float allowedErr = 0.05f;

		if ((client.righthand.y > client.rightelbow.y)
			&& (client.lefthand.y > client.leftelbow.y)
			&& ( client.righthand.x - client.rightelbow.x <= allowedErr )
			&& ( client.lefthand.x - client.leftelbow.x <= allowedErr ) )
			return GestureSecResult.Succeeded;
		return GestureSecResult.Failed;
	}
}

public class SkiPoleDig : GestureSegment {
	public GestureSecResult Update (KinectData client) {
		float allowedErr = 0.05f;
	
		if ((client.righthand.y < client.rightelbow.y)
			&& (client.lefthand.y < client.leftelbow.y)
			&& ( client.righthand.x - client.rightelbow.x <= allowedErr )
			&& ( client.lefthand.x - client.leftelbow.x <= allowedErr ) )
			return GestureSecResult.Succeeded;
		return GestureSecResult.Failed;
	}
}

public class ClickCardboard : GestureSegment {
	public GestureSecResult Update (KinectData client) {
		float allowedErr = 0.05f;
	
		if ((client.lefthand - client.head).magnitude <= allowedErr)
			return GestureSecResult.Succeeded;
		return GestureSecResult.Failed;
	}
}

public class BeginSkiing {
	readonly int WINDOW_SIZE = 50;
	int frameCount = 0;

	List<GestureSegment> segments;
	int currSegment = 0;

	public event EventHandler GestureRecognized;

	public BeginSkiing () {
		segments.Add (new SkiPoleRaise ());
		segments.Add (new SkiPoleDig ());
	}
}
/*
	public void Update (KinectData client) {
		GestureSecResult result = segments[currSegment].Update(client);

		if (result == GestureSecResult.Succeeded) {
			if (currSegment + 1 < segments.Count) {
				currSegment++;
				frameCount = 0;
			}
			else if (GestureRecognized != null) {
				GestureRecognized(this, new EventArgs());
				Reset();
			}
		}
		else if ((result == GestureSecResult.Failed) || (frameCount == WINDOW_SIZE)) {
			Reset ();
		}
		else {
			frameCount++;
		}
	}

	public void Reset () {
		currSegment = 0;
		frameCount = 0;
	}
	*/
// INTEGRATE THESE INSTEAD

public enum Status {
	FREE = 0,                 // When instantiated, switch to free.
	REQUESTING_CONNECTION,     // When the position is ClickPosition & the kinect is free, switch to request_connection.
	CONNECTED                 // When an association between the phone and the kinect is made, switch to CONNECTED.
};

/*
* KinectData:
* Struct is to hold the associated kinect_id and all the vector information.
* There will be a dictionary of this information.
*/
public struct KinectData {

//	public void ConstructData (Vector3 v) {
//		this.connection = Status.FREE;
//		this.head =
//			this.lefthand = this.righthand =
//				this.leftshoulder = this.rightshoulder =
//					this.leftelbow = this.rightelbow =
//						this.leftknee = this.rightknee = 
//							this.lefthip = this.righthip = 
//								this.torso =
//									this.leftfoot = this.rightfoot = new Vector3(0.0f, 0.0f, 0.0f);
//	}

	public Status         connection;
	public Vector3 head,
	lefthand, righthand,
	leftshoulder, rightshoulder,
	leftelbow, rightelbow,
	leftknee, rightknee,
	lefthip, righthip,
	torso,
	leftfoot, rightfoot;
}

public class AnimatorGestures : NetworkBehaviour {

	TcpClient 		mySocket;
	NetworkStream 	theStream;
	StreamWriter 	theWriter;
	StreamReader 	theReader;

	private GameObject cardboard;
	private GameObject camera;

	private Animator clientAnim;

	/*
     * Basic structure of association dictionary: If the client id is not in here, add and make the value = -1.
     * Dictionary<int, int>             associationDictionary;     // Key = client id. Value = kinect id. (Association dictionary.)
     * if (!clientDict.ContainsKey(clientId)) {
     *         clientDict.Add(clientId, -1);
     * }
     * 
     * Basic structure of kinect info dictionary:
     * Dictionary<int, KinectData>     kinectInfo; // Key = kinect id. Value = kinect data.
     */

	public Dictionary<int, int>			associationDictionary;
	public Dictionary<int, KinectData>		kinectInfo;

	float     Kinect2Unity = 1.0f/1000.0f;

	/* To-do:
     * Have the struct be accessible throughout the relevant scripts
     */

	private CharacterController characterController;

	private float moveScale = 0.1f;

	// Use this for initialization
	void Start () {
		mySocket 	= new TcpClient ("linux3.cs.uchicago.edu", 9000);
		theStream 	= mySocket.GetStream();
		theWriter 	= new StreamWriter(theStream);
		theReader 	= new StreamReader(theStream);

		clientAnim = GetComponent<Animator>();
		characterController = GetComponent<CharacterController> ();

		cardboard = GameObject.Find ("Scene_Cardboard");
		camera = cardboard.transform.GetChild (0).gameObject;
	}

	// Update is called once per frame
	void Update () {
		//*
        while (theStream.DataAvailable) {
            string reading = theReader.ReadLine ();
            string[] parts = reading.Split (new Char[] {' '});

			int currKinectID = (int)float.Parse (parts [2]);
            if (!kinectInfo.ContainsKey (currKinectID)) {
				print ("Attempting to add kinect.");
				kinectInfo.Add( currKinectID, new KinectData () );
            }

			KinectData temp;
			kinectInfo.TryGetValue (currKinectID, out temp);
             
			Vector3 pos = new Vector3 (float.Parse (parts [2]), float.Parse (parts [3]), float.Parse (parts [4])) * Kinect2Unity;
            pos.x *= -1;
            pos.z *= -1;

            switch (parts [0]) {
            case "head":
                break;
            case "lefthand":
				temp.lefthand = pos * 0.5f + temp.lefthand * 0.5f;
                break;
            case "righthand":
				temp.righthand = pos * 0.5f + temp.righthand * 0.5f;
                break;
            case "leftshoulder":
                temp.leftshoulder = pos;
                break;
            case "rightshoulder":
                temp.rightshoulder = pos;
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
                temp.lefthip = pos;
                break;
            case "righthip":
                temp.righthip = pos;
                break;
            case "torso":
                temp.torso = pos;
                break;
            case "leftfoot":
                temp.leftfoot = pos;
                break;
            case "rightfoot":
                temp.rightfoot = pos;
                break;
            default:
                print (parts [0]);
                cardboard.transform.position = new Vector3 (100, 100, 100);
                break;
            }

        }

		if (!isLocalPlayer)
			return;

		int assocKinect;
		associationDictionary.TryGetValue( int.Parse(Network.player.ToString()), out assocKinect);

		KinectData clientKinect;
		kinectInfo.TryGetValue(assocKinect, out clientKinect);

		Vector3 LeftRight		= Input.GetAxis("Vertical") * Vector3.Normalize(camera.transform.forward);
		Vector3 ForwardBack 	= Input.GetAxis("Horizontal") * Vector3.Normalize(camera.transform.right);

		// <Shamelessly stolen from Unity Standard Assets.>

		// always move along the camera forward as it is the direction that it being aimed at
		Vector3 desiredMove = (ForwardBack + LeftRight);

		// get a normal for the surface that is being touched to move along it
		RaycastHit hitInfo;
		Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
			characterController.height/2f, ~0, QueryTriggerInteraction.Ignore);
		desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized  * moveScale;

		characterController.Move (desiredMove);

		Vector3 shoulders = (clientKinect.leftshoulder + clientKinect.rightshoulder) / 2.0f;
		Vector3 hips = (clientKinect.lefthip + clientKinect.righthip) / 2.0f;
		Vector3 motion = (shoulders - hips) * 0.4f * (0.9f + Crouch(clientKinect));
		motion.y = 0;

		if (motion.magnitude >= 0.05)
			transform.Translate (motion);

		cardboard.transform.position = transform.position - new Vector3(0, -1.3f, 0.2f);

		// </ShamelesslyStolen>
	}

	void OnAnimatorIK(int layerIndex) {
		int assocKinect;
		associationDictionary.TryGetValue( int.Parse(Network.player.ToString()), out assocKinect);

		KinectData clientKinect;
		kinectInfo.TryGetValue(assocKinect, out clientKinect);

		clientAnim.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1);
		clientAnim.SetIKPosition(AvatarIKGoal.LeftHand, clientAnim.bodyPosition + (clientKinect.righthand - clientKinect.torso)); // <- intentional

		clientAnim.SetIKPositionWeight (AvatarIKGoal.RightHand, 1);
		clientAnim.SetIKPosition(AvatarIKGoal.RightHand, clientAnim.bodyPosition + (clientKinect.lefthand - clientKinect.torso));

		clientAnim.SetIKPositionWeight (AvatarIKGoal.LeftFoot, 1);
		clientAnim.SetIKPosition(AvatarIKGoal.LeftFoot, clientAnim.bodyPosition + (clientKinect.rightfoot - clientKinect.torso)); // <- intentional

		clientAnim.SetIKPositionWeight (AvatarIKGoal.RightFoot, 1);
		clientAnim.SetIKPosition(AvatarIKGoal.RightFoot, clientAnim.bodyPosition + (clientKinect.leftfoot - clientKinect.torso));

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

	/*
	 * CROUCH: Imitates the speeding up from a player crouching while skiing.
	 */
	float Crouch (KinectData client) {
		float angle = (Vector3.Angle (client.lefthip - client.leftknee, client.leftfoot - client.leftknee) + Vector3.Angle (client.righthip - client.righthip, client.rightfoot - client.rightknee)) / 2.0f;

		return (180.0f - angle) / 180.0f;
	}
	//*/
}

