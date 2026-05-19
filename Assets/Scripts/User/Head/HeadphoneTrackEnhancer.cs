/*******************************************************************/
/* Programmer: MRCane Development Team                             */
/* Date: July 9th, 2022                                            */
/* Class: HeadphoneTrackEnhancer                                   */
/* Purpose:                                                        */
/* The purpose of this class is using the ARFace tracking to       */
/* enhance the performance of "HeadphoneTracker". During runtime,  */
/* if the Avatar's head rotation becomes FAR ENOUGH from the       */
/* rotation detected by ARFace tracking ===> We will ask the       */
/* HeadphoneTracker to do calibration and treat the rotation       */
/* captured by the ARFace tracking as the "Origin". By doing this  */
/* we can ensure the Avatar's head rotation is always sync with    */
/* the rotation captured by ARFace tracking (which is always the   */
/* correct rotation). This class will be helpful if the users      */
/* adjusted position of their headphone, or their headphone loose  */
/* when playing the game.                                          */
/*******************************************************************/


using UnityEngine;
using UnityEngine.XR.ARFoundation;


[RequireComponent(typeof(ARFace))]

public class HeadphoneTrackEnhancer : MonoBehaviour
{
    ARFace arFace;                                 // Component from AR Foundation,used to get the data of face detection
    ARFaceManager faceManager;                     // The manager who takes control of the face tracking event
    GameObject head;                               // The prefab "Head" inside of the prefab "User"
    HeadphoneTracker headphoneTracker;             // The class controls the headphone tracking 

    /* [Default = 15f] If the x,y,z of an euler rotation is within "rotNearErrorMargin" distance from another euler rotation ===> We say these two rotations are near.
     * This value is VERY IMPORTANT, as it's used to judge when to calibrate headphoneTracker in "OnUpdated()" function */
    public static float rotNearErrorMargin = 15f;       


    /// <summary>
    /// 1. Assign the object which for tracking face movement when awake.
    /// 2. Also find the head object which we want to user AR to control rotatation
    /// </summary>
    void Awake()
    {
        InitVariables();
    }


    /// <summary>
    /// Initialize important variables
    /// </summary>
    void InitVariables()
    {
        /* Assign reference to the arFace and arFaceManager */
        arFace = GetComponent<ARFace>();
        faceManager = FindObjectOfType<ARFaceManager>();

        /* Assign reference to the head gameObject */
        head = GameObject.Find("/User/Head");

        /* Assign reference to the headphoneTracker */
        headphoneTracker = GameObject.Find("User").GetComponent<HeadphoneTracker>();
    }


    /// <summary>
    /// ---> OnEnable calls when this script is enabled. The script will be enabled when ARFaceManager 
    /// detected a face in real world, and generated an object out of the ArFacePrefab called "EyeTracking"
    /// (Find the prefab at "Assets => Prefabs => User"). Since this script is a component of that prefab,
    /// it will be activated as well.
    /// ---> The OnEnable function will subsribe an function "OnUpdated()" to the "arFace update event"
    /// So everytime the arFace is updated, it will do whatever stated in the "OnUpdated()" function.
    /// </summary>
    void OnEnable()
    {
        if (faceManager != null && faceManager.subsystem != null && faceManager.subsystem.subsystemDescriptor.supportsEyeTracking)
        {
            arFace.updated += OnUpdated;
            Debug.Log("Eye Tracking is support on this device ===> [From HeadphoneTrackEnhancer.cs]");
        }
        else
            Debug.Log("Eye Tracking is not support on this device ===> [From HeadphoneTrackEnhancer.cs]");
    }


    /// <summary>
    /// ---> When this script end, removed the function we subsribed to the "arFace update event"
    /// </summary>
    void OnDisable()
    {
        arFace.updated -= OnUpdated;
    }


    /// <summary>
    /// OnUpdated function states all the actions to take when arFaceManager detected any change on the face
    /// </summary>
    void OnUpdated(ARFaceUpdatedEventArgs eventArgs)
    {
        /* Only take action if both eyes are detected */
        if (arFace.leftEye == null || arFace.rightEye == null)
            return;

        /* Get the rotation of ARFace and User's head */
        Vector3 faceRot = arFace.transform.eulerAngles;
        Vector3 headRot = head.transform.eulerAngles;

        /* We ask the HeadphoneTracker to do calibration using "ARFace rotation as origin" when:
         * The rotation of head is too different from the rotation captured by ARFace (aka Two rotation are NOT near) */
        if(!EulerArithmeticHelper.IsEulerRotNear(faceRot, headRot, rotNearErrorMargin))
        {
            //Debug.Log($"ARFace Euler: {arFace.transform.eulerAngles} || Head Euler: {head.transform.eulerAngles}");
            headphoneTracker.CalibrateTargetRotation_GivenOrigin(Quaternion.Euler(faceRot));
        }
    }

}

