using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NavSpeakerManager : MonoBehaviour
{
    AudioSource audioSource;                            // The audioSource which carried by the NavSpeaker

    readonly static bool d_AudioContinuity = false;     // Default value of navSpeaker's audio continuity
    bool isContinuousAudio = d_AudioContinuity;

#region Pitch Change Related Variables

    bool wantPitchChangeOnFocus = true;                 // bool variable indicates if developer wants the audioSource's pitch to change

    float originPitch;                                  // The original value of pitch on NavSpeaker
    readonly float pitchMulti = 1.5f;                   // [Default = 1.5f] Multiplier to apply to pitch when it's focused by user head

    HeadCaster headCaster;                              // A class on user's head which is used for casting ray/capsule/etc...

    bool focusStatus = false;                           // The focus status (TRUE/ FALSE) of the current frame
    bool lastFocusStatus = false;                       // The focus status (TRUE/ FALSE) in the last frame

#endregion


    // Awake is called before all any other functions in Unity execution sequence
    void Awake()
    {
        /* Initialize the audioSource on NavSpeaker */
        audioSource = GetComponent<AudioSource>();

        /* Making the NavSpeaker completely invisible when the game start */
        GetComponent<MeshRenderer>().enabled = false;

        /* Instantiate a reference to the caster class on User's head */
        headCaster = GameObject.Find("User/Head").GetComponent<HeadCaster>();

        /* The original pitch of audioSource on NavSpeaker */
        originPitch = audioSource.pitch;
    }


    /// <summary>
    /// Function is called on every frame
    /// </summary>
    void Update()
    {
        /* If user want the pitch to change, we dynamically
         * change pitch based on "if user head focus on
         * NavSpeaker's direction or not" */
        if (wantPitchChangeOnFocus)
            OnFocusChange();
    }


    /// <summary>
    /// Function do something when the NavSpeaker is "focused/unfocused" by user's head
    /// "Focus" means the "user head is facing the direction of navSpeaker".
    /// When focus status is changed, we change the pitch of audioSource of navSpeaker.
    /// </summary>
    void OnFocusChange()
    {
        /* Get all hits information from head's absolute front capsule cast */
        RaycastHit[] hits = headCaster.AbsFrontCapsuleHits;

        /* If this specific NavSpeaker is hitted by capsule cast.
         * It means the user head is "facing" the direction of NavSpeaker  
         * Thus, the focus status is TRUE. Otherwise, FALSE */
        if (hits != null && Array.Exists(hits, hit => hit.transform == transform))
            focusStatus = true;
        else
            focusStatus = false;

        /* When the focus status on this frame is changed comparing to that of last frame 
         * We will try to change pitch of NavSpeaker audioSource if the focus status is changed */
        if (focusStatus != lastFocusStatus)
            ChangePitchBasedOnFocus();

        /* Update "lastFocusStatus" boolean variable */
        lastFocusStatus = focusStatus;
    }


    /// <summary>
    /// Change the pitch of NavSpeaker
    /// </summary>
    void ChangePitchBasedOnFocus()
    {
        if (focusStatus)
            audioSource.pitch = originPitch * pitchMulti;       // If focusStatus is TRUE, raise the pitch
        else
            audioSource.pitch = originPitch;                    // If focusStatus is FALSE, revert pitch back to original
    }


    /// <summary>
    /// Getter and setter of "wantPitchChangeOnFocus" boolean variable
    /// </summary>
    public bool WantPitchChangeOnFocus
    {
        get { return wantPitchChangeOnFocus; }
        set { wantPitchChangeOnFocus = value; }
    }


    /// <summary>
    /// Getter and Setter function of "isContinuousAudio" boolean variable
    /// </summary>
    public bool IsContinuousNavAudio
    {
        get { return isContinuousAudio; }
        set { isContinuousAudio = value; }
    }


    /// <summary>
    /// Function for transport the NavSpeaker to a specific position indicated
    /// </summary>
    public void TransportNavSpeaker(Vector3 destinationPoint)
    {
        /* The y-axis (height) of NavSpeaker will stay at 2 */
        Vector3 adjustedPoint = new Vector3(destinationPoint.x, 2, destinationPoint.z);

        /* Transport the NavSpeaker */
        transform.position = adjustedPoint;
    }


    /// <summary>
    /// Function to play the navigational audio
    /// </summary>
    public void PlayNavAudio()
    {
        if (isContinuousAudio)
        {
            /* Try to unpause first if no audio is playing */
            if (!IsPlaying())
                audioSource.UnPause();

            /* If after the unpause, the audio is not playing 
             * ===> it means the audio is never played 
             * ===> so call Play() function to play it */
            if (!IsPlaying())
                audioSource.Play();
        }
        else
            audioSource.Play();
    }


    /// <summary>
    /// Function to stop the navigational audio
    /// </summary>
    public void StopNavAudio()
    {
        /* If "audio need to be continuous" ===> pause the audio
         * Otherwise ===> stop the audio */
        if (isContinuousAudio)
            audioSource.Pause();
        else
            audioSource.Stop();
    }


    /// <summary>
    /// Function returns whether the navigation audio is playing
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }


    /// <summary>
    /// Function reverts the "audio continuity" of navSpeaker to its default value
    /// </summary>
    public void RevertAudioContinuity()
    {
        isContinuousAudio = d_AudioContinuity;
    }

}

