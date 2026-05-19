using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShorelineTutorial_Quiz : MonoBehaviour
{
    bool isRunning = false;                         // If the exercise is running
    GestureMenu gestureMenu;                        // A reference to the "normal gesture menu" class on User prefab

    NavigationManager navigationManager;            // The navigation manager in the scene
    VerbalManager_General verbalManager_General;    // The class urns the text to speech
    CaneContact caneContact;                        // A reference to the CaneContact class

    DetectCaneContact wallHitByCaneDetector;        // Class object tracks wall hits by cane activity
    DetectCaneContact InvWallHitByCaneDetector;     // Class object tracks invisible wall hits by cane activity
    DetectUserContact InvWallHitByUserDetector;     // Class object tracks invisible wall hits by user activity
    DetectUserContact checkTwoHitByUserDetector;    // Class object tracks checkPoint2 hits by user activity
    DetectCaneContact tableHitByCaneDetector;       // Class object tracks table hits by cane activity
    DetectUserContact pointHitByUserDetector;       // Class object tracks waypoint hits by user activity
    BoundaryManager boundaryManager;                // Class stores the information from "objects of BoundaryDetector class". BoundaryDetector class checks what object hits the sides of boundary

    GameObject userHead;                            // The user avatar's head
    GameObject checkPoint1;                         // The checkPoint1
    GameObject waypoint;                            // The only waypoint in the scene
    GameObject navSpeaker;                          // The NavSpeaker gameObject in Navigation System

    int processNo = -1;                             // The No. of the process which is running for this exercise
    int saveNo = -1;                                // The variable temporary holds the value of "processNo" after entering one process

    bool boundaryWarnGiven = false;                 // If warned the users about their cane hits the boundary or not
    float saveSpeakerVolume;                        // Save the navigation speaker's original volume before lower it 
    float lowSpeakerVolume = 0.1f;                  // The "lower volume" we want to set the navSpeaker to

    string promptToPointMessage = "Great! You are in 'the game' now. 'one waypoint' will show up in this virtual room after I finish speaking. By then, you will hear the '3D drum beat sound' emit from that way point. Try to locate the direction of this waypoint and walk to it. Remember to use the 'waypoint locating techniques' you previously learned.";
    string afterCaneHitEntranceMessage = "Awesome! Your cane touched the 'entrance' just now! Now, please keep doing shorelining using your cane. And move several steps forward. When you reach a moment that - the sliding pattern changes from 'floor to vibration' to 'floor to floor'. You body will be almost at the entrance. At that point, you can turn to the direction of the 3D drum beat sound. Try to 'go through the entrance'.";
    string afterUserHitEntranceMessage = "Nice! You are at the entrance, please step forward to go through the entrance!";
    string afterTouchTableMessage = "Oops! Here is the 'second' challenge. You are already super close to the 'waypoint', but a 'solid' 'large metal box' appear on your way. What should you do? 'Yesss', please do 'shorelining' to walk along the edge of table and 'circumvent' the table. I promise! The waypoint is on the other side of this table. Please try it now.";
    string afterTouchPointMessage = "Congratulation, You found the waypoint! You have conquered two 'extremely hard' challenges in this room. You did an amazing job! Remember to use 'shorelining' if you met similar situations in other virtual rooms later. Also, you are always welcome to come back to this room and enhance you shorelining skill again!";

    #region Public Functions

    /// <summary>
    /// Function starts the Shoreline_Rock exercise by turning "isRunning" to TRUE
    /// Once it's turned to TRUE, everything in the Update() function will start to run
    /// </summary>
    public void StartExercise()
    {
        isRunning = true;           // Turn on isRunning variable
        processNo = 0;              // The 1st process is at index 0
    }


    /// <summary>
    /// Getter and Setter of variable "isRunning"
    /// </summary>
    public bool IsRunning
    {
        get { return isRunning; }
    }

    #endregion


    /// <summary>
    /// Awake is the first function be called in Unity Execution loop
    /// </summary>
    void Awake()
    {
        /* Initialize the member variables */
        InitVariables();

        /* Disable the normal Gesture Menu */
        DisableNormalGestureMenu();
    }


    /// <summary>
    /// Initialize member variables of the class
    /// </summary>
    void InitVariables()
    {
        gestureMenu = GameObject.Find("User/PortableMenu").GetComponent<GestureMenu>();
        navigationManager = GameObject.Find("NavigationSystem").GetComponent<NavigationManager>();
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        caneContact = GameObject.Find("User/GripPoint/Cane").GetComponent<CaneContact>();

        wallHitByCaneDetector = GameObject.Find("Wall/Outside").GetComponent<DetectCaneContact>();
        InvWallHitByCaneDetector = GameObject.Find("InvisibleWall").GetComponent<DetectCaneContact>();
        InvWallHitByUserDetector = GameObject.Find("InvisibleWall").GetComponent<DetectUserContact>();
        checkTwoHitByUserDetector = GameObject.Find("CheckPoint2").GetComponent<DetectUserContact>();
        tableHitByCaneDetector = GameObject.Find("Table/Outside").GetComponent<DetectCaneContact>();
        pointHitByUserDetector = GameObject.Find("NavigationSystem/WayPoints/WayPoint").GetComponent<DetectUserContact>();
        boundaryManager = GameObject.Find("Boundary").GetComponent<BoundaryManager>();

        userHead = GameObject.Find("User/Head");
        checkPoint1 = GameObject.Find("CheckPoint");
        waypoint = GameObject.Find("NavigationSystem/WayPoints/WayPoint");
        navSpeaker = GameObject.Find("NavigationSystem/NavSpeaker");
    }


    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            GiveTouchBoundaryWarning();         // Give warning if user's cane hits boundary

            GuidePromptToPoint();               // In the beginning, prompt the user to walk to the waypoint
            GuideAfterTouchWall();              // After user's cane hits the wall when they are following the waypoint sound
            GuideAfterCaneHitEntrance();        // After user's cane hits the entrance
            GuideAfterUserHitsEntrance();       // After user's body hits the entrance
            GuideAfterGoThroughEntrance();      // After user's body go through the entrance
            GuideAfterCaneHitsTable();          // After user's cane hits the table 
            GuideAfterHitsPoint();              // After user' body hits the waypoint
        }
    }


    /// <summary>
    /// Function give guidance and encourage user to find the waypoint
    /// </summary>
    void GuidePromptToPoint()
    {
        /* Only start the code below if it's time */
        if (processNo == 0)
        {
            /* Guard to prevent multiple entering the section */
            ProcessGuard();

            /* Speak the message */
            HandleSpeakAndAfter(promptToPointMessage);
        }
    }


    /// <summary>
    /// Function give guidance when user's cane touches the wall
    /// </summary>
    void GuideAfterTouchWall()
    {
        /* Only start the code below if it's time */
        if (processNo == 1 && wallHitByCaneDetector.SlidedByCane)
        {
            /* Guard to prevent multiple entering the section */
            ProcessGuard();

            /* Craft the message */
            string toCheckOneDir = RelativePositionHelper.GetSimpleAndPrettyClockDirection(userHead, checkPoint1);
            string toCheckOneDist = GetUserToObjDist(checkPoint1);
            string afterTouchWallMessage = $"Oops! Your cane hitted the 'wall'. The waypoint sound is in front of you. But a wall 'lies between' you and the waypoint. It means the 'way point' is on the 'other side' of this wall. You need to 'find an entrance' to 'get to the other side'. Please turn to '{toCheckOneDir}' o'clock direction of your head. Walk along this side of the wall. Using shorelining to find the entrance. The entrance is about {toCheckOneDist} away from you. I will let you know when you arrived.";

            /* Speak the message */
            HandleSpeakAndAfter(afterTouchWallMessage);
        }
    }


    /// <summary>
    /// Function give guidance when user's cane hits the InvisibleWall, which is the entrance we prompted the user to find
    /// </summary>
    void GuideAfterCaneHitEntrance()
    {
        /* Only start the code below if it's time */
        if (processNo == 2 && InvWallHitByCaneDetector.SlidedByCane)
        {
            /* Guard to prevent multiple entering the section */
            ProcessGuard();

            /* Speak the message */
            HandleSpeakAndAfter(afterCaneHitEntranceMessage);
        }
    }


    /// <summary>
    /// Function give guidance when user's body hits the InvisibleWall, which is the entrance we prompted the user to find
    /// </summary>
    void GuideAfterUserHitsEntrance()
    {
        /* Only start the code below if it's time */
        if (processNo == 3 && InvWallHitByUserDetector.HitByUser)
        {
            /* Guard to prevent multiple entering the section */
            ProcessGuard();

            /* Speak the message */
            HandleSpeakAndAfter(afterUserHitEntranceMessage);
        }
    }


    /// <summary>
    /// Function give guidance when user's body go through the InvisibleWall and hits the checkPoint2
    /// </summary>
    void GuideAfterGoThroughEntrance()
    {
        /* Only start the code below if it's time */
        if (processNo == 4 && checkTwoHitByUserDetector.HitByUser)
        {
            /* Guard to prevent multiple entering the section */
            ProcessGuard();

            /* Craft the message */
            string pointDist = GetUserToObjDist(waypoint);
            string afterHitCheckTwoMessage = $"Great! You just went through the entrance, and you are now at the other side of the wall! Please follow the way point sound again to find that way point. As a quick hint. The waypoint is about {pointDist} away from you.";

            /* Speak the message */
            HandleSpeakAndAfter(afterHitCheckTwoMessage);
        }
    }


    /// <summary>
    /// Function give guidance when user's cane hits the table
    /// </summary>
    void GuideAfterCaneHitsTable()
    {
        if (processNo == 5 && tableHitByCaneDetector.SlidedByCane)
        {
            /* Guard to prevent multiple entering the section */
            ProcessGuard();

            /* Speak the message */
            HandleSpeakAndAfter(afterTouchTableMessage);
        }
    }


    /// <summary>
    /// Function give guidance when user's body hits the waypoint
    /// </summary>
    void GuideAfterHitsPoint()
    {
        if (processNo == 6 && pointHitByUserDetector.HitByUser)
        {
            /* Guard to prevent multiple entering the section */
            ProcessGuard();

            /* Speak the message */
            HandleSpeakAndAfter(afterTouchPointMessage);
        }
    }


    /// <summary>
    /// Function acts as a process guard to prevent duplicated entering a process
    /// </summary>
    void ProcessGuard()
    {
        /* Update the "processNo" to -1 after enter a section to prevent duplicated entering */
        saveNo = processNo;
        processNo = -1;
    }


    /// <summary>
    /// Function handle speak for this exercies, and do routine things before and after speak
    /// </summary>
    void HandleSpeakAndAfter(string message)
    {
        /* Temporary add object name to not callout list */
        AddToNotCallName("Wall");
        AddToNotCallName("Table");

        /* Lower the NavSpeaker's volume to make speech more clear */
        LowerNavSpeakerVolume();

        /* Speak the message */
        verbalManager_General.SpeakWaitAndCallback(
            message,
            () => {

                /* Allow the next process to start */
                processNo = saveNo + 1;

                /* Remove object name from callout list */
                RemoveFromNotCallName("Wall");
                RemoveFromNotCallName("Table");

                /* Reset NavSpeaker's volume back to its original */
                ResetNavSpeakerVolume();

                /* Start the navigation system when the 1st process end */
                if (saveNo == 0)
                    navigationManager.RestartNavSystem();

                /* Number '6' means it's after the last process */
                if (saveNo == 6)
                    isRunning = false;
            }
        );
    }


    /// <summary>
    /// Function gives warning to the users when their cane touches the boundary
    /// </summary>
    void GiveTouchBoundaryWarning()
    {
        /* Give warning when the cane hits the boundary */
        if (!boundaryWarnGiven && boundaryManager.BoundarySlided)
        {
            /* Update the entrance controller variable after entering */
            boundaryWarnGiven = true;

            /* Dynamically get the message */
            string pointDirection = RelativePositionHelper.GetSimpleAndPrettyClockDirection(userHead, waypoint);
            string boundaryWarnMessage = $"Stop! You're heading the wrong direction. Please turn to {pointDirection} o'clock direction of your face to walk to the waypoint";

            /* Give the message when user's cane hits the boundary */
            verbalManager_General.SpeakWaitAndCallback(
                boundaryWarnMessage,
                () => { boundaryWarnGiven = false; }
            );
        }

        /* If the cane moved out of boundary sides (BoundarySlided == FALSE). And the boundaryWarning is going.
         * ===> Stop the warning immediately. Otherwise, the user might be confused */
        if (!boundaryManager.BoundarySlided && boundaryWarnGiven)
        {
            verbalManager_General.StopSpeak();
        }
    }


    /// <summary>
    /// Function gets the user's distance to the a gameObject in the scene
    /// </summary>
    string GetUserToObjDist(GameObject gameObject)
    {
        if (SettingsMenu.measureSystem == "US")
            return RelativePositionHelper.GetDistance(userHead, gameObject);
        else if (SettingsMenu.measureSystem == "Imperial")
            return RelativePositionHelper.GetDistance(userHead, gameObject, useFeet: false);
        else
            return "None";
    }


    /// <summary>
    /// Function adds specific name of an object to the "NotCallName" list
    /// </summary>
    void AddToNotCallName(string objName)
    {
        /* Add the rock to not call name list */
        if (!caneContact.objDoNotCallName.Contains(objName))
            caneContact.objDoNotCallName.Add(objName);
    }


    /// <summary>
    /// Function removes specific name of an object from the "NotCallName" list
    /// </summary>
    void RemoveFromNotCallName(string objName)
    {
        caneContact.objDoNotCallName.Remove(objName);
    }


    /// <summary>
    /// Function disable the normal gesture menu on User prefab.
    /// Because we don't need it in this scene. We don't want the user
    /// accidentally turn on menu when we give instruction 
    /// </summary>
    void DisableNormalGestureMenu()
    {
        gestureMenu.enabled = false;
    }


    /// <summary>
    /// Function which temporarily lower the navSpeaker's volume
    /// </summary>
    void LowerNavSpeakerVolume()
    {
        saveSpeakerVolume = navSpeaker.GetComponent<AudioSource>().volume;      // save the volume 
        navSpeaker.GetComponent<AudioSource>().volume = lowSpeakerVolume;       // lower the volume
    }


    /// <summary>
    /// Function which turns back the navSpeaker's volume
    /// </summary>
    void ResetNavSpeakerVolume()
    {
        navSpeaker.GetComponent<AudioSource>().volume = saveSpeakerVolume;      // reset to the original volume
    }

}

