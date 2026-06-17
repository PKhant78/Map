using UnityEngine;
using UnityEngine.XR.ARFoundation;


[RequireComponent(typeof(ARFace))]

public class FaceTracker : MonoBehaviour
{
    ARFace arFace;               // Component from AR Foundation,used to get the data of face detection
    ARFaceManager faceManager;   // the manager who takes control of the face tracking event

    GameObject head;             //The prefab "Head" inside of the prefab "User"


    /// <summary>
    /// 1. Assign the object which for tracking face movement when awake.
    /// 2. Also find the head object which we want to user AR to control rotatation
    /// </summary>
    void Awake()
    {
        /* Assign the arFace and arFaceManager */
        arFace = GetComponent<ARFace>();
        faceManager = FindObjectOfType<ARFaceManager>();

        /* Get the head object */
        head = GameObject.Find("/User/Head");
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
            Debug.Log("Eye Tracking is support on this device");
        }
        else
            Debug.Log("Eye Tracking is not support on this device");
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
        /* Only update the head rotation when eyes are detected
         * because ArFaceManager relies on eyes to track face movement */
        if (arFace.leftEye != null && arFace.rightEye != null)
            head.transform.rotation = arFace.transform.rotation;
    }

}


