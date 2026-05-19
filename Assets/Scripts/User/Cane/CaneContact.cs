using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CaneContact : MonoBehaviour
{
    bool judgePosRot = true;                                    // bool var indicates if the cane should do positive rotate on x-axis. Doing tiny rotation on every frame to keep the cane awake

    bool stayWithInnerObj = false;                              // boolean to judge if it stays with inner obj

    Vector3 caneLastPos;                                        // Two variables will work together to track if cane moved or not
    bool caneMoved;                                             // A variable judges if the cane moved from last frame to this
    float caneMoveDist;                                         // Check the distance cane moved from last frame to this (Ignore the small distance to provide better UX in AR mode)
    float caneMoveBenchmarkDist = 0.01f;                        // [Default: 0.01f] The benchmark for judging if the cane moved from last frame to this. Move distances smaller than benchmark are counted as not moved

    SoundBallMovement soundBallMovement;                        // SoundBallMovement class for controlling where the sound ball will move to
    AudioManager_Feedback audioManagerFb;                       // AudioManager_Feedback class for feedback sound playing
    VibrationManager_Feedback vibrationManagerFb;               // VibrationManager_Feedback class for playing feedback vibration
    VerbalManager_Feedback verbalManagerFb;                     // VerbalManager_Feedback class for speaking a text

    [SerializeField]
    public List<string> objDoNotDetect = new List<string>()     // These are the objects we won't detect if cane is in contact with them
    { "User", "GripPoint", "ShadowGripPoint", "SoundBall", "SurroundRadar", "MusicPlayer" };

    [SerializeField]
    public List<string> objDoNotCallName = new List<string>()   // These objects are detectable, but don't use UAP to call their name when hitted
    { "PrevButton", "NextButton",
    "RightQuestionOption", "LeftQuestionOption",
    "Floor", "TactileRoad", "InstructionSystem" };

    [SerializeField]
    public List<string> objDoNotVibrate = new List<string>()    // For these objects, we don't want to provide physical vibration when the cane hits it 
    {};


    private void Start()
    {
        /* Assign components to the declared variables */
        soundBallMovement = GameObject.Find("SoundBall").GetComponent<SoundBallMovement>();
        audioManagerFb = GameObject.Find("SoundBall").GetComponent<AudioManager_Feedback>();
        vibrationManagerFb = transform.GetComponent<VibrationManager_Feedback>();
        verbalManagerFb = GameObject.Find("SoundBall").GetComponent<VerbalManager_Feedback>();

        /* Force turning off the "isTrigger" for the cane */
        GetComponent<Collider>().isTrigger = false;
    }


    void Update()
    {
        /* Detect if the cane moved */
        DetectIfCaneMoved();

        /* Keep the cane always awake */
        KeepCaneAwake();
    }


    /// <summary>
    /// When the cane touch a certain object for the first time
    ///
    /// [Developer Note]
    /// 1. Essential Settings for enabling both "Hit Point Detection" and "Cane Go Through Object" functions
    /// https://docs.google.com/document/d/1_b9sw6hOjG9i-t_TBx5twH_Hcf_Z8EkQG2N_yQfxS7g/edit?usp=sharing
    ///
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        Transform other = collision.collider.transform;               // The collider object which the cane collides with (Actually, the cane triggered. Cane is allowed to go through an obejct)

        /* Filter out the objects we don't want to provide feedback */
        if (!InNotDetectList(other))
        {
            /* Detecting if cane began to hit an object */
            string objectSide = AccessColliderInfo.WhichSide(other); // Whether the collider "other" is an outside or inside object part

            if (objectSide == "Outside" && (!stayWithInnerObj))
            {
                AccessColliderInfo.PrintHittedObject(other);         // Print the name of object hitted

                /* Step 1. Move the Sound Ball to the Hit Point*/
                MoveSoundBall(collision);

                /* Step 2. Play Hit Sound From the Sound Ball */
                string material = AccessColliderInfo.WhatMaterial(other);
                string soundClipName = material + " Hit";
                Debug.Log("Play " + material + " Hit Feedback!");

                audioManagerFb.StopFeedbackAudioByType("Slide");     // When audio requested to play has audio-type "Hit", we will stop ongoing music with type "Slide" 
                audioManagerFb.PlayFeedbackAudio(soundClipName);     // Playing the newly requested audio feedback

                /* Step 3. Play physical vibration */
                ProvideHitVibration(other, soundClipName);

                /* Step 4. Speak Out Loud the Hitted Object Name */
                SpeakHitObjName(other);
            }
        }
    }


    /// <summary>
    /// 
    /// When the cane stay in contact with certain object
    ///
    /// [Developer Note]
    /// Not like OnTriggerStay method will continuously detect if cane is triggering an object,
    /// the OnCollisionStay will turn cane to sleep if it's not moving. Thus, both the Slide and
    /// Alert sounds will play only when cane is moving. That's why we have "KeepCaneAwake()" function.
    /// 
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        Transform other = collision.collider.transform;                  // The collider object which the cane collides with

        /* Filter out the objects we don't want to provide feedback */
        if (!InNotDetectList(other))
        {
            /* Get object information and prepare for providing feedback */
            string objectSide = AccessColliderInfo.WhichSide(other);
            string material = AccessColliderInfo.WhatMaterial(other);
            string soundClipName = "";

            /* Scenario 1: Cane only stays contact with outer object, but not the inner object */
            if ((objectSide == "Outside") && (!stayWithInnerObj))
            {
                if (caneMoved)                                          // Make sure cane is moving before playing the sliding feedback
                {
                    /* Determine the feedback audio to play */
                    Debug.Log("Cane in surface zone, do the " + material + " Sliding Feedback!");
                    soundClipName = material + " Slide";
                }
            }

            /* Scenario 2: Cane is in contact with inner object (1st hit or stay within), play alert feedback */
            if (objectSide == "Inside")
            {
                stayWithInnerObj = true;

                /* Determine the feedback audio to play */
                Debug.Log("Warning: Inside Object Alert On!");
                soundClipName = "VibrationSlow Alert";
                //soundClipName = "Verbal Alert";

                /* Cut Ongoing audio feedback */
                audioManagerFb.StopFeedbackAudioByType("Slide");        // When audio requested to play has audio-type "Alert", we will stop ongoing music with type "Slide" or "Hit
                audioManagerFb.StopFeedbackAudioByType("Hit");

                /* Stop any physical vibration */
                vibrationManagerFb.StopVibrationByType("Hit");
            }

            /* Playing the newly requested audio feedback */
            //if (soundClipName != "")
            //{
            //    MoveSoundBall(collision);                               // Move sound ball to correct point before playing feedback audio
            //    audioManagerFb.PlayFeedbackAudio(soundClipName);
            //}
            if (caneMoved)                                          // Make sure cane is moving before playing the sliding feedback
            {
                /* Determine the feedback audio to play */
                Debug.Log("Cane in surface zone, do the " + material + " Sliding Feedback!");
                soundClipName = material + " Slide";
            }
        }

    }


    /// <summary>
    /// When the cane exit contact with certain object
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        Transform other = collision.collider.transform;                 // The collider object which the cane collides with

        if (!InNotDetectList(other))
        {
            string objectSide = AccessColliderInfo.WhichSide(other);

            /* After leaving contact with inner object, turn off stayWithInnerObj boolean */
            if (objectSide == "Inside")
            {
                stayWithInnerObj = false;

                /* Stop any feedback audio */
                audioManagerFb.StopFeedbackAudioByType("Alert");
                Debug.Log("Leave Inner Object, Alert Feedback Stop!");
            }
        }
    }


    /// <summary>
    /// 
    /// [Brief]
    /// Function moves sound ball to an specific coordinate before playing audio feedback.
    /// 
    /// [Detail]
    /// When cane hits, slids or get to side of an object, there always will be a specific contact point.
    /// The function moves the sound ball to that contact point before sound ball play feedback
    ///
    /// [Input]
    /// parameter "collision" is the object which contains all information about one contact between cane
    /// and an object. We will get contact point from it for transferring the sound ball
    /// 
    /// </summary>
    private void MoveSoundBall(Collision collision)
    {
        Vector3 contactPoint = collision.GetContact(0).point;    // The point where hit, slide or alert feedback is triggered. It's an (x,y,z) coordinate where the sound ball will move to
        soundBallMovement.transportSoundBall(contactPoint);      // Move the Sound Ball to the trigger point
    }


    /// <summary>
    /// 
    /// Brief: Function for speaking out loud the object name hitted by the cane
    ///
    /// Detail Rule of Callout Name:
    /// 1. Name calling will not happen if the hitted object is in "notCallOut" list
    /// 2. If cane hits an object it just hitted, there will be an X seconds of cooldown time before allow next callout
    /// 3. If cane hits a new object, the callout will happen immediately
    ///
    /// Input: The object (type is Collider) with hitted by the cane
    /// 
    /// </summary>
    private void SpeakHitObjName(Transform other)
    {
        if (!InNotCallNameList(other))
        {
            int deltaSeconds = (DateTime.Now - verbalManagerFb.GetLastCalloutTime()).Seconds;  // time gap between the "last successful name calling after hit" and "current hit"
            int actualReferObjId = AccessColliderInfo.GetActualReferObjId(other);
            if (verbalManagerFb.GetLastCalloutObjId() != actualReferObjId || deltaSeconds > verbalManagerFb.GetVerbalGapTime()) // If the scenario is "hitting a new object" or "it has already passed X seconds after hitting the same object last time", callout the hitted object name
            {
                string description = AccessColliderInfo.DescribeHittedObject(other);           // Get object description
                verbalManagerFb.SpeakFeedbackVerbal(description, actualReferObjId);            // Speak the desciption
            }
        }
    }


    /// <summary>
    /// Function takes care of providing physical vibration when hitting an object
    /// </summary>
    private void ProvideHitVibration(Transform other, string materialAndAction)
    {
        if (!InNotVibrateList(other))
        {
            vibrationManagerFb.StartVibration(materialAndAction);
        }
    }


    /// <summary>
    /// Function keeps the cane awake all the time!
    /// If the cane goes to sleep when user not moving, it won't be able to continuously provide vibration
    /// when the cane is inside of an object.
    /// </summary>
    private void KeepCaneAwake()
    {
        // the smallest rotation value needed to activate cane
        float rotVal = 0.0012f;

        if (judgePosRot)
        {
            // if boolean is true, do positive rotate on x-axis in this frame
            transform.Rotate(rotVal, 0, 0);
            judgePosRot = false;
        }
        else
        {   // if boolean is false, do negative rotate on x-axis in this frame
            transform.Rotate(-1 * rotVal, 0, 0);
            judgePosRot = true;
        }
    }


    /// <summary>
    /// Function for detecting if the cane moved
    /// </summary>
    private void DetectIfCaneMoved()
    {
        caneMoveDist = Vector3.Distance(transform.position, caneLastPos);   // The cane's position difference between last frame and this frame

        caneMoved = (caneMoveDist > caneMoveBenchmarkDist);                 // If cane's moving distance is larger than the benchmark, it's moved in this frame
        caneLastPos = transform.position;                                   // Continuously track the cane's position at the end of each frame
    }


    /// <summary>
    /// Function checks if a collided object is the object we don't want to detect
    /// </summary>
    bool InNotDetectList(Transform other)
    {
        bool selfInList = objDoNotDetect.Contains(other.name);
        bool rootInList = objDoNotDetect.Contains(AccessColliderInfo.GetRootName(other));

        /* Either the object itself is in the list, or its root parent is in the list*/
        return (selfInList || rootInList);
    }


    /// <summary>
    /// Function checks if a collided object is the object we don't want to callout name
    /// </summary>
    bool InNotCallNameList(Transform other)
    {
        /* If the collided object's root name is in the list, we don't call its name
         * When it comes to calling name, we usually want to stop calling name for an
         * object as a whole. That's why we check the name of collider's root object */
        return objDoNotCallName.Contains(AccessColliderInfo.GetRootName(other));
    }


    /// <summary>
    /// Function checks if a collided object is the object we don't want to provide physical vibration for
    /// </summary>
    bool InNotVibrateList(Transform other)
    {
        return objDoNotVibrate.Contains(AccessColliderInfo.GetRootName(other));
    }

}

