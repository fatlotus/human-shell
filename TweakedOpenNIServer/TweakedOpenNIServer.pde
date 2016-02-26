/* 
 * Modified sketch by.
 *
 * --------------------------------------------------------------------------
 * SimpleOpenNI User3d Test
 * --------------------------------------------------------------------------
 * Processing Wrapper for the OpenNI/Kinect 2 library
 * http://code.google.com/p/simple-openni
 * --------------------------------------------------------------------------
 * prog:  Max Rheiner / Interaction Design / Zhdk / http://iad.zhdk.ch/
 * date:  12/12/2012 (m/d/y)
 * ----------------------------------------------------------------------------
 */
 
import SimpleOpenNI.*;
import processing.net.*;

SimpleOpenNI context;
float        zoomF =0.5f;
float        rotX = radians(180);  // by default rotate the hole scene 180deg around the x-axis, 
                                   // the data from openni comes upside down
float        rotY = radians(0);
boolean      autoCalib=true;

PVector      bodyCenter = new PVector();
PVector      bodyDir = new PVector();
PVector      com = new PVector();                                   
PVector      com2d = new PVector();                                   
color[]       userClr = new color[]{ color(255,0,0),
                                     color(0,255,0),
                                     color(0,0,255),
                                     color(255,255,0),
                                     color(255,0,255),
                                     color(0,255,255)
                                   };

Client client;

void setup()
{
  context = new SimpleOpenNI(this);
  if(context.isInit() == false)
  {
     println("Can't init SimpleOpenNI, maybe the camera is not connected!"); 
     exit();
     return;  
  }

  // disable mirror
  context.setMirror(false);

  // enable depthMap generation 
  context.enableDepth();

  // enable skeleton generation for all joints
  context.enableUser();

  client = new Client(this, "linux3.cs.uchicago.edu", 9000);

 }

/*
 * First body part:   lefthand
 * Final body part:   head
 */
void send(int user, String joint, int model) {
  PVector pos = new PVector();
  context.getJointPositionSkeleton(user, model, pos);
  client.write(joint + " " + pos.x + " " + pos.y + " " + pos.z + "\n");
}

void reset(String joint) {
  PVector pos = new PVector(0, 0, 0);
  client.write(joint + " " + pos.x + " " + pos.y + " " + pos.z + "\n");
}

void draw()
{
  // update the cam
  context.update();

  // draw the skeleton if it's available
  int[] userList = context.getUsers();
  boolean any = false;
  for (int i = 0; i < userList.length; i++) {
     
    if(context.isTrackingSkeleton(userList[i])) {
      println("Got data!");
      send(userList[i], "head", SimpleOpenNI.SKEL_HEAD);
      send(userList[i], "lefthand", SimpleOpenNI.SKEL_LEFT_HAND);
      send(userList[i], "righthand", SimpleOpenNI.SKEL_RIGHT_HAND);
      send(userList[i], "leftelbow", SimpleOpenNI.SKEL_LEFT_ELBOW);
      send(userList[i], "rightelbow", SimpleOpenNI.SKEL_RIGHT_ELBOW);
      send(userList[i], "leftshoulder", SimpleOpenNI.SKEL_LEFT_SHOULDER);
      send(userList[i], "rightshoulder", SimpleOpenNI.SKEL_RIGHT_SHOULDER);
      
      send(userList[i], "leftfoot", SimpleOpenNI.SKEL_LEFT_FOOT);
      send(userList[i], "rightfoot", SimpleOpenNI.SKEL_RIGHT_FOOT);
      send(userList[i], "leftknee", SimpleOpenNI.SKEL_LEFT_KNEE);
      send(userList[i], "rightknee", SimpleOpenNI.SKEL_RIGHT_KNEE);
      send(userList[i], "lefthip", SimpleOpenNI.SKEL_LEFT_HIP);
      send(userList[i], "righthip", SimpleOpenNI.SKEL_RIGHT_HIP);
      
      send(userList[i], "torso", SimpleOpenNI.SKEL_TORSO);

      any = true;
      break;
    }
  }
  if (!any) {
    reset("head");
    reset("lefthand");
    reset("righthand");
    reset("leftelbow");
    reset("rightelbow");
    reset("leftshoulder");
    reset("rightshoulder");
    
    reset("leftfoot");
    reset("rightfoot");
    reset("leftknee");
    reset("rightknee");
    reset("lefthip");
    reset("righthip");
    
    reset("torso");
  }
}

// -----------------------------------------------------------------
// SimpleOpenNI user events

void onNewUser(SimpleOpenNI curContext,int userId)
{
  println("onNewUser - userId: " + userId);
  println("\tstart tracking skeleton");
  
  context.startTrackingSkeleton(userId);
}

void onLostUser(SimpleOpenNI curContext,int userId)
{
  println("onLostUser - userId: " + userId);
}

void onVisibleUser(SimpleOpenNI curContext,int userId)
{
  //println("onVisibleUser - userId: " + userId);
}


// -----------------------------------------------------------------
// Keyboard events

void keyPressed()
{
  switch(key)
  {
  case ' ':
    context.setMirror(!context.mirror());
    break;
  }
    
  switch(keyCode)
  {
    case LEFT:
      rotY += 0.1f;
      break;
    case RIGHT:
      // zoom out
      rotY -= 0.1f;
      break;
    case UP:
      if(keyEvent.isShiftDown())
        zoomF += 0.01f;
      else
        rotX += 0.1f;
      break;
    case DOWN:
      if(keyEvent.isShiftDown())
      {
        zoomF -= 0.01f;
        if(zoomF < 0.01)
          zoomF = 0.01;
      }
      else
        rotX -= 0.1f;
      break;
  }
}

void getBodyDirection(int userId,PVector centerPoint,PVector dir)
{
  PVector jointL = new PVector();
  PVector jointH = new PVector();
  PVector jointR = new PVector();
  float  confidence;
  
  // draw the joint position
  confidence = context.getJointPositionSkeleton(userId,SimpleOpenNI.SKEL_LEFT_SHOULDER,jointL);
  confidence = context.getJointPositionSkeleton(userId,SimpleOpenNI.SKEL_HEAD,jointH);
  confidence = context.getJointPositionSkeleton(userId,SimpleOpenNI.SKEL_RIGHT_SHOULDER,jointR);
  
  // take the neck as the center point
  confidence = context.getJointPositionSkeleton(userId,SimpleOpenNI.SKEL_NECK,centerPoint);
  
  /*  // manually calc the centerPoint
  PVector shoulderDist = PVector.sub(jointL,jointR);
  centerPoint.set(PVector.mult(shoulderDist,.5));
  centerPoint.add(jointR);
  */
  
  PVector up = PVector.sub(jointH,centerPoint);
  PVector left = PVector.sub(jointR,centerPoint);
    
  dir.set(up.cross(left));
  dir.normalize();
}
