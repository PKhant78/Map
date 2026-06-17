using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class WayPointTutorial_Walk : MonoBehaviour
{
    #region Main Variables of Walk Exercises

    readonly string navSystemName = "NavigationSystem_Walk";
    const int numPoints = 5;                      // Total number of way points in navigation system for generating a walking path
    List<Transform> wayPointsTrans = new();       // List of transform of way points for creating a circle

    Transform objWayPointsTrans;                  // The transform of wayPoints object in Navigation system which holds all way points
    Transform bodyTrans;                          // The transform of the user body game object
    Transform headTrans;                          // The transform of user's head
    CaneContact caneContact;                      // Cane contact class
    VerbalManager_General verbalManager_General;  // The class which gives speech

    float pointsDist = 2;                         // [Default = 2] Distance between this way point to the next point

    float angleThreshold = 60;                    // [Default = 60] The maximum difference in angle between direction for generating this way point and next way point
    float largeAngleThreshold = 135;              // [Default = 135] A backup angle if the "angleThreshold" doesn't work! Prevent infinite loop. When a previous way point's generating direction is point out to an vertice of bounding box, it needs at least 135 degree to place the next point 
    readonly int maxTry = 10;                     // [Default = 10] Maximum times for trying to place a way point before switch to larger AngleThreshold

    float boundBoxWidth = 5;                      // [Default = 5] The width of bounding box (maybe with user as the center...or something else). Generated way point can't exceed the bounding box
    bool userAsBoxCenter = false;                 // [Default = False] boolean variable indicates if we want to use user as the center of bounding box
    Vector3 boxCenter;                            // Variable represents the position of bounding box center

    Vector2 lastDirection;                        // The last direction which used when placing the last way point

    NavigationManager navigationManager_walk;     // The navigation system for random walk exercise

    bool isRunning = false;                       // Bool variable indicates if the current wayPointTutorial_Walk is running
    GestureMenu gestureMenu;                      // A reference to the "normal gesture menu" class on User prefab

    #endregion


    #region Variables Related to Checking User Moving and Give Hint

    Transform activePoint;                        // The current active way point
    Transform lastActivePoint;                    // The active way point in the last frame

    Vector3 bodyPos;                              // The current position of user's body
    Vector3 bodyLastPos;                          // The position of user's body when we check it the last time
    float bodyMoveThreshold = 0.5f;               // [Default = 0.5f] A threshold distance to determine if the user's body moved
    float bodyCheckTimer = 0f;                    // Timer for seeing when should we check if user moved 
    float bodyCheckTimeThreshold = 20f;           // Gap needed before doing a "new check" after "checking user moved the last time"
    bool initBodyLastPos = false;                 // If the "bodyLastPos" is initialized yet or not
    bool userMoved = true;                        // A variable indicates if user moved in the last X seconds

    Vector3 headRot;                              // The current rotation (Eular Angles) of user's head
    Vector3 headLastRot;                          // The rotation of user's head when we check it the last time
    float headRotYThreshold = 5f;                 // [Default = 5f] A threshold eular angle to determine if the user head rotated in the past X seconds
    float headCheckTimer = 0f;                    // Timer for seeing when should we check if user head rotated
    float headCheckTimeThreshold = 5f;            // Every X seconds, check if user's head has significant rotation
    bool initHeadLastRot = false;                 // If the "headLastRot" is initialized yet or not
    bool headRotated = false;                     // Variable records if the user head rotated

    bool initHintGiven;                           // Bool variable indicates if the initial hint is given or not
    //string initHint = "Try to find the waypoint using the techniques you learned and walk to it. Remember, always walking towards the direction that you can continuously hear 'fast' drum beat music.";
    string initHint = "Try to find the waypoint using the techniques you learned and walk to it. When you long-press the screen for two seconds, the virtual character begins to move. Remember, always walking towards the direction that you can continuously hear 'fast' drum beat music.";


    #endregion


    /// <summary>
    /// Awake is the first funtion called in Unity execution loop
    /// </summary>
    private void Awake()
    {
        InitVariables();                        // Initiate member variables
        CollectWayPointsInfo();                 // Collect way points information and put them into a list
        DisableNormalGestureMenu();             // Disable the normal gesture menu to prevent unwanted actions
    }


    /// <summary>
    /// Update runs at each frame 
    /// </summary>
    void Update()
    {
        /* When the system is running, get the only active point at every frame */
        if (isRunning)
        {
            GetActivePoint();           // Get the current active way point

            GiveInitHint();             // Give the initial hint when the game started

            CheckIfUserMoved();         // Check if the user moved
            CheckIfHeadRotated();       // Check if the user head rotated
            GiveHintIfUserNotMove();    // Give user hint if the user does not move for a while

            CalloutWhenArrive();        // Callout when user arrives at a way point

            ModifyLastActivePoint();    // Assign value of "activePoint" to "lastActivePoint"
        }
    }


    /// <summary>
    /// Function controls the flow of providing initial hint
    /// </summary>
    void GiveInitHint()
    {
        if (wayPointsTrans.IndexOf(activePoint) == 0 && !initHintGiven)
        {
            initHintGiven = true;
            verbalManager_General.Speak(initHint);
        }
    }


    /// <summary>
    /// Function initialize member variables of this class as preparation
    /// </summary>
    void InitVariables()
    {
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();

        navigationManager_walk = GameObject.Find("NavigationSystem_Walk").GetComponent<NavigationManager>();
        objWayPointsTrans = GameObject.Find(navSystemName + "/WayPoints").transform;

        bodyTrans = GameObject.Find("User/Body").transform;
        headTrans = GameObject.Find("User/Head").transform;

        caneContact = GameObject.Find("User/GripPoint/Cane").GetComponent<CaneContact>();
        gestureMenu = GameObject.Find("User/PortableMenu").GetComponent<GestureMenu>();
    }


    #region Main Functions of Walk Exercises

    /// <summary>
    /// Setter and getter function of isRunning boolean variable
    /// </summary>
    public bool IsRunning
    {
        get { return isRunning; }
        set { isRunning = value; }
    }


    /// <summary>
    /// Function starts the random walk exercise
    /// "drawBoundBox" means if we want to see the layout of the bounding box in the scene (5 vertices of the box)
    /// </summary>
    public async void StartExercise(bool drawBoundBox = false)
    {
        isRunning = true;

        /* Setup important values for the walking exercise */
        SetupForWalkPointExercise(drawBoundBox, userAsBoxCenter);

        /* Start the navigation system */
        navigationManager_walk.RestartNavSystem();

        /* Wait for navigation system to end */
        while (navigationManager_walk.IsRunning)
        {
            await Task.Yield();
        }

        /* Updates isRunning variable */
        isRunning = false;
    }


    /// <summary>
    /// Function sets up important values for the walking exercise
    /// </summary>
    void SetupForWalkPointExercise(bool drawBoundBox, bool userAsBoxCenter)
    {
        boxCenter = GetBoundBoxCenter(userAsBoxCenter);                             // The center position of bounding box next way point"
        lastDirection = new Vector2(bodyTrans.forward.x, bodyTrans.forward.z);      // Take user body's facing direction as "last direction"

        if (drawBoundBox)                                                           // Do we draw the bounding box or not
            DrawBoundBox();

        RandomlyPlaceWayPointsForWalk();                                            // Randomly place the way points for walking exercise
    }


    /// <summary>
    /// Function returns the center of bounding box
    ///
    /// [Detail]
    /// If "userAsBoxCenter = false", we always want to place the bounding box in front of user body,
    /// because in real game scenario, way points are always placed in front of the user
    ///
    /// [Parameter]
    /// userAsBoxCenter ===> If we want to treat user as the center of bounding box or not 
    /// </summary>
    Vector3 GetBoundBoxCenter(bool userAsBoxCenter = false)
    {
        /* If we treat the user's position as the center of bounding box */
        if (userAsBoxCenter)
        {
            return bodyTrans.position;
        }

        /* The forward direction of user with unit length = 1 */
        Vector3 bodyForward = bodyTrans.forward;

        /* Get the center of bounding box
            * User is at the edge of the bounding box */
        Vector3 boxCenterPos = new Vector3(bodyForward.x, bodyTrans.position.y, bodyForward.z + boundBoxWidth / 2);
        return boxCenterPos;
    }


    /// <summary>
    /// Function returns a random position (Vector3) in the range of a circle where the
    /// center and radius are provided as paramters.
    ///
    /// [Note]
    /// 1. Function generates random X & Z value, but it needs user to provide y-value "height".
    /// 2. wayPoint Transform is passed in only for printing purpose.
    /// </summary>
    Vector3 GetRandomPlaceAroundCenter(Vector3 center, float radius, float height, Transform wayPoint)
    {
        /* Initialize a fake absolute angle */
        float absAngle = 888;

        /* Initialize a randDirection variable */
        Vector2 randomDirection = Vector2.zero;

        /* Initialize a result position variable */
        Vector3 resultPos = new Vector3(boxCenter.x + 888, boxCenter.y, boxCenter.z + 888);

        /* Variables related to dynamically change way point placing angle threshold */
        int tried = 0;
        bool enlarged = false;
        float currAngleThreshold = angleThreshold;

        /* The angle between "direction for placing the next way point"
         * and "direction for placing this way point" must be <= 90 degree */
        while (absAngle > currAngleThreshold || ExceedBoundBox(resultPos))
        {
            /* A random x & z value of a point on a circle with "radius" provided and (0,0) as center */
            randomDirection = Random.insideUnitCircle.normalized;

            /* Get the absolute angle difference between new direction and the last direction */
            absAngle = Mathf.Abs(Vector2.Angle(randomDirection, lastDirection));

            /* Get the generated random position (Vector3).
             * The y-axis of position is from the "height" parameter which provided by developer.
             * 
             * The x-axis & z-axis are randomly generated value:
             * We firstly get a 2D point which is on the circle where (0,0) is the center and radius 1.
             * Then we multiplied the x and "z" value from 2D vector by a radius provided.
             * Lastly, we added the x and "z" value to the center provided. 
             * 
             * So we get the real Vector3 position we want to apply to the new way point */
            float xPosition = center.x + randomDirection.x * radius;
            float zPosition = center.z + randomDirection.y * radius;
            resultPos = new Vector3(xPosition, height, zPosition);

            /* If number of times tried to place way point exceed maximum times we allowed for trying
             * to place a way point. It's very likely that angleThreshold is too small, we increase it */
            if (tried++ >= maxTry && !enlarged)
            {
                currAngleThreshold = largeAngleThreshold;
                enlarged = true;
                Debug.Log(wayPoint.name + " ===> Used larger angle when placing");
            }
        }

        /* Update the "lastDirection" */
        lastDirection = randomDirection;

        return resultPos;
    }


    /// <summary>
    /// Function checks if provided a position exceeds the bounding box
    /// </summary>
    bool ExceedBoundBox(Vector3 position)
    {
        /* Get the range of x & z value for comparing with position */
        float x = boxCenter.x + (boundBoxWidth / 2);
        float negX = boxCenter.x - (boundBoxWidth / 2);
        float z = boxCenter.z + (boundBoxWidth / 2);
        float negZ = boxCenter.z - (boundBoxWidth / 2);

        /* Check if the position is out of bounding box */
        if (position.x >= negX && position.x <= x && position.z >= negZ && position.z <= z)
            return false;
        else
            return true;
    }


    /// <summary>
    /// A Tool function for visualizing the bouding box
    /// </summary>
    void DrawBoundBox()
    {
        /* A list for storing vertices and center of the bounding box */
        List<GameObject> boundVertices = new();

        /* A list for storing the name of vertices */
        List<string> vertexNames = new List<string>()
        { "Center", "NorthWest", "NorthEast", "SouthWest", "SouthEast"};

        /* Height of the cube objects */
        float height = 0;

        /* The position for placing the vertices */
        List<Vector3> vertexPos = new()
        {
            new Vector3(boxCenter.x, height, boxCenter.z),
            new Vector3(boxCenter.x - boundBoxWidth / 2, height, boxCenter.z + boundBoxWidth / 2),
            new Vector3(boxCenter.x + boundBoxWidth / 2, height, boxCenter.z + boundBoxWidth / 2),
            new Vector3(boxCenter.x - boundBoxWidth / 2, height, boxCenter.z - boundBoxWidth / 2),
            new Vector3(boxCenter.x + boundBoxWidth / 2, height, boxCenter.z - boundBoxWidth / 2)
        };

        /* Instantiate center cube objects and add it to the list */
        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Cube);
        center.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        boundVertices.Add(center);

        /* Creating another 4 vertex objects */
        for (int i = 0; i < 4; ++i)
        {
            GameObject temp = Instantiate(center);
            boundVertices.Add(temp);
        }

        /* Do stuff for all 5 vertices/points of bounding box */
        for (int i = 0; i < boundVertices.Count; ++i)
        {
            /* Change name for vertices */
            boundVertices[i].name = vertexNames[i];

            /* Add to cane not detect lise */
            if (!caneContact.objDoNotDetect.Contains(boundVertices[i].name))
                caneContact.objDoNotDetect.Add(boundVertices[i].name);

            /* Move vertices to their correct position 
             * This will draw the layout of bounding box */
            boundVertices[i].transform.position = vertexPos[i];

            /* Change vertices color */
            var renderer = boundVertices[i].GetComponent<Renderer>();
            renderer.material.SetColor("_Color", Color.black);
        }
    }


    /// <summary>
    /// Function randomly places the way points for "Walk Exercise"
    /// </summary>
    void RandomlyPlaceWayPointsForWalk()
    {
        /* The height (y-axis) of the way point */
        float height = wayPointsTrans[0].position.y;

        /* Randomly place way points */
        for (int i = 0; i < wayPointsTrans.Count; ++i)
        {
            /* Transfer the way point to a randomly generated position */
            if (i == 0)
                wayPointsTrans[i].position = GetRandomPlaceAroundCenter(bodyTrans.position, pointsDist, height, wayPointsTrans[i]);
            else
                wayPointsTrans[i].position = GetRandomPlaceAroundCenter(wayPointsTrans[i - 1].position, pointsDist, height, wayPointsTrans[i]);
        }
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
    /// Function disable the normal gesture menu on User prefab.
    /// Because we don't need it in this scene. We don't want the user
    /// accidentally turn on menu when we give instruction 
    /// </summary>
    void DisableNormalGestureMenu()
    {
        gestureMenu.enabled = false;
    }

    #endregion


    #region Functions Related to Arriving Callout

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
    /// Function assign value to the variable "lastActivePoint"
    /// </summary>
    void ModifyLastActivePoint()
    {
        lastActivePoint = activePoint;
    }


    /// <summary>
    /// Function callout using UAP after user arrives at a way point 
    /// </summary>
    void CalloutWhenArrive()
    {
        if (lastActivePoint != null && activePoint != null && activePoint != lastActivePoint)
        {
            /* Index of the way point which just been deactivated */
            int index = wayPointsTrans.IndexOf(lastActivePoint);

            /* Callout and let user know that they successfully arrived at a way point */
            verbalManager_General.Speak($"Arrived at waypoint {index + 1}.");
        }
    }

    #endregion


    #region Functions Related to User Not Moving Hint

    /// <summary>
    /// Function checks if the user's body moved comparing to the value when we check it the last time.
    /// It will update the boolean value "userMoved" accordingly
    /// </summary>
    void CheckIfUserMoved()
    {
        /* Set "bodyLastPos" at first frame */
        if (!initBodyLastPos)
        {
            bodyLastPos = bodyTrans.position;
            initBodyLastPos = true;
        }

        /* Update the body check timer */
        bodyCheckTimer += 1f * Time.deltaTime;

        /* Don't even try to check if user moved if it's not time yet */
        if (bodyCheckTimer < bodyCheckTimeThreshold)
            return;

        /* Get "user body position" in this frame */
        bodyPos = bodyTrans.position;

        /* Check if the user moved */
        float bodyDist = Vector3.Distance(bodyPos, bodyLastPos);
        if (bodyDist < bodyMoveThreshold)
            userMoved = false;
        else
            userMoved = true;

        /* Update the "bodyLastPos" */
        bodyLastPos = bodyPos;

        /* After checking "if user moved", reset the bodyCheckTimer */
        bodyCheckTimer = 0f;
    }


    /// <summary>
    /// Function gives hint to user using UAP if the user is not moving.
    /// The function judges "user not moving" according to variable "userMoved"
    /// </summary>
    void GiveHintIfUserNotMove()
    {
        /* When there still are active points in the scene, run the following code if:
         * 1. The user is not moved comparing to last time we check it
         * 2. User head's is not rotated in last X seconds 
         * 
         * [Note]
         * We check "User head rotation" before giving hint ===> because we don't want to
         * give user directional hint when users are drastically rotating head. Their head rotation
         * can changed immediately after we give hint, which will make our hint inaccurate
         */
        if (activePoint != null && !userMoved && !headRotated)
        {
            /* Update the userMove & headRotated value to prevent duplicated entering this section */
            userMoved = true;
            headRotated = true;

            /* The active way point is at which direction of user head's direction (in clock format) */
            string clockDirStr = GetDirectionActivePointToHead();

            /* Get user head's distance from the active way point */
            string distanceStr = GetDistHeadToActivePoint();

            /* Give hint using UAP */
            string hint = $"The way point is at {clockDirStr} o'clock direction of your face. It's {distanceStr} away from you.";
            Debug.Log(hint);
            verbalManager_General.Speak(hint);
        }
    }


    /// <summary>
    /// Function checks if the user head rotated comparing to the value when we check it the last time.
    /// It will update the boolean value "headRotated" accordingly
    /// </summary>
    void CheckIfHeadRotated()
    {
        /* Initialize the "headLastRot" variable */
        if (!initHeadLastRot)
        {
            headLastRot = headTrans.eulerAngles;
            initHeadLastRot = true;
        }

        /* Update the head check timer */
        headCheckTimer += 1f * Time.deltaTime;

        /* Don't even try to check if user's head rotated or not if it's not time yet */
        if (headCheckTimer < headCheckTimeThreshold)
            return;

        /* Get difference of head rotation's y-value (this frame compare to previous) */
        headRot = headTrans.eulerAngles;
        float absDiffOnHeadRotY = Mathf.Abs(headRot.y - headLastRot.y);

        /* Check if user's head rotated */
        if (absDiffOnHeadRotY < headRotYThreshold)
            headRotated = false;
        else
            headRotated = true;

        /* Update the "headLastRot" */
        headLastRot = headRot;

        /* After checking "if user head rotated", reset the headCheckTimer */
        headCheckTimer = 0f;
    }


    /// <summary>
    /// Function returns a string to indicates that:
    /// The active way point is at which direction of user head's facing direction (in clock format)
    /// </summary>
    string GetDirectionActivePointToHead()
    {
        return RelativePositionHelper.GetSimpleAndPrettyClockDirection(headTrans.gameObject, activePoint.gameObject);
    }


    /// <summary>
    /// Function gets distance from user's head to the active way point.
    /// It returns a formatted string
    /// </summary>
    string GetDistHeadToActivePoint()
    {
        /* Get the distance. The measurement system is based on user's choice in SettingsMenu */
        string distanceStr = "None";
        if (SettingsMenu.measureSystem == "US")
            distanceStr = RelativePositionHelper.GetDistance(headTrans.gameObject, activePoint.gameObject);
        else if (SettingsMenu.measureSystem == "Imperial")
            distanceStr = RelativePositionHelper.GetDistance(headTrans.gameObject, activePoint.gameObject, useFeet: false);

        /* Formatting the string */
        distanceStr = PrettifyNumStrWithDot(distanceStr);

        return distanceStr;
    }


    /// <summary>
    /// Function replaces the "." in a number string with " point ".
    /// Number string is like "4.9". By doing this, UAP can speak it better
    /// </summary>
    string PrettifyNumStrWithDot(string numStr)
    {
        if (numStr.Contains("."))
            return numStr.Replace(".", " point ");
        return numStr;
    }

    #endregion


}

