using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


public class WayPointTutorial_Circle : MonoBehaviour
{
    #region Main Variables of Circle Exercises

    string currentScene;                 // the name of current scene

    readonly string navSystemName = "NavigationSystem_Circle";
    const int numPoints = 12;            // total number of way points in the circle 
    readonly float defaultDist = 2;      // default distance between way point and the center of user for creating circle

    int exerciseMax = 0;                 // maximum number of circle exercise will be done (aka. Number of way points will be activated)
    int exerciseDone = 0;                // number of exercise done

    int activePointIndex = 0;            // the index of current active point in circle exercise

    Transform objWayPointsTrans;         // the transform of wayPoints object in Navigation system which holds all way points

    Transform bodyTrans;                 // body trans of user will be used for finding the center of circle
    Transform headTrans;                 // head trans will be used to get relative direction of way point to user's facing direction
    HeadCaster headCaster;               // The class which manages the casting from user's head, like rayCast/capsuleCast/etc...

    GameObject goalBar;                  // gameObject of Goal Bar 
    CaneContact caneContact;             // a reference to the CaneContact class
    GestureMenu gestureMenu;             // A reference to the "normal gesture menu" class on User prefab

    bool isRunning = false;              // bool variable indicates if the current wayPointTutorial_Circle is running
    bool detectCast = false;             // bool variable indicates if the we detect the absolute front capsuleCast from user's head 

    float chosenTimer = 0;               // The timer records the time user's cane points to the goal bar
    float chosenTimeThreshold = 1f;      // The threshold time for judging if the user selects a direction

    UserTranslationController userTranslationController;    // the class act as an API for controlling user's translation lock in the scene

    /* List of transform of way points for creating a circle */
    List<Transform> wayPointsTrans = new();

    /* Default position (x & z) of way points for creating circle */
    float[,] defaultPointPosition = new float[12, 2];

    /* Class that controls the verbal callout */
    VerbalManager_General verbalManager_General;

    #endregion


    #region Variables Related to Giving Hints

    //string firstHint = "Try to rotate your head and body to that direction and stay for 1 second to confirm. When your face is in that direction, the drum beat music will become very fast.";
    string firstHint = "Try to rotate your body to that direction and stay for 1 second to confirm. When your face is in that direction, the drum beat music will become very fast.";
    string afterTouchFirst = "Nice! You were in the direction of way point 1 just now. Stay in that direction for 1 second to confirm. You will continuously hear fast drum beat music when you stay in that direction.";
    string afterSecond = "Awesome, you just found two way points! Now, we will stop providing hints. Try to find the rest of the way points using the same rotation technique";

    bool firstHintGiven = false;         // bool variable tracks if the hint for finding the 1st way point given or not 
    bool everHittedGoalBar = false;      // bool variable tracks if the goalBar is ever hitted by head cast or not ===> it's for helping with giving the hint after face touch the direction of the 1st way point
    bool afterFirstGiven = false;        // bool variable tracks if the "after first way point" hint is given or not
    bool afterSecondGiven = false;       // bool variable tracks if the "after second way point" hint is given or not 

    #endregion


    /// <summary>
    /// Awake is the first function be called in Unity Execution loop
    /// </summary>
    void Awake()
    {
        /* Initialize the member variables */
        InitVariables();

        /* Disable the normal gesture menu to prevent unwanted actions */
        DisableNormalGestureMenu();

        /* Preparation for circle point exercise */
        SetupForCirclePointExercise();           
    }


    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        /* When the exercise runs, we implement the process to give hints to user */
        if (isRunning)
            GiveHints();

