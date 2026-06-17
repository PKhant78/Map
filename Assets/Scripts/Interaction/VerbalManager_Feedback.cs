using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class VerbalManager_Feedback : MonoBehaviour
{
    [SerializeField] int verbalGapTime = 3;     // The time needed (in seconds) for playing verbal hit feedback (callout the name) for a same object

    int lastCalloutObjId;                       // The unique ID of the last object which verbal system successfully called the name for it after cane hitting it. <Note That> IsPlaying() judge could prevent a verbal from been called out 
    DateTime lastCalloutTime;                   // The time of the last successful verbal name calling


    /// <summary>
    /// The function uses TextToSpeech function from UAP Accessibility Plugin to speak a text
    /// </summary>
    public void SpeakFeedbackVerbal(string text, int actualReferObjId)
    {
        // if hit any new object and the name callout of another object is
        // not finished, we manually cut that callout
        StopVerbalFeedback();

        // play the new callout
        if (!UAP_AccessibilityManager.IsSpeaking())
        {
            UAP_AccessibilityManager.Say(text, true, true, UAP_AudioQueue.EInterrupt.All);
            lastCalloutTime = DateTime.Now;
            lastCalloutObjId = actualReferObjId;
        }
    }


    /// <summary>
    /// The function will stop any verbal speech which is playing by using UAP
    /// </summary>
    public void StopVerbalFeedback()
    {
        if (UAP_AccessibilityManager.IsSpeaking())
            UAP_AccessibilityManager.StopSpeaking();
    }


    /// <summary>
    /// Getter for verbal gap time
    /// </summary>
    public int GetVerbalGapTime()
    { return verbalGapTime; }

    /// <summary>
    /// Getter for last called time
    /// </summary>
    public DateTime GetLastCalloutTime()
    { return lastCalloutTime; }


    /// <summary>
    /// Getter for the last called object ID
    /// </summary>
    public int GetLastCalloutObjId()
    { return lastCalloutObjId; }

}
























//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;


//public class VerbalManager_Feedback : MonoBehaviour
//{
//    [SerializeField] int verbalGapTime = 3;     // The time needed (in seconds) for playing verbal hit feedback (callout the name) for a same object

//    int lastCalloutObjId;                       // The unique ID of the last object which verbal system successfully called the name for it after cane hitting it. <Note That> IsPlaying() judge could prevent a verbal from been called out 
//    DateTime lastCalloutTime;                   // The time of the last successful verbal name calling


//    /// <summary>
//    /// The function uses TextToSpeech function from UAP Accessibility Plugin to speak a text
//    /// </summary>
//    public void SpeakFeedbackVerbal(string text, int actualReferObjId)
//    {
//        // if hit any new object and the name callout of another object is
//        // not finished, we manually cut that callout
//        StopVerbalFeedback();

//        // play the new callout
//        if (!UAP_AccessibilityManager.IsSpeaking())
//        { 
//            UAP_AccessibilityManager.Say(text, true, true, UAP_AudioQueue.EInterrupt.All);
//            lastCalloutTime = DateTime.Now;
//            lastCalloutObjId = actualReferObjId;
//        }
//    }


//    /// <summary>
//    /// The function will stop any verbal speech which is playing by using UAP
//    /// </summary>
//    public void StopVerbalFeedback()
//    {
//        if (UAP_AccessibilityManager.IsSpeaking())
//            UAP_AccessibilityManager.StopSpeaking();
//    }


//    /// <summary>
//    /// Getter for verbal gap time
//    /// </summary>
//    public int GetVerbalGapTime()
//    { return verbalGapTime; }

//    /// <summary>
//    /// Getter for last called time
//    /// </summary>
//    public DateTime GetLastCalloutTime()
//    { return lastCalloutTime; }


//    /// <summary>
//    /// Getter for the last called object ID
//    /// </summary>
//    public int GetLastCalloutObjId()
//    { return lastCalloutObjId; }

//}
