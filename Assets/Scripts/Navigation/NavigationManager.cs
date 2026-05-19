/******************************************************************/
/* Programmer: MRCane Development Team                            */
/* Date: June 14th, 2022                                          */
/* Class: NavigationManager                                       */
/* Purpose:                                                       */
/* The class controls the navigation system.                      */
/* The navigation system will not start automatically. Developers */
/* need to call the "RestartNavigation" to start the system.      */
/******************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class NavigationManager : MonoBehaviour
{
    bool isRunning = false;                                             // bool variable indicates whether a navigation tour from this navigation system is running or not

    public bool useDefaultMessage = false;                              // bool variable indicates whether speak the default message when starting and ending the navigation system
    public bool isRoundTour = false;                                    // bool variable indicates whether this navigation system is for providing a round tour

    List<Transform> wayPointsTrans = new List<Transform>();             // a list stores all waypoints' transforms in this scene
    int numWayPoints;                                                   // number of way points in the scene

    int idxActivePoint;                                                 // track the index of active wayPoint

    CaneContact caneContact;                                            // cane contact script on Avatar's cane. Will add objects in navigation system to its doNotDetect list
    NavSpeakerManager speakerManager;                                   // Manager takes care of NavSpeaker 

    VerbalManager_General verbalManager_General;                        // the manager who speak out loud to generally introduce the navigation system
    readonly string beginWordsRound = "Welcome to navigation session. Please follow the drum beats 3D sound to take a round tour.";
    readonly string beginWordsNotRound = "Welcome to navigation session. Please follow the drum beats 3D sound to take a tour.";
    readonly string endWordsRound = "Navigation End! You are now at your starting position. Thanks for using!";
    readonly string endWordsNotRound = "Navigation End! Thanks for using!";


    #region private functions

    /// <summary>
    /// Function runs when awake. Awake is the first function to be called in unity execution sequence
    /// </summary>
    void Awake()
    {
        /* Initial setups which must be done before doing anything in navigation system */
        InitSetup();
    }


    /// <summary>
    /// Function does multiple essential initial setups for the navigation system, before it can do anything
    /// For example: initialize several objects, get all way points information, and making objects from
    /// navigation system cannot be detected by cane
    /// </summary>
    void InitSetup()
    {
        caneContact = GameObject.Find("/User/GripPoint/Cane").GetComponent<CaneContact>();
        speakerManager = transform.Find("NavSpeaker").GetComponent<NavSpeakerManager>();        // get the child NavSpeaker
        verbalManager_General = GetComponent<VerbalManager_General>();                          // get the verbal manager script which is attached to the Navigation System

        CollectWayPointsInfo();                                         // get all info about waypoints in the scene
        AddToNotDetect();                                               // add to "objDoNotDetect" list on cane, so cane won't provide feedback for object in NavigationSystem
    }


    /// <summary>
    /// Function initialize the navigation system.
    /// It only activate wayPoint at index 0.
    /// </summary>
    void InitNavigation()
    {
        /* [Extra Note]
         * 
         * The wayPoints in the scene are set to inactive when they awake.
         * The "set to inactive" process happens in the "WayPointManager.cs" script.
         * 
         * However, it's possible that the navigation system is called to restart by developer before
         * a previous navigation system is ended ===> taking this very sepcial scenario into account, 
         * we set all wayPoints to inactive everytime when restarting the navigation system.
         */
        DeactivateAllPoints();

        /* Activate the 1st wayPoint when initialize the system */
        idxActivePoint = 0;
        ActivatePointByIndex(idxActivePoint);
    }


    /// <summary>
    /// Function finishes a navigation tour after the last
    /// wayPoint in that tour has been triggered. When call
    /// this function, the variable "idxActivePoint" must
    /// >= "numWayPoints - 1"
    /// </summary>
    void FinishTour()
    {
        if (idxActivePoint >= numWayPoints - 1)                     // only run the following when wayPoints run out, which means the navigation tour needs to finish
        {
            DeactivatePointByIndex(idxActivePoint);                 // deactivate the current wayPoint

            /* Convey the default ending message to user */
            if (useDefaultMessage)
            {
                if (isRoundTour)
                    verbalManager_General.Speak(endWordsRound);
                else
                    verbalManager_General.Speak(endWordsNotRound);
            }

            isRunning = false;                                      // set "isRunning" variable to FALSE when a tour is finished
        }
    }


    /// <summary>
    /// Function gets transforms of all waypoints in the scene
    /// </summary>
    void CollectWayPointsInfo()
    {
        // if there's no wayPoint trans in the "wayPointsTrans" list, add them
        if (wayPointsTrans.Count == 0)
        {
            numWayPoints = transform.Find("WayPoints").childCount;      // number of waypoints in the scene
            for (int i = 0; i < numWayPoints; ++i)
                wayPointsTrans.Add(transform.Find("WayPoints").GetChild(i));
        }
    }


    /// <summary>
    /// Function adds objects of "Navigation System" to not detect list on Avatar's cane
    /// So it won't trigger feedback when cane hits them
    /// </summary>
    void AddToNotDetect()
    {
        /* Add the NavSpeaker */
        if (!caneContact.objDoNotDetect.Contains("NavSpeaker"))
            caneContact.objDoNotDetect.Add("NavSpeaker");

        /* Add the wayPoints */
        foreach (Transform trans in wayPointsTrans)
        {
            string pointName = trans.name;
            if (!caneContact.objDoNotDetect.Contains(pointName))
                caneContact.objDoNotDetect.Add(pointName);
        }
    }


    /// <summary>
    /// Function deactivates one wayPoint by its index
    /// </summary>
    void DeactivatePointByIndex(int idx)
    {
        wayPointsTrans[idx].gameObject.SetActive(false);
    }


    /// <summary>
    /// Function activates one wayPoint by its index
    /// </summary>
    void ActivatePointByIndex(int idx)
    {
        wayPointsTrans[idx].gameObject.SetActive(true);
    }

    #endregion private functions


    #region public functions

    /// <summary>
    /// Function deactivates all the wayPoints
    /// </summary>
    public void DeactivateAllPoints()
    {
        foreach (Transform pointTrans in wayPointsTrans)
            pointTrans.gameObject.SetActive(false);
    }


    /// <summary>
    /// Function deactivates all the wayPoints
    /// </summary>
    public void ActivateAllPoints()
    {
        foreach (Transform pointTrans in wayPointsTrans)
            pointTrans.gameObject.SetActive(true);
    }


    /// <summary>
    /// Getter of the isRunning boolean value
    /// </summary>
    public bool IsRunning
    {
        get { return isRunning; }
    }


    /// <summary>
    /// 
    /// Function kill the current active navigation system and start a new one.
    /// If no existing navigation system is running, the function will just start a
    /// new navigation session
    /// 
    /// Usage:
    /// Can be called from other class to start a navigation session
    /// 
    /// </summary>
    public void RestartNavSystem()
    {
        isRunning = true;                                               // this navigation system is started, so change variable "isRunning" to TRUE

        InitNavigation();                                               // initialize all wayPoints => index 0 is activated, and the rest will all be deactivated

        /* Convey the default beinging message to user */
        if (useDefaultMessage)
        {
            if (isRoundTour)
                verbalManager_General.Speak(beginWordsRound);
            else
                verbalManager_General.Speak(beginWordsNotRound);
        }
    }


    /// <summary>
    /// Function completely shuts down a navigation session.
    /// Deactivating all way points of the system, and stop the audio from navigation speaker.
    /// It can be called from other functions for ending a navigation system at developer's needs.
    /// </summary>
    public void EndNavSystem()
    {
        /* Deactivate all way points */
        DeactivateAllPoints();

        /* Stop sound playing from Nav Speaker */
        speakerManager.StopNavAudio();

        /* Update isRunning variable */
        isRunning = false;
    }


    /// <summary>
    /// Function deactivates current way point and activate the next
    /// Will be called from the way point which hit the Avatar's body
    /// </summary>
    public void NextPoint()
    {
        if (idxActivePoint < numWayPoints - 1)                      // if more wayPoints are available
        {
            DeactivatePointByIndex(idxActivePoint);                 // deactivate the current wayPoint
            ActivatePointByIndex(++idxActivePoint);                 // activate the next wayPoint by its index
        }
        else                                                        // if no more wayPoints ===> deactivate the active one
            FinishTour();                                           // finishing a navigation tour
    }


    /// <summary>
    /// Function plays 3D sound from all way points one-by-one at its original or reversed order
    ///
    /// [Parameters]
    /// 1.reverseOrder = boolean variable indicates whether play sound from way points in reverse order or not
    /// 2. waitTime = the time (in seconds) of waiting for playing sound from next wait point after playing a previous one 
    ///
    /// [Note on returned value]
    /// This async function returns a boolean variable TRUE when the process is done.
    /// 
    /// Developers can create a async function in their class.
    /// In that function, developer can wait for this function to be done before doing any other things
    /// For example ===> "await x = PlayAllWayPoints();"
    /// 
    /// If developers can also call this function in an synchronized function in their class. 
    /// For example ===> "_ = PlayAllWayPoints();"
    /// </summary>

    public async Task<bool> PlayAllWayPoints(bool reverseOrder = false, float waitTime = 1f)
    {
        /* User input of "waitTime" is in second, we need to cast it
         * into milliseconds before feeding to Task.Delay() function */
        int newWaitTime = (int)(waitTime * 1000);

        /* A list of way points to play (reverse it if needed) */
        List<Transform> pointsToPlayTrans = UsefulTools.ListDeepCopy(wayPointsTrans);

        if (reverseOrder)
            pointsToPlayTrans.Reverse();

        /* Play the way points in the list "pointsToPlayTrans" */
        foreach (Transform pointTrans in pointsToPlayTrans)
        {
            /* Activate way point and wait for a while only when application is active 
             * Once a way point is active, the 3D sound will play from it */
            if (Application.isPlaying)
            {
                pointTrans.gameObject.SetActive(true);
                await Task.Delay(newWaitTime);
            }

            /* Only deactivate an activated way point if the application is running. 
             * If the application is stopped, and an way point has been activated and 
             * the waitTime is undergoing ===> we don't deactivate this way point after
             * the waiting time is completed */
            if (Application.isPlaying)
                pointTrans.gameObject.SetActive(false);
        }

        /* Stop playing audio from Nav Speaker if speakerManager object is not NULL */
        if (speakerManager)
            speakerManager.StopNavAudio();

        /* Return true when this way points playing process is done*/
        return true;
    }


    /// <summary>
    /// 
    /// Set the continuity of the navigation audio.
    /// "Continuity" means whether "restart a navigation audio" or "resume an paused navigation audio"
    /// when calling "PlayNavAudio()" from NavSpeakerManager class.
    ///
    /// [Example]
    /// The function can be called before calling PlayAllWayPoints() function. Then, the user will hear
    /// continuous navigation audio when jumping from point to point. If the function SetNavAudioContinuity()
    /// is not called ===> each time jumping to a new way point will restart the navigation audio again.
    /// 
    /// </summary>
    public void SetNavAudioContinuity(bool isContinuous = false)
    {
        if (speakerManager)
            speakerManager.IsContinuousNavAudio = isContinuous;
    }


    /// <summary>
    /// Function revert audio continuity of navSpeaker to its default value 
    /// </summary>
    public void RevertAudioContinuity()
    {
        if (speakerManager)
            speakerManager.RevertAudioContinuity();
    }

    #endregion public functions

}

