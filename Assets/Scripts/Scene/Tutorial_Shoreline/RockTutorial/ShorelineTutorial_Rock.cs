using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class ShorelineTutorial_Rock : MonoBehaviour
{
    #region Main Variables of ShorelineTutorial_Rock

    bool isRunning = false;                             // If the Shoreling_Rock tutorial is running

    GestureMenu gestureMenu;                            // A reference to the "normal gesture menu" class on User prefab
    GameObject userHead;                                // A reference to user's head
    GameObject rock;                                    // A reference to the cube rock in the scene
    CaneContact caneContact;                            // A reference to the CaneContact class
    GameObject navSpeaker;                              // The NavSpeaker gameObject in Navigation System
    VerbalManager_General verbalManager_General;        // Class control text to speech

    FloorDetector floorDetector;                        // A class checks what object hits the floor
    RockInsideDetector rockInsideDetector;              // A class checks what object hits the inside part of Rock
    BoundaryManager boundaryManager;                    // A class stores the information from "objects of BoundaryDetector class". BoundaryDetector class checks what object hits the sides of boundary
    WaypointDetector activeWaypointDetector;            // The "WaypointDetector.cs" on the active waypoint

    float shorelineTimer = 0;                           // A timer to track how long time left for user to switch to the other shorelining status
    float switchThreshold = 1.5f;                       // [Default = 1.5f] Max time gap for user to stay in one shorelining status (It's kind of like the speed of user when doing shorelinig)
    bool shorelineIsRunning = false;                    // If the user is doing shorelining
    string shorelineStatus = "";                        // The shorelining status of the current frame ("In", "Out", or "")
    string shorelineLastStatus = "";                    // The shorelining status of the last frame

    bool closeCase = true;                              // If the shorelining pattern is off ===> when cane is not touching any shorelining objects
    bool switchCaseGetIn = false;                       // If the 1st type of shorelining switch happened from last frame to this frame 
    bool switchCaseGetOut = false;                      // If the 2nd type of shorelining switch happened from last frame to this frame
    bool unchangeCaseStayIn = false;                    // If the 1st type of shorelining unchanged scenario happened from last frame to this frame
    bool unchangeCaseStayOut = false;                   // If the 2nd type of shorelining unchanged scenario happened from last frame to this frame

    Transform objWayPointsTrans;                        // The transform of wayPoints object in Navigation system which holds all way points
    List<Transform> wayPointsTrans = new();             // List of transform of way points on the corner of the cube rock
    const int numPoints = 4;                            // [Default = 4] Total number of way points in the corner of the cube
    Transform activePoint;                              // The current active way point
    readonly List<string> pointsLocation = new() { "NorthEast", "NorthWest", "SouthWest", "SouthEast" };

    #endregion


    #region Variables Related to In-Game Guidance

    bool beginMessageGiven = false;                 // If the "beginMessage" is given or not 
    bool beginMessageDone = false;                  // If the speech of "beginMessage" is done or not
    bool secondMessageGiven = false;                // If the "secondMessage" is given or not
    bool floorOpenToTouch = false;                  // If this variable is TRUE, after user touches the floor, we will give "afterTouchFloorMessage"
    bool rockOpenToTouch = false;                   // If this variable is TRUE, after user touches the inside of rock, we will give "afterTouchRockMessage"
    bool readyToCompleteOneShoreline = false;       // When this variable is TRUE, we start to track user cane's movement from "inside of rock" to "floor"
    bool boundaryWarnGiven = false;                 // If warned the users about their cane hits the boundary or not
    bool shorelineWarnGiven = false;                // If the user stopped doing shorelining, we give them warning

    int walkCornerIdx = -1;                         // The index of corner which user is walking to
    List<string> corners = new() { "NorthEast", "NorthWest", "SouthWest", "SouthEast" };

    int askToDoShorelineTimeGap = 3;                // [Default = 3] When detecting the shorelining is stopped, we ask them to "do shorelining" after every X seconds (gap)
    float toPointNoticeTimer = 0f;                  // A timer tracks time for helping with giving user "distance to corner/active waypoint" notice when shorelining is ongoing
    float toPointNoticeTimeThreshold = 5f;          // [Default = 5f] After each of this "threshold time gap", we give user "distance to corner/active waypoint" notice

    float totalWalkTime = 0f;                       // From the "user begin to walk to corner" to "user arrived at the last corner", total amount of time user used arrive at all 4 corners
    float shorelineWalkTime = 0f;                   // From the "user begin to walk to corner" to "user arrived at the last corner", total amount of time the user did shorelining 

    readonly string beginMessage = "You are at the 'SouthEast corner' of the rock cube. On the 'left hand side of your cane', it's the 'east side' of the cube. When you walk forward and keep your 'forwarding direction' 'parallel' to this 'east side', you will reach the 'NorthEast corner' of the cube.";
    readonly string secondMessage = "Now, try to lower the cane and touch the floor in front of you.";
    readonly string afterTouchFloorMessage = "Great job! Always keeps the cane 'in contact' with the floor. Now, slide the cane to its left and make it 'stick' into the rock cube.";
    readonly string afterTouchRockMessage = "Nice! Keep your cane inside of the cube for now. Always keep your cane in contact with the floor. Now, slide the cane to the right to leave the cube while touching the floor.";
    readonly string completeOneShorelineMessage = "Congratulation! You justed did one 'shorelining' using your cane. Sliding your cane from the floor into the inside of an object and then slide back to the floor - this is one shorelining. Now, if you still remember. The 'East Side of the cube' is on your left hand side. When you walk 'parallel to' the 'East side of the cube' and towards your facing direction. You will reach 'NorthEast corner of the cube'. By using the shorelining technique you just learned, you can walk 'parallel to' the 'East side' of the cube. Now, please move forward to your heading direction and using the shorelining technique to reach the 'NorthEast corner' of the cube. I will let you know when you arrived.";

    #endregion


    #region Public Functions

    /// <summary>
    /// Function starts the Shoreline_Rock exercise by turning "isRunning" to TRUE
    /// Once it's turned to TRUE, everything in the Update() function will start to run
    /// </summary>
    public void StartExercise()
    {
        isRunning = true;
    }


    /// <summary>
    /// Getter and Setter of variable "isRunning"
    /// </summary>
    public bool IsRunning
    {
        get { return isRunning; }
    }

    #endregion


    #region General Functions

    /// <summary>
    /// Awake is the first function be called in Unity Execution loop
    /// </summary>
    void Awake()
    {
        /* Initialize the member variables */
        InitVariables();

        /* Mute the NavSpeaker */
        MuteNavSpeaker();

        /* Disable the normal Gesture Menu */
        DisableNormalGestureMenu();

        /* Add object to not detect list */
        AddToNotDetect();

        /* Add object to not call name list */
        AddToNotCallName();

        /* Collect way points information */
        CollectWayPointsInfo();
    }


    // Start is called before the first frame update
    void Start()
    {
        /* Init a reference to rockInsideDetector. 
         * Can't be placed in Awake(), because Inside part of rock is not generated yet */
        rockInsideDetector = GameObject.Find("Rock/Inside").GetComponent<RockInsideDetector>();
    }


    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            UpdateShorelineStatus();            // Update the "ShorelineStatus" variable in the beginning of a frame
            CheckShorelineProcess();            // Check if the shorelining is on-going and if the status switched
            GetActivePoint();                   // Get the Active Point
            GetActiveWaypointDetector();        // Get WaypointDetector.cs from the active waypoint

            InGameGuide();                      // Provides all in-game guidances
            CheckShorelineRunning();            // Turn True/False the shorelineIsRunning variable user does shorelining correctly and wrongly

            UpdateShorelineLastStatus();        // Update the "ShorelineLastStatus" variable in the end of a frame
        }
    }


    /// <summary>
    /// Function initialize member variables of this class as preparation
    /// </summary>
    void InitVariables()
    {
        /* Instantiate a reference to user's GestureMenu class */
        gestureMenu = GameObject.Find("User/PortableMenu").GetComponent<GestureMenu>();

        /* Instantiate a reference to user's head gameObject */
        userHead = GameObject.Find("User/Head");

        /* Instantiate a reference to the actual Rock gameObject 
         * ("Rock" is an empty object, and "Outside" is the materialized gameObject in the scene) */
        rock = GameObject.Find("Rock/Outside");

        /* Instantiate the cane contact class */
        caneContact = GameObject.Find("User/GripPoint/Cane").GetComponent<CaneContact>();

        /* Instantiate a reference to the gameObject navSpeaker */
        navSpeaker = GameObject.Find("NavigationSystem/NavSpeaker");

        /* Instantiate a reference to the floorDetector class */
        floorDetector = GameObject.Find("Floor/Outside").GetComponent<FloorDetector>();

        /* Instantiate a reference to the boundaryManager class */
        boundaryManager = GameObject.Find("Boundary").GetComponent<BoundaryManager>();

        /* A reference to the class control text-to-speech */
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();

        /* Initialize the wayPoints gameObject */
        objWayPointsTrans = GameObject.Find("NavigationSystem/WayPoints").transform;
    }

    #endregion


    #region Functions Related to In-Game Guidance

    /// <summary>
    /// Function provides in-game guidance to users 
    /// </summary>
    void InGameGuide()
    {
        GuideGiveFirstMessage();            // Gives the 1st in-game message!
        GuideAskToTouchFloor();             // Ask the user to touch floor after the 1st message is finished

        GuideAfterCaneTouchFloor();         // Provide guidance to user when they used cane to touch floor after receiving our instruction. We will ask them to slide into the rock cube.
        GuideAfterCaneIntoRock();           // Provide guidance to user when their cane slides into the rock from outside (while the cane is touching the floor). We will ask them to slide out of the rock cube.
        GuideAfterCaneOutOfRock();          // Provide guidance to user when their cane slides out of the rock. We will let them know we will start a calibration session.

        WalkToOneCorner();                  // Provide guidance and control flow when user walks to one corner
    }


    /// <summary>
    /// Function gives the 1st in-game message
    /// </summary>
    void GuideGiveFirstMessage()
    {
        /* Give user game-begin message */
        if (!beginMessageGiven)
        {
            beginMessageGiven = true;
            verbalManager_General.SpeakWaitAndCallback(
                beginMessage,
                () => { beginMessageDone = true; }
            );
        }
    }


    /// <summary>
    /// Function asks the users to use their cane to touch floor after the 1st message is conveyed
    /// </summary>
    void GuideAskToTouchFloor()
    {
        /* Give user the second message to ask them touch the floor */
        if (beginMessageDone && !secondMessageGiven)
        {
            secondMessageGiven = true;
            verbalManager_General.SpeakWaitAndCallback(
                secondMessage,
                () => { floorOpenToTouch = true; }
            );
        }
    }


    /// <summary>
    /// Function gives guidance after user use the cane to touch the floor after our instruction
    /// </summary>
    void GuideAfterCaneTouchFloor()
    {
        if (floorOpenToTouch && floorDetector.FloorSlided)
        {
            floorOpenToTouch = false;
            verbalManager_General.SpeakWaitAndCallback(
                afterTouchFloorMessage,
                () => { rockOpenToTouch = true; }
            );
        }
    }


    /// <summary>
    /// Function gives guidance after the cane slides into the inside of rock from the floor
    /// </summary>
    void GuideAfterCaneIntoRock()
    {
        /* If user's cane slided from floor into the rock cube, give them message */
        if (rockOpenToTouch && switchCaseGetIn)
        {
            rockOpenToTouch = false;
            verbalManager_General.SpeakWaitAndCallback(
                afterTouchRockMessage,
                () => { readyToCompleteOneShoreline = true; }
            );
        }
    }


    /// <summary>
    /// Function gives guidance after the cane slides out from inside of the rock
    /// </summary>
    void GuideAfterCaneOutOfRock()
    {
        /* After user moves cane out of the rock and slided to floor 
         * ===> give message and start the calibration process */
        if (readyToCompleteOneShoreline && switchCaseGetOut)
        {
            readyToCompleteOneShoreline = false;
            verbalManager_General.SpeakWaitAndCallback(
                completeOneShorelineMessage,
                () => { walkCornerIdx = 0; }
            );
        }
    }


    /// <summary>
    /// Function controls the flow when user walks to one corner/waypoint of the rock.
    /// It also speak the ending message when the user reaches that corner
    /// </summary>
    void WalkToOneCorner()
    {
        /* Equal to -1 means the user is not in the process of walking to a corner, thus return */
        if (walkCornerIdx == -1)
            return;

        /* Record "times" when users on their way walks to a way point */
        UpdateWalkTimes();

        /* Providing guidance when walking to the active waypoint */
        WhenWalkToActivePoint();

        /* To do when user hits the active waypoint */
        if (activeWaypointDetector.HitByUser)
        {
            /* Save the value of "walkCornerIdx", and change it to -1 to prevent entering this section again */
            int saveCornerIdx = walkCornerIdx;
            walkCornerIdx = -1;

            /* Dynamically craft the notice messge */
            string nextPointDir = GetNextPointDirection();
            string reachCornerMessage = "";

            /* If it's the last corner, we use specialized message. If not, we use auto-generated message */
            string currCorner = corners[saveCornerIdx];
            if (saveCornerIdx == corners.Count - 1)
            {
                float pctShorelineWalk = Mathf.Ceil(shorelineWalkTime / totalWalkTime * 100);
                Debug.Log($"Total Time: {totalWalkTime} || Shoreline Time: {shorelineWalkTime} || pct: {pctShorelineWalk}");
                reachCornerMessage = $"Congratulation! You are now at your starting position, '{currCorner}' corner. When you had the round tour along 4 sides of the rock cube, you were doing shorelining for {pctShorelineWalk} percent of the time. Good job! You have completed this 'Shorelining Beginner Tutorial'.";
            }
            else
            {
                string nextCorner = corners[saveCornerIdx + 1];
                reachCornerMessage = $"You arrived at '{currCorner}' corner of the rock cube. Now, please turn to {nextPointDir} o'clock direction of your face, and walk to the {nextCorner} corner of the cube";
            }

            /* Give notice to the users */
            verbalManager_General.SpeakWaitAndCallback(
                reachCornerMessage,
                () => {
                    /* If user reached the last corner, we update "isRunning" variable to end the tutorial
                     * Othewise, we increase the "walkCornerIdx" to next index to continue */
                    if (saveCornerIdx == corners.Count - 1)
                        isRunning = false;
                    else
                        walkCornerIdx = saveCornerIdx + 1;
                }
            );
        }
    }


    /// <summary>
    /// Function updates walk related times when user walks to four corners 
    /// </summary>
    void UpdateWalkTimes()
    {
        /* Increment the totalWalkTime */
        totalWalkTime += Time.deltaTime;

        /* Conditionally increase the shorelineWalkTime */
        if (shorelineIsRunning)
            shorelineWalkTime += Time.deltaTime;
    }


    /// <summary>
    /// 
    /// Function contains the guidance we want to give to user when they are walking to a corner of the rock cube.
    /// When we say the corner ===> we are actually referring to the active waypoint at the corner.
    ///
    /// [The notices we will provide]
    /// 1. Warning notice when user bumps into the boundary wall
    /// 2. Warning notice when user is not doing shorelining
    /// 3. Notice the user about their distance to the corner when they does shorelining
    /// 
    /// </summary>
    void WhenWalkToActivePoint()
    {
        /* [Priority of Notices]
         * When user walks to an active waypoint, the priorities of speeches are:
         * boundaryWarning > continueShorelineWarning > tellDistanceToActiveWaypoint */

        GiveTouchBoundaryWarning();
        GiveDoShorelineWarning();
        GiveDistanceToPointNotice();
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
            string rockDirection = GetRockDirection();
            string boundaryWarnMessage = $"Stop! You're heading the wrong direction. Please turn to your {rockDirection} o'clock direction to go back to the Rock Cube";

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
    /// Function gives warning to users when they stopped doing shorelining when walking to active waypoint
    /// </summary>
    void GiveDoShorelineWarning()
    {
        /* Give shoreline warning if the following criterias meet:
         * 1. User stopped doing shorelining
         * 2. BoundaryWarning is not ongoing. 
         * 3. ShorelineWarning is not given yet. */
        if (!boundaryWarnGiven && !shorelineWarnGiven && !shorelineIsRunning)
        {
            /* Update the entrance controller variable after entering */
            shorelineWarnGiven = true;

            /* Give the message when user is not doing shorelining */
            string cornerDir = GetDirectionToPointText();
            string askToDoShorelineMessage = $"Please do shorelining against the edge of rock! {cornerDir}. Use shorelining to walk to it!";
            verbalManager_General.SpeakWaitAndCallback(
                askToDoShorelineMessage,
                async () => {
                    await Task.Delay(askToDoShorelineTimeGap * 1000);
                    shorelineWarnGiven = false;
                }
            );
        }

        /* If user started shorelining when the shorelining warning is ongoing
         * ===> Stop the shorelining warning immediately. Otherwise, the user might be confused */
        if (shorelineIsRunning && shorelineWarnGiven)
        {
            verbalManager_General.StopSpeak();
        }
    }


    /// <summary>
    /// Function gives notice to user about their distance to the active waypoint
    /// during the process that they are walking toward that place
    /// </summary>
    void GiveDistanceToPointNotice()
    {
        /* Let the users know their distance to the active waypoint if the following criterias meet:
         * 1. The boundary warning is not ongoing
         * 2. The shoreline warning (asks the users to continue shorelining) is not ongoing 
         * 3. After every X seconds of walking */
        toPointNoticeTimer += 1f * Time.deltaTime;
        if (!boundaryWarnGiven && !shorelineWarnGiven && toPointNoticeTimer > toPointNoticeTimeThreshold)
        {
            toPointNoticeTimer = 0f;                                                        // Reset the timer
            string distToPointMessage = GetDistanceToPointText();                           // Get the text about user's distance to the corner/active waypoint
            verbalManager_General.Speak(distToPointMessage);                                // Speak the message
        }
    }


    /// <summary>
    /// Function returns a text about user's distance to a corner/active waypoint
    /// </summary>
    string GetDistanceToPointText()
    {
        string distToActivePoint = GetActivePointDistance();                              // User's distance (In feet) to active waypoint
        string pointLoc = GetActivePointCornerName();                                     // The location of the current active waypoint ===> eg. "NorthEast", "NorthWest", etc...
        string distToPointMessage = $"{distToActivePoint} to {pointLoc} corner.";         // The message about user's relative distance to the corner/active_waypoint
        return distToPointMessage;
    }


    /// <summary>
    /// Function returns a text about corner/active waypoint is at user's what direction
    /// </summary>
    string GetDirectionToPointText()
    {
        string dirToActivePoint = GetActivePointDirection();                                                // Active point is at what direction of user's face
        string pointLoc = GetActivePointCornerName();                                                       // The location of the current active waypoint ===> eg. "NorthEast", "NorthWest", etc...
        string dirToPointMessage = $"{pointLoc} corner is at {dirToActivePoint} o'clock of your face.";     // The message about the corner/active_waypoint's relative direction to user's face
        return dirToPointMessage;
    }

    #endregion


    #region Functions Gets WayPoint Related Infos

    /// <summary>
    /// Function gets all way points' info
    /// </summary>
    void CollectWayPointsInfo()
    {
        // if there's no wayPoint trans in the "wayPointsTrans" list, add them
        if (wayPointsTrans.Count == 0)
        {
            for (int i = 0; i < numPoints; ++i)
                wayPointsTrans.Add(objWayPointsTrans.GetChild(i));
        }
    }


    /// <summary>
    /// Function gets the only active point in the scene 
    /// </summary>
    void GetActivePoint()
    {
        /* Check all the way points to find the active one */
        foreach (Transform pointTrans in wayPointsTrans)
        {
            if (pointTrans.gameObject.activeSelf)
            {
                activePoint = pointTrans;
                return;
            }
        }

        /* If no one is active, set the activePoint to NULL */
        activePoint = null;
    }


    /// <summary>
    /// Function gets WaypointDetector.cs from the active waypoint
    /// </summary>
    void GetActiveWaypointDetector()
    {
        /* If there is active waypoint */
        if (activePoint)
            activeWaypointDetector = activePoint.GetComponent<WaypointDetector>();
    }


    /// <summary>
    /// Function gets the corner name where the current active waypoint is placed at
    /// </summary>
    string GetActivePointCornerName()
    {
        int idxActivePoint = wayPointsTrans.IndexOf(activePoint);            // The index of the active waypoint
        return pointsLocation[idxActivePoint];                               // The location of the current active waypoint ===> eg. "NorthEast", "NorthWest", etc...
    }

    #endregion


    #region Functions Related to Get Direction and Distance Infos

    /// <summary>
    /// Function gets the rock's relative "clock direction" to user's head
    /// </summary>
    string GetRockDirection()
    {
        return RelativePositionHelper.GetSimpleAndPrettyClockDirection(userHead, rock);
    }


    /// <summary>
    /// Function gets the active waypoint's relative "clock direction" to user's head
    /// </summary>
    string GetActivePointDirection()
    {
        return RelativePositionHelper.GetSimpleAndPrettyClockDirection(userHead, activePoint.gameObject);
    }


    /// <summary>
    /// Function gets the active waypoint's relative distance to user
    /// </summary>
    string GetActivePointDistance()
    {
        /* Get distance with measurement based on user's measurement system choice in settingsMenu */
        switch (SettingsMenu.measureSystem)
        {
            case "US":
                return RelativePositionHelper.GetDistance(userHead, activePoint.gameObject);
            case "Imperial":
                return RelativePositionHelper.GetDistance(userHead, activePoint.gameObject, useFeet: false);
            default:
                return "None";
        }
    }


    /// <summary>
    /// Function gets the "NEXT" active waypoint's relative "clock direction" to user's head
    /// </summary>
    string GetNextPointDirection()
    {
        /* Get the index of the next potential active way point */
        int nextIdx = wayPointsTrans.IndexOf(activePoint) + 1;

        /* If the index of next potential waypoint exceed the range 
         * ===> the next waypoint doesn't exist! */
        if (nextIdx >= wayPointsTrans.Count)
            return "NA";

        /* If the next waypoint exists */
        return RelativePositionHelper.GetSimpleAndPrettyClockDirection(userHead, wayPointsTrans[nextIdx].gameObject);
    }

    #endregion


    #region Functions Provides Core Shorelining Features

    /// <summary>
    /// 
    /// Function gets the shorelining status of current frame
    /// 
    /// [Shoreline status]
    /// "In" ===> cane is in the alert part of shorelining
    /// "Out" ===> cane is in the floor part of shorelining
    /// "" ===> cane is not in alert or floor part
    /// 
    /// </summary>
    void UpdateShorelineStatus()
    {
        bool floorSlided = floorDetector.FloorSlided;
        bool rockInsideSlided = rockInsideDetector.RockInsideSlided;

        /* Update the shoreline status */
        if (rockInsideSlided)
            shorelineStatus = "In";
        else if (!rockInsideSlided && floorSlided)
            shorelineStatus = "Out";
        else
            shorelineStatus = "";
    }


    /// <summary>
    /// Function assign value to "shorelineLastStatus" in the end of "Update()" function
    /// </summary>
    void UpdateShorelineLastStatus()
    {
        shorelineLastStatus = shorelineStatus;
    }


    /// <summary>
    /// Function check the shorelining process.
    /// It checks if the shorelining is ongoing? If the shorelining status switched? etc...
    /// </summary>
    void CheckShorelineProcess()
    {
        /* The cases indicates the shorelining status is switched from the last frame to this frame.
         * Switch means status changed like ["In" to "Out"] or ["Out" to "In"]*/
        switchCaseGetIn = shorelineStatus == "In" && shorelineLastStatus == "Out";
        switchCaseGetOut = shorelineStatus == "Out" && shorelineLastStatus == "In";

        /* The cases indicates the shorelining status is unchanged from the last frame to this frame. */
        unchangeCaseStayIn = shorelineStatus == "In" && shorelineLastStatus == "In";
        unchangeCaseStayOut = shorelineStatus == "Out" && shorelineLastStatus == "Out";

        /* The cases indicates the shorelining is not ongoing. Cane is not touching any shoreling objects */
        closeCase = shorelineStatus == "";
    }


    /// <summary>
    /// Function turns "shorelingIsRunning" variable to TRUE if user is doing shorelining
    /// Otherwise, turn it to FALSE
    /// </summary>
    void CheckShorelineRunning()
    {
        /* When we detect a switch between "In" and "Out" shorelining status */
        if (switchCaseGetIn || switchCaseGetOut)
        {
            /* Update the variable to TRUE */
            shorelineIsRunning = true;

            /* Reset the shoreline timer to 0 after each switch */
            shorelineTimer = 0f;
        }

        /* Update the variable to FALSE if the cane leaves contact with the shoreline objects or floor */
        if (closeCase)
            shorelineIsRunning = false;

        /* Update timer. If the time for "switch shoreline status" arrives but the user
         * doesn't switch it, we will turn "shorelineIsRunning" to FALSE */
        shorelineTimer += 1f * Time.deltaTime;
        if (shorelineTimer > switchThreshold && (unchangeCaseStayIn || unchangeCaseStayOut))
            shorelineIsRunning = false;
    }

    #endregion


    #region Functions Deals with Miscellaneous Tasks

    /// <summary>
    /// Function adds things in this scene to not call name list 
    /// </summary>
    void AddToNotCallName()
    {
        /* Add the rock to not call name list */
        if (!caneContact.objDoNotCallName.Contains("Rock"))
            caneContact.objDoNotCallName.Add("Rock");
    }


    /// <summary>
    /// Function adds things in this scene to not detect list
    /// </summary>
    void AddToNotDetect()
    {
        /* Add the Boundary to not detect list */
        if (!caneContact.objDoNotDetect.Contains("Boundary"))
            caneContact.objDoNotDetect.Add("Boundary");
    }


    /// <summary>
    /// Function which mute the navSpeaker (turns the volume to zero)
    /// </summary>
    void MuteNavSpeaker()
    {
        navSpeaker.GetComponent<AudioSource>().volume = 0;
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

    #endregion

}

