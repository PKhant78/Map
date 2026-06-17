using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MusicPlayerManager : MonoBehaviour
{
    #region Normal Variables

    public List<AudioClip> audioList = new List<AudioClip>();   // A list of music that can be played
    AudioSource spaAudioSource;                                 // The audioSource play spatial audio

    #endregion


    #region SpaAudioSource Pitch Change Related Variables

    bool wantSpaPitchChangeOnFocus = true;                      // bool variable indicates if developer wants the spaAudioSource's pitch to change

    float spaOriginPitch;                                       // The original value of pitch on spaAudioSource 
    readonly float pitchMulti = 1.5f;                           // [Default = 1.5f] Multiplier to apply to pitch when it's focused by user head

    HeadCaster headCaster;                                      // A class on user's head which is used for casting ray/capsule/etc...

    bool spaFocusStatus = false;                                // The focus status (TRUE/ FALSE) of the current frame
    bool spaLastFocusStatus = false;                            // The focus status (TRUE/ FALSE) in the last frame

    #endregion


    /// <summary>
    /// Function Start() is called in the beginning of each frame
    /// </summary>
    private void Start()
    {
        /* Making the Music Player completely invisible when the game start */
        //GetComponent<MeshRenderer>().enabled = false;

        /* Initialize important member variables */
        InitVariables();

        /* Moves the "MusicPlay" off the user prefab on hierarchy */
        MoveMusicPlayerOffUser();
    }


    /// <summary>
    /// Function is called on every frame
    /// </summary>
    void Update()
    {
        /* If user want the pitch to change, we dynamically
         * change pitch based on "if user head focus on
         * MusicPlayer's direction or not" */
        if (wantSpaPitchChangeOnFocus)
            OnFocusChange_Spa();
    }


    /// <summary>
    /// Function initialize important member variables
    /// </summary>
    private void InitVariables()
    {
        spaAudioSource = transform.GetComponent<AudioSource>();
        headCaster = GameObject.Find("User/Head").GetComponent<HeadCaster>();
        spaOriginPitch = spaAudioSource.pitch;
    }


    /// <summary>
    /// Function for transport the Music Player to a specific position indicated
    /// </summary>
    public void TransportMusicPlayer(Vector3 destinationPoint)
    {
        transform.position = destinationPoint;
    }


    /// <summary>
    /// The function is for matching and sound feedback from list and play it
    /// </summary>
    public void PlaySpaAudio(string clipName)
    {
        /* Find the audioClip */
        AudioClip tempAudioClip = audioList.Find(x => x.name == clipName);

        /* Play one shot of the audio */
        if (!spaAudioSource.isPlaying)
            spaAudioSource.PlayOneShot(tempAudioClip);
    }


    /// <summary>
    /// The function stops the SpaAudioSource
    /// </summary>
    public void StopSpaAudio()
    {
        /* Play one shot of the audio */
        if (spaAudioSource.isPlaying)
            spaAudioSource.Stop();
    }


    /// <summary>
    /// Function return if the spatial audio source is playing
    /// </summary>
    public bool IsSpaAudioPlaying()
    {
        /* Return based on reality if "spaAudioSource" is still available.
         * Otherwise, we just return FALSE */
        if (spaAudioSource)
            return spaAudioSource.isPlaying;
        else
            return false;
    }


    /// <summary>
    /// Function moves the MusicPlayer off the user prefab.
    /// Otherwise, it will rotate along with the user when within Unity Editor.
    /// </summary>
    private void MoveMusicPlayerOffUser()
    {
        transform.SetParent(null);
    }


    /// <summary>
    /// Function do something when the MusicPlayer is "focused/unfocused" by user's head
    /// "Focus" means the "user head is facing the direction of MusicPlayer".
    /// When focus status is changed, we change the pitch of SpaAudioSource.
    /// </summary>
    void OnFocusChange_Spa()
    {
        /* Get all hits information from head's absolute front capsule cast */
        RaycastHit[] hits = headCaster.AbsFrontCapsuleHits;

        /* If user's face is facing the MusicPlayer, turn on "spaFocusStatus" */
        if (hits != null && Array.Exists(hits, hit => hit.transform == transform))
            spaFocusStatus = true;
        else
            spaFocusStatus = false;

        /* We will try to change pitch of SpaAudioSource if the focus status is changed */
        if (spaFocusStatus != spaLastFocusStatus)
            ChangePitchBasedOnFocus_Spa();

        /* Update "lastFocusStatus" boolean variable */
        spaLastFocusStatus = spaFocusStatus;
    }


    /// <summary>
    /// Change the pitch of SpaAudioSource on MusicPlayer
    /// </summary>
    void ChangePitchBasedOnFocus_Spa()
    {
        if (spaFocusStatus)
            spaAudioSource.pitch = spaOriginPitch * pitchMulti;       // If focusStatus is TRUE, raise the pitch
        else
            spaAudioSource.pitch = spaOriginPitch;                    // If focusStatus is FALSE, revert pitch back to original
    }

}

