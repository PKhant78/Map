/*****************************************************************/
/* Programmer: MRCane Development Team                           */
/* Date: July 26th, 2022                                         */
/* Class: HeadNodDetector                                        */
/* Purpose:                                                      */
/* A class designed to detect user's head noding motion          */
/*****************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HeadNodDetector : MonoBehaviour
{
    #region General Variables

    bool headNodded = false;                                // Variable indicates if the head nodded



    // ------------ Testing Below ------------ //

    bool reverseHeadNodded = false;                         // Variable indicates if done a reverse head nod or not


    // ------------ Testing Above ------------ //




    public static string nodDetectMethod = "Speed"; // [Available values: Speed, Threshold] This variable indicates we will use which way to detect user's head nod motion       

    #endregion


    #region Variables for Threshold Based "Head Nod" Detection

    float riseHeadTimer = 0f;                       // The timer for helping with determining head rising
    float riseHeadTimeThreshold = 2f;               // [Default = 2] The time threshold for judging user's head rise up         
    float headHorRotX = 0f;                         // The "horizontal degree" of head rotation on x-axis
    float headNodRotX = -10f;                       // [Default = -10] A rotation degree of head on x-axis, below which indicates the start of head nod tracking
    bool trackNoding = false;                       // Boolean variable indicates if we want to track the head noding

    #endregion


    #region Shared Variables for Rotation Speed Based Detection

    float lastNormHeadRotX;                         // The normalized head rotation on x-axis in the last frame
    float normHeadRotX;                             // The normalized head rotation on x-axis in this frame
    float normHeadRotSpeedX;                        // The speed of head rotation on x-axis (from last frame to this frame)

    static public int minFrameReq = 2;              // [Default = 2] The minimum number of frames needed to judge if the user is doing head-noding
    static public float minRotXSpeed = 80f;         // [Default = 80f] The minimum speed for judging if the user is doing head-noding
    static public float waitWindowTime = 0.5f;      // [Default = 1f] The time in seconds we will wait for the user to raise their head

    #endregion


    #region Variables for Rotation Speed Based "Head Nod" Detection

    int negFrameTracker = 0;                        // Tracking number frame of negative speed ===> it means user is lowering their heads
    int posFrameTracker = 0;                        // Tracking number frame of positive speed ===> it means user is raising their heads
    float windowTimeTracker = 0f;                   // Tracking the time in seconds elapsed in "waiting window"

    bool windowOpen = false;                        // Open the window for waiting user to perform raise head motion

    #endregion



    // ------------ Testing Below ------------ //

    #region Variables for Rotation Speed Based "Reverse Head Nod" Detection

    int negFrameTracker_Reverse = 0;                // Tracking number frame of negative speed ===> it means user is lowering their heads
    int posFrameTracker_Reverse = 0;                // Tracking number frame of positive speed ===> it means user is raising their heads
    float windowTimeTracker_Reverse = 0f;           // Tracking the time in seconds elapsed in "waiting window"

    bool windowOpen_Reverse = false;                // Open the window for waiting user to perform raise head motion

    #endregion

    // ------------ Testing Above ------------ //




    


    #region Public Functions

    /// <summary>
    /// Getter of "headNodded" variable
    /// </summary>
    public bool HeadNodded
    {
        get { return headNodded; }
    }

    

    // ------------ Testing Below ------------ //

    /// <summary>
    /// Getter of "reverseHeadNodded" variable
    /// </summary>
    public bool ReverseHeadNodded
    {
        get { return reverseHeadNodded; }
    }

    // ------------ Testing Above ------------ //

    #endregion


    #region General Functions

    /// <summary>
    /// Awake function is the first function called in an Unity execution loop
    /// </summary>
    private void Awake()
    {
        InitLastHeadRotX();
    }






    ///// <summary>
    ///// Function is called on every frame
    ///// </summary>
    //private void Update()
    //{
    //    UpdateThisHeadRotX();                           // In the beginning of each frame, update "normHeadRotX"

    //    if (nodDetectMethod == "Speed")
    //        headNodded = DetectHeadNod_Speed();
    //    else if (nodDetectMethod == "Threshold")
    //        headNodded = DetectHeadNod_Threshold();

    //    UpdateLastHeadRotX();                           // In the end of each frame, update "lastNormHeadRotX"
    //}






    // ------------ Testing Below ------------ //

    /// <summary>
    /// Function is called on every frame
    /// </summary>
    private void Update()
    {
        UpdateThisHeadRotX();                           // In the beginning of each frame, update "normHeadRotX"
        CalcNormHeadRotSpeedX();                        // Calculate the speed of head rotation on x-axis (vertical movement of head)

        if (nodDetectMethod == "Speed")
        {
            headNodded = DetectHeadNod_Speed();
            reverseHeadNodded = DetectReverseHeadNod_Speed();
        }
        else if (nodDetectMethod == "Threshold")
            headNodded = DetectHeadNod_Threshold();

        UpdateLastHeadRotX();                           // In the end of each frame, update "lastNormHeadRotX"
    }

    // ------------ Testing Above ------------ //






    #endregion


    #region HeadNodDetect 1: Threshold Based

    /// <summary>
    /// Function detects 1 head noding movement/gesture
    /// </summary>
    bool DetectHeadNod_Threshold()
    {
        /* If user's head rotation is below the threshold, we initialize head nod tracking */
        if (normHeadRotX < headNodRotX)
            trackNoding = true;

        /* If trackNoding flag is TRUE, we keep updating the timer */
        if (trackNoding)
            riseHeadTimer += 1f * Time.deltaTime;

        /* If user's head rotation is above the horizontal threshold */
        if (normHeadRotX > headHorRotX)
        {
            /* Prevent accessing code below if riseHeadTimer is at its initial value 0, 
             * it means the head never go below the head-noding line yet */
            if (riseHeadTimer == 0f)
                return false;

            /* Stop head-nod tracking */
            trackNoding = false;

            /* If we are within the "nod-checking" time range */
            if (riseHeadTimer <= riseHeadTimeThreshold)
            {
                riseHeadTimer = 0f;
                return true;
            }

            /* If the timer is out of threshold-time, we reset timer only */
            riseHeadTimer = 0f;
        }

        return false;
    }

    #endregion


    #region HeadNodDetect 2: Rotation Speed Based

    /// <summary>
    /// Function detects 1 head noding movement/gesture
    /// </summary>
    bool DetectHeadNod_Speed()
    {
        /* If the window for "waiting head raise motion" is not yet opened */
        if (!windowOpen)
        {
            /* Update the "negFrameTracker" variable */
            TrackNegativeFrames();

            /* Open the window for "waiting head raise motion" if system detects N consecutive negative speed */
            if (negFrameTracker > minFrameReq)
                HandleWindowOpen();
        }
        else
        {
            /* Track the time elapsed when the window is open */
            windowTimeTracker += 1f * Time.deltaTime;

            /* If the window tracker hasn't passed the threshold */
            if (windowTimeTracker < waitWindowTime)
                TrackPositiveFrames();                                // Update the "posFrameTracker" variable
            else
                HandleWindowClose();

            /* If system detects N consecutive frames which user is raising their head */
            if (posFrameTracker > minFrameReq)
            {
                HandleWindowClose();
                return true;
            }
        }

        //Debug.Log($"Speed: {normHeadRotSpeedX} || Speed Threshold: {minRotXSpeed} || Window Open: {windowOpen} || negFrameTracker: {negFrameTracker} || posFrameTracker: {posFrameTracker} || windowFrameTracker: {windowTimeTracker}");

        return false;
    }


    /// <summary>
    /// Function takes care of the update of "negFrameTracker" variable
    /// </summary>
    void TrackNegativeFrames()
    {
        /* [Important Note] 
         * "Headphone Motion API" gets head rotation every 2 frames, while ARFace does it every 1 frame.
         * Because of this laggy updat frequency, it will make the speed of 1 frame to be 0.
         * Eventually, it will cause malfunction in the code below ===> so we added this control gate. */
        if (normHeadRotSpeedX == 0f)
            return;

        /* Track the number of consecutive frames which user is doing lowering head motion.
         * System only counts the speed which passes the threshold speed */
        if (normHeadRotSpeedX < -minRotXSpeed)
            negFrameTracker++;
        else
            negFrameTracker = 0;
    }


    /// <summary>
    /// Function takes care of the update of "posFrameTracker" variable
    /// </summary>
    void TrackPositiveFrames()
    {
        /* Added this control gate for the same reason as stated in the "TrackNegativeFrames()" function */
        if (normHeadRotSpeedX == 0f)
            return;

        /* Track the number of consecutive frames which user is doing raising head motion.
         * System only counts the speed which passes the threshold speed */
        if (normHeadRotSpeedX > minRotXSpeed)
            posFrameTracker++;
        else
            posFrameTracker = 0;
    }


    /// <summary>
    /// Function handles the things to do right before opening the window,
    /// in the process of checking "head nod".
    /// </summary>
    void HandleWindowOpen()
    {
        /* Reset the "negFrameTracker" back to 0, when window is opening */
        negFrameTracker = 0;

        /* Open the window */
        windowOpen = true;
    }


    /// <summary>
    /// Function handles the things to do right before closing the window,
    /// in the process of checking "head nod".
    /// </summary>
    void HandleWindowClose()
    {
        /* Reset the "posFrameTracker" and "windowTimeTracker" back to 0, when window is closing */
        posFrameTracker = 0;
        windowTimeTracker = 0f;

        /* Close the window */
        windowOpen = false;
    }

    #endregion













    // ------------ Testing Below ------------ //



    /// <summary>
    /// Function calculates the speed of head rotation on x-axis from last frame to this
    /// </summary>
    void CalcNormHeadRotSpeedX()
    {
        normHeadRotSpeedX = (normHeadRotX - lastNormHeadRotX) / Time.deltaTime;
    }









    #region  ReverseHeadNodDetect 2: Rotation Speed Based

    /// <summary>
    /// Function detects 1 reverse head noding movement/gesture
    /// </summary>
    bool DetectReverseHeadNod_Speed()
    {
        /* If the window for "waiting head lowering motion" is not yet opened */
        if (!windowOpen_Reverse)
        {
            /* Update the "posFrameTracker_Reverse" variable */
            TrackPositiveFrames_Reverse();

            /* Open the window for "waiting head lowering motion" if system detects N consecutive positive speed */
            if (posFrameTracker_Reverse > minFrameReq)
                HandleWindowOpen_Reverse();
        }
        else
        {
            /* Track the time elapsed when the window is open */
            windowTimeTracker_Reverse += 1f * Time.deltaTime;

            /* If the window tracker hasn't passed the threshold */
            if (windowTimeTracker_Reverse < waitWindowTime)
                TrackNegativeFrames_Reverse();                        // Update the "negFrameTracker_Reverse" variable
            else
                HandleWindowClose_Reverse();

            /* If system detects N consecutive frames which user is lower their head */
            if (negFrameTracker_Reverse > minFrameReq)
            {
                HandleWindowClose();
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Function takes care of the update of "negFrameTracker_Reverse" variable
    /// </summary>
    void TrackNegativeFrames_Reverse()
    {
        /* Added this control gate for the same reason as stated in the "TrackNegativeFrames()" function */
        if (normHeadRotSpeedX == 0f)
            return;

        /* Track the number of consecutive frames which user is doing lowering head motion.
         * System only counts the speed which passes the threshold speed */
        if (normHeadRotSpeedX < -minRotXSpeed)
            negFrameTracker_Reverse++;
        else
            negFrameTracker_Reverse = 0;
    }


    /// <summary>
    /// Function takes care of the update of "posFrameTracker_Reverse" variable
    /// </summary>
    void TrackPositiveFrames_Reverse()
    {
        /* Added this control gate for the same reason as stated in the "TrackNegativeFrames()" function */
        if (normHeadRotSpeedX == 0f)
            return;

        /* Track the number of consecutive frames which user is doing raising head motion.
         * System only counts the speed which passes the threshold speed */
        if (normHeadRotSpeedX > minRotXSpeed)
            posFrameTracker_Reverse++;
        else
            posFrameTracker_Reverse = 0;
    }


    /// <summary>
    /// Function handles the things to do right before opening the window,
    /// in the process of checking "reverse head nod".
    /// </summary>
    void HandleWindowOpen_Reverse()
    {
        /* Reset the "posFrameTracker_Reverse" back to 0, when window is opening */
        posFrameTracker_Reverse = 0;

        /* Open the window */
        windowOpen_Reverse = true;
    }


    /// <summary>
    /// Function handles the things to do right before closing the window,
    /// in the process of checking "reverse head nod".
    /// </summary>
    void HandleWindowClose_Reverse()
    {
        /* Reset the "negFrameTracker_Reverse" and "windowTimeTracker_Reverse" back to 0, when window is closing */
        negFrameTracker_Reverse = 0;
        windowTimeTracker_Reverse = 0f;

        /* Close the window */
        windowOpen_Reverse = false;
    }

    #endregion


    // ------------ Testing Above ------------ //














    #region Shared Functions

    /// <summary>
    /// Function assigns value to "lastNormHeadRotX" when game begins
    /// </summary>
    void InitLastHeadRotX()
    {
        lastNormHeadRotX = NormalizeHeadRotationX(transform.eulerAngles.x);
    }


    /// <summary>
    /// Function assigns value of "normHeadRotX" to "lastNormHeadRotX" in the end of each frame
    /// </summary>
    void UpdateLastHeadRotX()
    {
        lastNormHeadRotX = normHeadRotX;
    }


    /// <summary>
    /// Function updates "normHeadRotX" in the beginning of each frame
    /// </summary>
    void UpdateThisHeadRotX()
    {
        normHeadRotX = NormalizeHeadRotationX(transform.eulerAngles.x);
    }


    /// <summary>
    /// Function normalize the head rotation from (0,360) range
    /// to two ranges, which are (0,180) and (0,-180)
    /// </summary>
    float NormalizeHeadRotationX(float rotVal)
    {
        if (rotVal > 180)
            return -(rotVal - 360);
        else
            return -rotVal;
    }

    #endregion Shared Functions

}






























///*****************************************************************/
///* Programmer: MRCane Development Team                           */
///* Date: July 26th, 2022                                         */
///* Class: HeadNodDetector                                        */
///* Purpose:                                                      */
///* A class designed to detect user's head noding motion          */
///*****************************************************************/

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//public class HeadNodDetector : MonoBehaviour
//{
//    #region General Variables

//    bool headNodded;                                // Variable indicates if the head nodded
//    public static string nodDetectMethod = "Speed"; // [Available values: Speed, Threshold] This variable indicates we will use which way to detect user's head nod motion       

//    #endregion


//    #region Variables for Hard-coded Threshold Based Detection

//    float riseHeadTimer = 0f;                       // The timer for helping with determining head rising
//    float riseHeadTimeThreshold = 2f;               // [Default = 2] The time threshold for judging user's head rise up         
//    float headHorRotX = 0f;                         // The "horizontal degree" of head rotation on x-axis
//    float headNodRotX = -10f;                       // [Default = -10] A rotation degree of head on x-axis, below which indicates the start of head nod tracking
//    bool trackNoding = false;                       // Boolean variable indicates if we want to track the head noding

//    #endregion


//    #region Variables for Head Rotation Speed Based Detection

//    float lastNormHeadRotX;                         // The normalized head rotation on x-axis in the last frame
//    float normHeadRotX;                             // The normalized head rotation on x-axis in this frame
//    float normHeadRotSpeedX;                        // The speed of head rotation on x-axis (from last frame to this frame)

//    static public int minFrameReq = 2;              // [Default = 2] The minimum number of frames needed to judge if the user is doing head-noding
//    static public float minRotXSpeed = 80f;         // [Default = 80f] The minimum speed for judging if the user is doing head-noding
//    static public float waitWindowTime = 0.5f;      // [Default = 1f] The time in seconds we will wait for the user to raise their head

//    int negFrameTracker = 0;                        // Tracking number frame of negative speed ===> it means user is lowering their heads
//    int posFrameTracker = 0;                        // Tracking number frame of positive speed ===> it means user is raising their heads
//    float windowTimeTracker = 0f;                   // Tracking the time in seconds elapsed in "waiting window"

//    bool windowOpen = false;                        // Open the window for waiting user to perform raise head motion

//    #endregion


//    #region Public Functions

//    /// <summary>
//    /// Getter of "headNodded" variable
//    /// </summary>
//    public bool HeadNodded
//    {
//        get { return headNodded; }
//    }

//    #endregion


//    #region General Functions

//    /// <summary>
//    /// Awake function is the first function called in an Unity execution loop
//    /// </summary>
//    private void Awake()
//    {
//        InitLastHeadRotX();
//    }


//    /// <summary>
//    /// Function is called on every frame
//    /// </summary>
//    private void Update()
//    {
//        UpdateThisHeadRotX();                           // In the beginning of each frame, update "normHeadRotX"

//        if (nodDetectMethod == "Speed")
//            headNodded = DetectHeadNod_Speed();
//        else if (nodDetectMethod == "Threshold")
//            headNodded = DetectHeadNod_Threshold();

//        UpdateLastHeadRotX();                           // In the end of each frame, update "lastNormHeadRotX"
//    }

//    #endregion


//    #region 1: Hard-coded Threshold Based Detection

//    /// <summary>
//    /// Function detects 1 head noding movement/gesture
//    /// </summary>
//    bool DetectHeadNod_Threshold()
//    {
//        /* If user's head rotation is below the threshold, we initialize head nod tracking */
//        if (normHeadRotX < headNodRotX)
//            trackNoding = true;

//        /* If trackNoding flag is TRUE, we keep updating the timer */
//        if (trackNoding)
//            riseHeadTimer += 1f * Time.deltaTime;

//        /* If user's head rotation is above the horizontal threshold */
//        if (normHeadRotX > headHorRotX)
//        {
//            /* Prevent accessing code below if riseHeadTimer is at its initial value 0, 
//             * it means the head never go below the head-noding line yet */
//            if (riseHeadTimer == 0f)
//                return false;

//            /* Stop head-nod tracking */
//            trackNoding = false;

//            /* If we are within the "nod-checking" time range */
//            if (riseHeadTimer <= riseHeadTimeThreshold)
//            {
//                riseHeadTimer = 0f;
//                return true;
//            }

//            /* If the timer is out of threshold-time, we reset timer only */
//            riseHeadTimer = 0f;
//        }

//        return false;
//    }

//    #endregion


//    #region 2: Head Rotation Speed Based Detection

//    /// <summary>
//    /// Function detects 1 head noding movement/gesture
//    /// </summary>
//    bool DetectHeadNod_Speed()
//    {
//        /* Calculate the speed of head rotation on x-axis from last frame to this */
//        normHeadRotSpeedX = (normHeadRotX - lastNormHeadRotX) / Time.deltaTime;

//        /* If the window for "waiting head raise motion" is not yet opened */
//        if (!windowOpen)
//        {
//            /* Update the "negFrameTracker" variable */
//            TrackNegativeFrames();

//            /* Open the window for "waiting head raise motion" if system detects N consecutive negative speed */
//            if (negFrameTracker > minFrameReq)
//                HandleWindowOpen();
//        }
//        else
//        {
//            /* Track the time elapsed when the window is open */
//            windowTimeTracker += 1f * Time.deltaTime;

//            /* If the window tracker hasn't passed the threshold */
//            if (windowTimeTracker < waitWindowTime)
//                TrackPositiveFrames();                                // Update the "posFrameTracker" variable
//            else
//                HandleWindowClose();

//            /* If system detects N consecutive frames which user is raising their head */
//            if (posFrameTracker > minFrameReq)
//            {
//                HandleWindowClose();
//                return true;
//            }
//        }

//        //Debug.Log($"Speed: {normHeadRotSpeedX} || Speed Threshold: {minRotXSpeed} || Window Open: {windowOpen} || negFrameTracker: {negFrameTracker} || posFrameTracker: {posFrameTracker} || windowFrameTracker: {windowTimeTracker}");

//        return false;
//    }


//    /// <summary>
//    /// Function takes care of the update of "negFrameTracker" variable
//    /// </summary>
//    void TrackNegativeFrames()
//    {
//        /* [Important Note] 
//         * "Headphone Motion API" gets head rotation every 2 frames, while ARFace does it every 1 frame.
//         * Because of this laggy updat frequency, it will make the speed of 1 frame to be 0.
//         * Eventually, it will cause malfunction in the code below ===> so we added this control gate. */
//        if (normHeadRotSpeedX == 0f)
//            return;

//        /* Track the number of consecutive frames which user is doing lowering head motion.
//         * System only counts the speed which passes the threshold speed */
//        if (normHeadRotSpeedX < -minRotXSpeed)
//            negFrameTracker++;
//        else
//            negFrameTracker = 0;
//    }


//    /// <summary>
//    /// Function takes care of the update of "posFrameTracker" variable
//    /// </summary>
//    void TrackPositiveFrames()
//    {
//        /* Added this control gate for the same reason as stated in the "TrackNegativeFrames()" function */
//        if (normHeadRotSpeedX == 0f)
//            return;

//        /* Track the number of consecutive frames which user is doing raising head motion.
//         * System only counts the speed which passes the threshold speed */
//        if (normHeadRotSpeedX > minRotXSpeed)
//            posFrameTracker++;
//        else
//            posFrameTracker = 0;
//    }


//    /// <summary>
//    /// Function handles the things to do right before opening the window
//    /// </summary>
//    void HandleWindowOpen()
//    {
//        /* Reset the "negFrameTracker" back to 0, when window is opening */
//        negFrameTracker = 0;

//        /* Open the window */
//        windowOpen = true;
//    }


//    /// <summary>
//    /// Function handles the things to do right before closing the window
//    /// </summary>
//    void HandleWindowClose()
//    {
//        /* Reset the "posFrameTracker" and "windowTimeTracker" back to 0, when window is closing */
//        posFrameTracker = 0;
//        windowTimeTracker = 0f;

//        /* Close the window */
//        windowOpen = false;
//    }

//    #endregion


//    #region Shared Functions

//    /// <summary>
//    /// Function assigns value to "lastNormHeadRotX" when game begins
//    /// </summary>
//    void InitLastHeadRotX()
//    {
//        lastNormHeadRotX = NormalizeHeadRotationX(transform.eulerAngles.x);
//    }


//    /// <summary>
//    /// Function assigns value of "normHeadRotX" to "lastNormHeadRotX" in the end of each frame
//    /// </summary>
//    void UpdateLastHeadRotX()
//    {
//        lastNormHeadRotX = normHeadRotX;
//    }


//    /// <summary>
//    /// Function updates "normHeadRotX" in the beginning of each frame
//    /// </summary>
//    void UpdateThisHeadRotX()
//    {
//        normHeadRotX = NormalizeHeadRotationX(transform.eulerAngles.x);
//    }


//    /// <summary>
//    /// Function normalize the head rotation from (0,360) range
//    /// to two ranges, which are (0,180) and (0,-180)
//    /// </summary>
//    float NormalizeHeadRotationX(float rotVal)
//    {
//        if (rotVal > 180)
//            return -(rotVal - 360);
//        else
//            return -rotVal;
//    }

//    #endregion

//}

