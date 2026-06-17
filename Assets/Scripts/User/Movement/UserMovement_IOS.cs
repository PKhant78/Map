/*****************************************************************/
/* Programmer: MRCane Development Team                           */
/* Date: April 3rd, 2022                                         */
/* Class: UserMovement_IOS                                       */
/* Purpose:                                                      */
/* The class controls Avatar's movement (translation & rotation) */
/* on IOS Device, by using ARFoundation. Thus, the script only   */
/* works on IOS Device. For "movement control in Unity Editor"   */
/* please visit "UserMovement_Editor" class instead.             */
/*****************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


public class UserMovement_IOS : MonoBehaviour
{

#if UNITY_IOS

    string currentScene;                                          // the name of current scene

    public bool lockTranslation = false;                          // a boolean variable allow disabling avatar's movement (rotation still available)
    public static string headControlChoice = "Airpod";            // [Valid Strings: Airpod, ARFace. Default = Airpod] allow user to choose how to control user's head rotation
    public static bool enhanceAirpodRotate = true;                // a boolean variable indicates if we want to use ARFace to enhance the performance when using "Headphone/Airpod to control head rotate"

    private bool shadowSpineAdded = false;                        // a boolean variable indicates whether a ShadowSpine is added to the scene or not

    GameObject head;
    GameObject body;
    GameObject gripPoint;

    ARSessionOrigin arSessionOrigin;    // we can get ARCamera's data from ARSessionOrigin. We use AR to control Avatar movement on IOS  

    Vector3 arCamLastPos;               // position of ARCamera in the last frame
    Vector3 arCamLastRot;               // rotation of ARCamera in the last frame

    Vector3 arCamCurrPos;               // position of ARCamera in the this frame
    Vector3 arCamCurrRot;               // rotation of ARCamera in the this frame

    Vector3 arCamPosDiff;               // difference between AR Camera's last vs. current position
    Vector3 arCamRotDiff;               // difference between AR Camera's last vs. current rotation

    GameObject shadowGripPoint;         // a tiny object at same position as gripPoint. It only takes ARCamera's rotation on y-axis (horzontal) 
    GameObject shadowTorso;             // a tiny object at same x & z axis as the torse (head+body). It will be child of shadowGripPoint. It will be used to locate the x & z position of "Head+Body" when using AR control

    GameObject shadowSpine;             // an empty game object which will be added to game if user position is locked. It will be destroyed if user position is unlocked (free to move)

    ARFaceManager faceManager;          // the manager who control the AR Face Detection


    /// <summary>
    /// Start() initialize variables and setting for control Avatar on IOS device
    /// 1. Firstly, it will adjust the rotation for Avatar to make it align with the rotation of ARCamera.
    /// Also, other objects will rotate along with user Avatar to keep their relative position and rotation
    /// 2. It will do other setup for controlling Avatar on IOS
    /// </summary>
    private void Start()
    {
        /* Non movement related setups */
        NormalSetup();

        /* Rotate user Avatar to align the rotation of ARCamera */
        //InitRotationForArControl();

        /* Setup for controlling Avatar on IOS */
        MovementSetup();
    }


    /// <summary>
    /// Update user's movement when the App runs on IOS (use ARFoundation control)
    /// </summary>
    void Update()
    {
        MovementControl();
    }


    /// <summary>
    /// Function does Non-movement related setups 
    /// </summary>
    void NormalSetup()
    {
        /* Get the name of current scene */
        currentScene = SceneManager.GetActiveScene().name;
    }


    /// <summary>
    /// Setup steps to do specifically for running this program on IOS
    /// </summary>
    void MovementSetup()
    {
        /* Assigning "head", "body" and "gripPoint" object */
        head = transform.Find("Head").gameObject;
        body = transform.Find("Body").gameObject;
        gripPoint = transform.Find("GripPoint").gameObject;

        /* Get ARSessionOrigin from Scene and initialize it*/
        arSessionOrigin = FindObjectOfType<ARSessionOrigin>();

        /* Initialize the Vector3 variables lastPos and lastRot */
        arCamLastPos = arSessionOrigin.camera.transform.localPosition;
        arCamLastRot = arSessionOrigin.camera.transform.localEulerAngles;

        /* Initiatation for shadowGripPoint & shadowTorso */
        InitShadowElements();

        /* Find the faceManager, it will be used in both "ARFace" or "Headphone" control 
         * 1. "FaceTracker.cs" can enable controlling user's head rotation using ARFace
         * 2. "HeadphoneTrackEnhancer.cs" can improve performance of head rotation control when using HeadphoneTracker.cs */
        faceManager = GameObject.Find("AR Session Origin").GetComponent<ARFaceManager>();

        /* Call functions to decide using which method to control head rotation */
        DecideUseArHeadRotate();
        DecideUseAirpodRotate();
    }


    /// <summary>
    /// Function initializes shadow elements. These shadow elements are used to locate 
    /// the position of User Avatar's head and body.
    ///
    /// [Why?]
    /// The back AR Camera's rotation and position is applied to the gripPoint.
    /// We uses the position of gripPoint and the distance (offset) between gripPoint
    /// and avatar's spinal (mid-point of avatar's head and body) to locate avatar's
    /// head and body position
    /// 
    /// </summary>
    void InitShadowElements()
    {
        /* Initiatation for shadowGripPoint empty object */
        shadowGripPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);        // Instantiate an empty object as shadowGripPoint
        shadowGripPoint.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f); // Make the shadowGripPoint very tiny
        shadowGripPoint.name = "ShadowGripPoint";                                // Set Name for shadowGripPoint object
        shadowGripPoint.transform.SetParent(gripPoint.transform);                // Temporarily place shadowGripPoint under the gripPoint
        shadowGripPoint.transform.localPosition = Vector3.zero;                  // Move shadowGripPoint to the same position as gripPoint
        shadowGripPoint.transform.SetParent(transform);                          // Set user as the direct parent of shadowGripPoint

        /* Instantiation for shadowTorso empty object */
        shadowTorso = GameObject.CreatePrimitive(PrimitiveType.Cube);            // Instantiate an empty object as shadowTorso
        shadowTorso.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);     // Make the shadowTorso very tiny
        shadowTorso.name = "ShadowTorso";                                        // Set Name for shadowTorse object
        shadowTorso.transform.SetParent(head.transform);                         // Temporarily place shadowTorso under the head
        shadowTorso.transform.localPosition = Vector3.zero;                      // Move shadowTorso to the same position as head
        shadowTorso.transform.SetParent(shadowGripPoint.transform);              // Set shadowGripPoint as the direct parent of shadowTorso
        shadowTorso.transform.localPosition = Vector3.Scale(shadowTorso.transform.localPosition, new Vector3(1, 0, 1));      // Change shadowTorso to same height as shadowGripPoint
    }


    /// <summary>
    /// Function adds a ShadowSpine.
    /// Usually, ShadowSpine is added when user movement is locked
    /// </summary>
    void AddShadowSpine()
    {
        /* Collect transforms of all children under User (except Head). We don't
         * want to control head rotation ===> it's controlled by something else */
        List<Transform> childrenTrans = new();
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i).name != "Head")
                childrenTrans.Add(transform.GetChild(i));
        }

        /* Init shadowSpine */
        shadowSpine = new GameObject();                                          // Instantiate a shadow spine empty game object
        shadowSpine.name = "ShadowSpine";                                        // Rename the shadow spine
        shadowSpine.transform.SetParent(body.transform);                         // Set User's body as the parent of ShadowSpine gameObject
        shadowSpine.transform.localPosition = Vector3.zero;                      // Put shadow spine to the same position as User's Body
        shadowSpine.transform.SetParent(transform);                              // Put the ShadowSpine as a direct child of User

        /* Let "ShadowSpine" to become the parent of all
         * other gameObjects under the User gameObject. 
         * so we treat ShadowSpine as a vertical center line,
         * and rotate of all "User parts" around it */
        foreach (Transform childTrans in childrenTrans)
            childTrans.SetParent(shadowSpine.transform);
    }


    /// <summary>
    /// Function removes an existing ShadowSpine.
    /// Usually, ShadowSpine is removed when user movement is unlocked
    /// </summary>
    void RemoveShadowSpine()
    {
        /* Move all children of "ShadowSpine" to become the children of "User" game object */
        for (int i = shadowSpine.transform.childCount - 1; i >= 0; --i)
        {
            Transform childTrans = shadowSpine.transform.GetChild(i);
            childTrans.SetParent(transform);
        }

        Destroy(shadowSpine);                                                     // Destroy the shadow spine game object
    }


    /// <summary>
    /// Function decides whether using ARFace for controlling user head rotation or not 
    /// </summary>
    void DecideUseArHeadRotate()
    {
        /* Setup for using ARFaceTracking to control Avatar's head rotation */
        if (headControlChoice == "ARFace")                                                      // ARFaceTracking control head rotate on/off
        {
            faceManager.facePrefab.GetComponent<FaceTracker>().enabled = true;                  // If user chose to use "ArHeadRotate" then we turn on the "FaceTracker.cs" script
            faceManager.facePrefab.GetComponent<HeadphoneTrackEnhancer>().enabled = false;      // And we turn off the "HeadphoneTrackEnhancer.cs" script because it's a helper class when using Airpod for controlling head rotation
        }
        else
            faceManager.facePrefab.GetComponent<FaceTracker>().enabled = false;
    }


    /// <summary>
    /// Function decides whether using airpod for controlling user head rotation or not 
    /// </summary>
    async void DecideUseAirpodRotate()
    {
        /* Setup for using HeadPhoneMotion plugin to control Avatar's head rotation
         * [Note] HeadPhoneMotion plugin need some setup in XCode before building to phone,  
         * please see documentation here: https://github.com/anastasiadevana/HeadphoneMotion
         */
        if (headControlChoice == "Airpod")                                                  // Check if user choose to use headphone to control head rotation
        {
            GetComponent<HeadphoneTracker>().TryTurnOnTracking(head.transform);             // Turn on the Headphone tracker and make it control user head                       

            /* Conditionally turn on/off the functionality that "using ARFace tracking to enhance headphone rotation control during runtime"*/
            if (enhanceAirpodRotate)
                faceManager.facePrefab.GetComponent<HeadphoneTrackEnhancer>().enabled = true;
            else
                faceManager.facePrefab.GetComponent<HeadphoneTrackEnhancer>().enabled = false;

            /* Wait X seconds and calibrate avatar's head rotation to horizontal 
                * If the application is ended ===> don't calibrate avatar's head
                * Otherwise, it will trigger error */
            await Task.Delay(3000);
            if (Application.isPlaying && currentScene == SceneManager.GetActiveScene().name)
                GetComponent<HeadphoneTracker>().CalibrateTargetRotation();
        }
    }


    /// <summary>
    /// Method for controlling user's movement when running the program on phone (IOS or Android)
    /// </summary>
    void MovementControl()
    {
        PositionControl();
        RotationControl();
    }


    /// <summary>
    /// Method for controlling user's position when running on IOS
    /// </summary>
    void PositionControl()
    {
        /* Get AR Camera's latest position of current frame (No matter will use it to change avatar's position or not) */
        arCamCurrPos = arSessionOrigin.camera.transform.localPosition;

        /* update user's position in each frame according to AR Camera (only when avatar's translation is not locked) */
        if (!lockTranslation)
        {
            /* Get AR Camera's position difference between current vs. last frame */
            arCamPosDiff = arCamCurrPos - arCamLastPos;

            /* 1. We want the GripPoint of cane does exactly same position movement as AR-Camera does in real world, so it can lead to position movement of cane
            /* 2. We don't want GripPoint position in y-axis to change, so times (1, 0, 1) to camera's position difference */
            gripPoint.transform.position += Vector3.Scale(arCamPosDiff, new Vector3(1, 0, 1));

            /* In terms of "position", we want the shadowGripPoint follows the gripPoint
            /* When shadowGripPoint move, its child "shadowTorso" will move along with it */
            shadowGripPoint.transform.position = gripPoint.transform.position;

            /* Use the position of "shadowTorso" to locate the head and body's position */
            Vector3 newHeadPos = shadowTorso.transform.position;            // head will be at same x & z position as shadowTorso       
            newHeadPos.y = head.transform.position.y;                       // head will stay at its on y position
            head.transform.position = newHeadPos;                           // update position for head

            Vector3 newBodyPos = shadowTorso.transform.position;            // body will be at same x & z position as shadowTorso
            newBodyPos.y = body.transform.position.y;                       // body will stay at its on y position
            body.transform.position = newBodyPos;                           // update position for body
        }

        /* Save AR Camera's latest position of current frame (No matter used it to change avatar's position or not) */
        arCamLastPos = arCamCurrPos;
    }


    /// <summary>
    /// Method for controlling user's rotation when running on IOS
    /// </summary>
    void RotationControl()
    {
        arCamCurrRot = arSessionOrigin.camera.transform.localEulerAngles;  // Get AR Camera's latest rotation of current frame
        arCamRotDiff = arCamCurrRot - arCamLastRot;                        // Get AR Camera's rotation difference between current vs. last frame

        /* Dynamically control the existence of ShadowSpine. It will be
         * used for controlling rotation when user's position is locked */
        ShadowSpineExistenceController();

        /* Different ways of rotation when user position is locked vs. unlocked */
        if (!lockTranslation)
        {
            /* We want ARCamera's rotaion controls gripPoint's vertical (x-axis) & horizontal (y-axis) rotaion*/
            gripPoint.transform.localEulerAngles += Vector3.Scale(arCamRotDiff, new Vector3(1, 1, 0));

            /* We want ARCamera's rotaion controls shadowGripPoint's horizontal (y-axis) rotation ONLY */
            shadowGripPoint.transform.localEulerAngles += Vector3.Scale(arCamRotDiff, new Vector3(0, 1, 0));

            /* We want ARCamera's rotaion controls body's horizontal (y-axis) rotation ONLY */
            body.transform.localEulerAngles += Vector3.Scale(arCamRotDiff, new Vector3(0, 1, 0));
        }
        else
        {
            /* We want ARCamera's rotaion controls gripPoint's horizontal (y-axis) rotaion ONLY */
            shadowSpine.transform.localEulerAngles += Vector3.Scale(arCamRotDiff, new Vector3(0, 1, 0));

            /* We want ARCamera's rotaion controls gripPoint's vertical (x-axis) rotaion*/
            gripPoint.transform.localEulerAngles += Vector3.Scale(arCamRotDiff, new Vector3(1, 0, 0));
        }

        /* [Note] --- Head Rotation Control
         * 
         * ---> The 1st way to control Head rotation is using AR Face Tracking 
         * in another script called "FaceTracker.cs". The MovementSetup() function
         * contains code for enable and disable "AR Face Rotation Control" based 
         * on user choice. The instance of "FaceTracker.cs" script will be created
         * only when face is detected. To dynamically control head rotation more
         * easier, we do "using AR to control head rotation" in that script.
         * 
         * ---> The 2nd way to control Head rotation is using Apple's headphone
         * motion. In MovementSetup() function, we initiazlie the "HeadPhoneMotion"
         * plugin and subscribe a function, which rotate head according to headphone
         * movement, to the event which tracks headphone motion. Thus, Avatar's head 
         * will move along with the headphone.
         */

        /* after adjusting user's rotation, save this frame's rotation data for next compare */
        arCamLastRot = arCamCurrRot;

    }


    /// <summary>
    /// Function adds and removes ShadowSpine when criteria meets
    /// </summary>
    void ShadowSpineExistenceController()
    {
        /* Add the ShadowSpine when developer requires to lock user movement 
         * Using a boolean "shadowSpineAdded" to make sure this code runs only once */
        if (lockTranslation && !shadowSpineAdded)
        {
            AddShadowSpine();
            shadowSpineAdded = true;
        }

        /* When user movement is unlocked, remove ShadowSpine */
        if (!lockTranslation && shadowSpineAdded)
        {
            RemoveShadowSpine();
            shadowSpineAdded = false;
        }
    }


    /// <summary>
    /// 1. We need to adjust rotation of user Avatar to make it face the same direction as ARCamera when App start,
    /// so the AR camera's movement can be used to control user Avatar accurately & seamlessly.
    /// 2. We need to keep the relative position and rotation between user vs. other objects. Thus, we can't
    /// rotate the user only, we also need to rotate all other objects in the scene along with the user.
    /// </summary>
    void InitRotationForArControl()
    {
        /* Name of objects which we don't want to rotate */
        List<string> objNotRotate = new() { "AR Session Origin", "AR Session" };

        /* Find all root GameObjects which are in the scene */
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        /* Initialize an empty object at (0,0,0) in the scene to help with rotation */
        GameObject rotateHelper = new();

        /* Find the degrees to rotate
         * <Note> "degrees to rotate" is a value which is needed to rotate
         * the y-axis of userAvatar to the y-axis of AR Camera. AR Camera
         * is by default at position = (0,0,0) and rotation = (0,0,0). So
         * changing Avatar's y-axis rotation to 0.
         */
        GameObject user = GameObject.Find("User");
        float userRotY = user.transform.localEulerAngles.y;
        float degreesToRot = 0 - userRotY;

        /* Rotate all objects in the scene (Except "AR Session Origin" and "AR Session") */
        if (degreesToRot != 0)
        {
            // set "rotateHelper" as the parent of all objects in the scene
            foreach (GameObject obj in rootObjects)
                if (!objNotRotate.Contains(obj.name))
                    obj.transform.SetParent(rotateHelper.transform);

            // rotate the "rotateHelper" so all the objects will be rotated along with it
            rotateHelper.transform.localEulerAngles += new Vector3(0, degreesToRot, 0);

            // remove the parent-child relation between "rotateHelper" and all objects
            foreach (GameObject obj in rootObjects)
                obj.transform.parent = null;
        }

        // destroy the empty "rotateHelper" object
        Destroy(rotateHelper);
    }

#endif

}