        /* Check if the capsuleCast from user's head hits GoalBar on every frame
         * if the boolean varialbe "detectCast" is on */
        if (detectCast)
            CheckCastHitGoal();
    }


    /// <summary>
    /// Function controls the flow of giving hints when the way point circle/direction exercise runs
    /// </summary>
    async void GiveHints()
    {
        /* Give the first hint when begin */
        if (exerciseDone == 0 && !firstHintGiven)
        {
            firstHintGiven = true;
            GameObject firstPoint = wayPointsTrans[activePointIndex].gameObject;
            string wayPointOneClockDir = RelativePositionHelper.GetSimpleAndPrettyClockDirection(headTrans.gameObject, firstPoint);
            string toSpeak = $"The 1st way point is at {wayPointOneClockDir} o'clock direction of your face. " + firstHint;
            Debug.Log(toSpeak);
            verbalManager_General.Speak(toSpeak);
        }

        /* Give the hint after user hits the goal bar for the first time (they might forget to stay there) */
        RaycastHit[] hits = headCaster.AbsFrontCapsuleHits;
        if (!everHittedGoalBar && hits != null && Array.Exists(hits, hit => hit.transform.name == "GoalBar") && goalBar.activeSelf)
        {
            everHittedGoalBar = true;
            Debug.Log(afterTouchFirst);
            verbalManager_General.Speak(afterTouchFirst);
        }

        /* Give the hint after user gets the first way point confirmed 
         * When we just enter the section, the active point is still 1 ===> only after success sound played,
         * the "activePointIndex" will change to the index of the next activated way point
         */
        if (exerciseDone == 1 && !afterFirstGiven)
        {
            afterFirstGiven = true;
            int idxFirstPoint = activePointIndex;

            /* Wait for jumpping to the second point */
            while (activePointIndex == idxFirstPoint)
                await Task.Yield();

            /* Just in case if the scene is closed, stop the async process */
            if (currentScene != SceneManager.GetActiveScene().name)
                return;

            /* When its the 2nd point, we give hint */
            GameObject secondPoint = wayPointsTrans[activePointIndex].gameObject;
            string wayPointTwoClockDir = RelativePositionHelper.GetSimpleAndPrettyClockDirection(headTrans.gameObject, secondPoint);
            string toSpeak = $"Great! The 2nd way point is at {wayPointTwoClockDir} o'clock direction of your face. Try to rotate to that direction and confirm that waypoint.";
            Debug.Log(toSpeak);
            verbalManager_General.Speak(toSpeak);
        }

        /* Give the hint after user gets the second way point confirmed */
        if (exerciseDone == 2 && !afterSecondGiven)
        {
            afterSecondGiven = true;
            Debug.Log(afterSecond);
            verbalManager_General.Speak(afterSecond);
        }
    }


    /// <summary>
    /// Function initialize member variables of this class as preparation
    /// </summary>
    void InitVariables()
    {
        currentScene = SceneManager.GetActiveScene().name;
        userTranslationController = GameObject.Find("SceneManager").GetComponent<UserTranslationController>();
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();

        bodyTrans = GameObject.Find("User/Body").transform;
        headTrans = GameObject.Find("User/Head").transform;

        caneContact = GameObject.Find("User/GripPoint/Cane").GetComponent<CaneContact>();
        gestureMenu = GameObject.Find("User/PortableMenu").GetComponent<GestureMenu>();
        headCaster = GameObject.Find("User/Head").GetComponent<HeadCaster>();

        objWayPointsTrans = GameObject.Find(navSystemName + "/WayPoints").transform;
        goalBar = GameObject.Find("GoalBar");
    }


    /// <summary>
    /// Function do steps for preparing circle way point exercise
    /// </summary>
    void SetupForCirclePointExercise()
    {
        /* Get the information of all way points */
        CollectWayPointsInfo();

        /* Add things to not detect list */
        AddToNotDetect();

        /* Generate values for d_circlePointPosition list */
        GenerateDefaultCirclePointPosition();
    }


    /// <summary>
    /// Function generates values for "d_circlePointPosition" list
    /// </summary>
    void GenerateDefaultCirclePointPosition()
    {
        /* Prepare three length variables related to circle */
        float a = defaultDist;
        float b = (defaultDist / 2) * Mathf.Sqrt(3);
        float c = defaultDist / 2;

        /* Generate a temp list of positions */
        float[,] temp = new float[numPoints, 2]
        { { 0, a }, { 0, -a },
          { a, 0}, { -a, 0},
          { b, c}, { c, b},
          { -b, -c}, { -c, -b},
          { b, -c}, { c, -b},
          { -b, c}, { -c, b} };

        /* Shallow copy the generate circle point position to d_circlePointPosition list */
        defaultPointPosition = temp.Clone() as float[,];
    }


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
    /// Function adds things in this scene to not detect list
    /// </summary>
    void AddToNotDetect()
    {
        /* Add Goal Bar to not detect list */
        if (!caneContact.objDoNotDetect.Contains("GoalBar"))
            caneContact.objDoNotDetect.Add("GoalBar");

        /* Add all way points to not detect list */
        foreach (Transform trans in wayPointsTrans)
        {
            if (!caneContact.objDoNotDetect.Contains(trans.name))
                caneContact.objDoNotDetect.Add(trans.name);
        }
    }


    /// <summary>
    /// Function arrange the initial location of the circle way points
    /// </summary>
    void ArrangeCirclePoints(bool isRandom = false)
    {
        /* Origin position of the circle */
        float originX = bodyTrans.position.x;
        float originZ = bodyTrans.position.z;

        /* Y-value of the waypoints */
        float pointY = wayPointsTrans[0].position.y;

        /* Assign position to way points so they will be placed in a circle round the user */
        for (int i = 0; i < wayPointsTrans.Count; ++i)
        {
            /* Based on "isRandom" value to determine random multipler.
             * After applying the multiplier, the distance between way point and 
             * user will become random value. However, the relative direction 
             * between user and way point stays the same. For example, way points
             * will always at user's 12 o'clock, 1 o'clock, etc... directions */
            float randomMulti = 1f;
            if (isRandom)
                randomMulti = (float)UsefulTools.GetRandomDouble(1, 2);

            /* Offset x & z values from origin point */
            float offsetX = defaultPointPosition[i, 0] * randomMulti;
            float offsetZ = defaultPointPosition[i, 1] * randomMulti;

            /* Generate position value for applying to way points */
            Vector3 newPos = new(originX + offsetX, pointY, originZ + offsetZ);

            /* Move the way points */
            wayPointsTrans[i].position = newPos;
        }
    }


    /// <summary>
    /// Function check if the absolute front capsule cast from user's head
    /// hits the "GoalBar" object which wrapped around the active way point
    /// </summary>
    async void CheckCastHitGoal()
    {
        /* Get all hits information from head's absolute front capsule cast */
        RaycastHit[] hits = headCaster.AbsFrontCapsuleHits;

        /* Check if the goal bar is hitted by the abs front capsuleCast from user's head */
        if (hits != null && Array.Exists(hits, hit => hit.transform.name == "GoalBar"))
        {
            chosenTimer += 1f * Time.deltaTime;       // increase the time when the capsuleCast from user's head hits goalBar

            if (chosenTimer > chosenTimeThreshold)    // if the goalBar is hitted for threshold time ===> we counts the user selected direction
                await AfterCastHitGoal();
        }
        else
        {
            if (chosenTimer != 0)                     // if the way point isn't focused, reset "chosenTimer" to 0 if it's not 0 already
                chosenTimer = 0;
        }
    }


    /// <summary>
    /// Function does things-needed after capsuleCast hits GoalBar
    /// </summary>
    async Task<bool> AfterCastHitGoal()
    {
        /* Deactivate the GoalBar once the current way point is deactivated */
        goalBar.SetActive(false);

        /* Update exercise done number */
        exerciseDone++;

        /* Callout when user found way point */
        if (exerciseDone != 2)
            verbalManager_General.Speak("Waypoint " + exerciseDone + ", found!");

        /* Wait for completing things-to-do after triggering a way point */
        await wayPointsTrans[activePointIndex].GetComponent<WayPointManager>().AfterTriggerWayPoint();

        /* If application is running and the current scene is still active, 
         * deactivate the current point and activate a new one */
        if (Application.isPlaying && currentScene == SceneManager.GetActiveScene().name)
        {
            /* Deactivate the current way point */
            wayPointsTrans[activePointIndex].gameObject.SetActive(false);

            /* Continue the exercise if number of exercise done doesn't exceed maximum */
            if (exerciseDone < exerciseMax)
            {
                ContinueExercise();
            }
            else
            {
                Debug.Log("Circle Way Point Exercise done!");
                EndExercise();
            }
        }

        return true;
    }


    /// <summary>
    /// Functions takes actions to continue the exercise after 
    /// capsuleCast hits GoalBar, and the current point is deactivated.
    ///
    /// If it's the last way point ===> activate the 1st way point in list
    /// so we can lead user rotate back to origin facing direaction.
    /// </summary>
    void ContinueExercise()
    {
        if (exerciseDone < exerciseMax - 1)
            RandomActivatePoint();      // randomly activate the next way point
        else
            ActivatePointByIndex(0);    // activate the 1st way point 

        goalBar.SetActive(true);        // reactivate the GoalBar because a new point is activated
    }


    /// <summary>
    /// Functions takes actions to end this circle exercise
    /// if the number of exercises done >= max number of exercises
    /// </summary>
    void EndExercise()
    {
        isRunning = false;                                  // update the "isRunning" to false after finishing the exercise
        detectCast = false;                                 // don't detect capsuleCast anymore
        activePointIndex = 0;                               // revert the "activePointIndex" back to 0
        exerciseDone = 0;                                   // revert "number of exercises done" back to 0
        exerciseMax = 0;                                    // reset the "max number of exercise to do" back to 0

        userTranslationController.ResumeUserTranslation();  // reactivate the translation for user
    }


    /// <summary>
    /// Function randomly picks a way point to activate
    /// </summary>
    void RandomActivatePoint()
    {
        /* Get a random integer among [0, 11] as index of next way point
         * to be activated, but it can't be index of the current active point 
         *
         * Also, when its the last-1 exercise, don't activate the way point at index 0
         * because we want to activate way point at index 0 for the last exercise to 
         * lead the user turns back to origin direction */
        int index = activePointIndex;
        while (index == activePointIndex || (exerciseDone == exerciseMax - 2 && index == 0))
        {
            index = (int)UsefulTools.GetRandomDouble(0, 12);
        }

        ActivatePointByIndex(index);
    }


    /// <summary>
    /// Function activates a specific way point by its index
    /// </summary>
    void ActivatePointByIndex(int index)
    {
        /* Update the last active point index */
        activePointIndex = index;

        /* Activate the selected way point */
        wayPointsTrans[index].gameObject.SetActive(true);

        /* Transport the GoalBar to the newly activated way point */
        Vector3 newPos = wayPointsTrans[index].position;
        goalBar.GetComponent<GoalBarManager>().TransportGoalBar(newPos);
    }


    /// <summary>
    /// Starts the Circle tutorial exercise
    ///
    /// [Parameters]
    /// 1. exerciseNumber ===> Number of exercises in one round
    /// 2. Whether the distance between user and wayPoint will be randomized or not
    /// </summary>
    public void StartExercise(int exerciseNumber = 6, bool randomize = true)
    {
        isRunning = true;                                   // turn the "isRunnig" variable to TRUE when starting exercise
        exerciseMax = exerciseNumber;                       // assign value to "number of exercises to do"

        userTranslationController.PauseUserTranslation();   // temporarily pause the translation of user

        goalBar.SetActive(true);                            // activate the goal bar
        ArrangeCirclePoints(randomize);                     // put way points in scene to their circle position */
        RandomActivatePoint();                              // randomly activate a way point to start the exercise

        detectCast = true;                                  // start to detect "capsuleCast from head hits goalBar"
    }


    /// <summary>
    /// Getter and Setter of variable "isRunning"
    /// </summary>
    public bool IsRunning
    {
        get { return isRunning; }
    }


    /// <summary>
    /// Getter and Setter of variable "detectCast"
    /// </summary>
    public bool DetectCast
    {
        get { return detectCast; }
        set { detectCast = value; }
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

}

