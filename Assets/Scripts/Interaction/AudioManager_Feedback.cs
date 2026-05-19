using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;   // For Version 2



// ============================================================================================ //
// ================== (Version 1) Using List to manually load music and play ================== //
// ============================================================================================ //


public class AudioManager_Feedback : MonoBehaviour
{
    AudioSource[] audioSources;                        // An array for storing all audioSources
    public AudioSource feedbackAudioSource_Object;     // The audio source for playing object related sound
    public AudioSource feedbackAudioSource_Floor;      // The audio source for playing floor related sound. We separated audioSource, because we want to head "lower" volume of Floor sound.

    string nameOfAudioPlaying = "N/A";                 // Record the name (eg. Wood Hit, Wood Slide, etc...) of the music which is currently playing
    string typeOfAudioPlaying = "N/A";                 // Record the type (eg. Hit, Slide, Alert, etc...) of the music which is currently playing

    public List<AudioClip> feedbackAudioList = new List<AudioClip>();


    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        /* Assign the audioSource component which the current object has */
        audioSources = GetComponents<AudioSource>();
        feedbackAudioSource_Object = audioSources[0];
        feedbackAudioSource_Floor = audioSources[1];

        /* Turn off all audioListener and only turn on the one on Head Vision
         * So the user can hear 3D sound */
        var AudioListeners = FindObjectsOfType<AudioListener>();
        foreach (AudioListener al in AudioListeners)
            al.enabled = false;
        GameObject.Find("HeadVision").GetComponent<AudioListener>().enabled = true;
    }


    /// <summary>
    /// The function is for matching and sound feedback from list and play it
    /// </summary>

    public void PlayFeedbackAudio(string clipName)
    {
        AudioClip tempAudioClip;

        nameOfAudioPlaying = clipName;
        string[] nameSplitList = nameOfAudioPlaying.Split(" ");
        typeOfAudioPlaying = nameSplitList[nameSplitList.Length - 1];

        tempAudioClip = feedbackAudioList.Find(x => x.name == clipName);

        /* Playing music based on the type of clip */
        if (clipName.Contains("Floor"))
            PlayOneShotOfAudio(feedbackAudioSource_Floor, tempAudioClip);
        else
            PlayOneShotOfAudio(feedbackAudioSource_Object, tempAudioClip);
    }


    /// <summary>
    /// General function for playing one shot of audio on specific audioSource
    /// </summary>
    private void PlayOneShotOfAudio(AudioSource source, AudioClip clip)
    {
        /* Playing new music only if all audioSources are not playing */
        foreach (AudioSource s in audioSources)
        {
            if (s.isPlaying)
                return;
        }

        source.PlayOneShot(clip);
    }


    /// <summary>
    /// The function is for stopping an ongoing audio feedback IMMEDIATELY if it match the indicated Audio Type (eg. Hit, Slide, Alert) to stop
    /// Audio Feedback Priority: Alert > Hit > Slide
    /// </summary>
    public void StopFeedbackAudioByType(string audioTypeToStop)
    {
        if (typeOfAudioPlaying == audioTypeToStop)
        {
            /* Try to stop all of audioSources.
             * Because based on our designed logic ===> there will always be only
             * one running audioSource ===> so we attempt to close all of them */
            StopAllAudioSource();

            Debug.Log("Stop ongoing " + typeOfAudioPlaying + " audio feedback");
        }
    }


    /// <summary>
    /// Function stop the audio for a specific audioSource
    /// </summary>
    public void StopAudioSource(AudioSource source)
    {
        if (source.isPlaying)
            source.Stop();
    }


    /// <summary>
    /// Function tries to stop all of audioSources
    /// </summary>
    private void StopAllAudioSource()
    {
        foreach (AudioSource source in audioSources)
            StopAudioSource(source);
    }

}

























// ======================================================================================== //
// ================== (Version 2) Using Coroutine to Directly play Music ================== //
// ======================================================================================== //


//public class AudioManager_Feedback : MonoBehaviour
//{

//    public AudioSource feedbackAudioSource; // The only audio source for playing sound

//    string appDirectory;    // Path of the application
//    string audioDirectory;  // Path where the audios stored

//    string nameOfAudioPlaying;  // Record the name (eg. Wood Hit, Wood Slide, etc...) of the music which is currently playing
//    string typeOfAudioPlaying;  // Record the type (eg. Hit, Slide, Alert, etc...) of the music which is currently playing


//    /// <summary>
//    /// Start is called before the first frame update
//    /// </summary>
//    private void Start()
//    {
//        /* Assign the audioSource component which the current object has */
//        feedbackAudioSource = GetComponent<AudioSource>();

//        /* Assign "App Address" and "Audio Address" */
//        appDirectory = "file://" + Application.dataPath.Replace("Assets", "");   // Address of the application ".../" before "Assets/..."
//        audioDirectory = "Assets/Sounds/Feedbacks/Object";
//    }



//    /// <summary>
//    /// Coroutine Function for acquiring one feedback audio based on audioPath
//    /// </summary>

//    private IEnumerator PlayOneAudio(string audioPath)
//    {
//        AudioClip tempAudioClip;

//        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.WAV))
//        {
//            yield return www.SendWebRequest();

//            /* Request and get the audioClip */
//            if (www.result == UnityWebRequest.Result.ConnectionError)
//            {
//                Debug.Log(www.error);
//            }
//            else
//            {
//                tempAudioClip = DownloadHandlerAudioClip.GetContent(www);

//                /* Playing music */
//                if (!feedbackAudioSource.isPlaying)
//                {
//                    feedbackAudioSource.PlayOneShot(tempAudioClip);
//                }
//            }

//            /* Store audio-type and audio-name to global variables */
//            string[] pathSplitList = audioPath.Split("/");  // Split the audioPath on "/"
//            nameOfAudioPlaying = pathSplitList[pathSplitList.Length - 1].Replace(".wav", "");   //the last element in the array, after removing suffix (eg. ".wav"), is the audio name like "Wood Hit"
//            string[] nameSplitList = nameOfAudioPlaying.Split(" ");
//            typeOfAudioPlaying = nameSplitList[nameSplitList.Length - 1];
//        }
//    }


//    /// <summary>
//    /// The function is for matching and sound feedback from list and play it
//    /// </summary>

//    public void PlayFeedbackAudio(string clipName)
//    {
//        /* generate the path of audioClip based on the provided clipName */
//        string clipPath = appDirectory + audioDirectory + "/" + clipName + ".wav";

//        /* Play this feedback audioClip */
//        StartCoroutine(PlayOneAudio(clipPath));
//    }


//    ///// <summary>
//    ///// The function is for stopping an ongoing audio feedback IMMEDIATELY if it match the indicated Audio Type (eg. Hit, Slide, Alert) to stop
//    ///// Audio Feedback Priority: Alert > Hit > Slide
//    ///// </summary>
//    public void StopFeedbackAudioByType(string audioTypeToStop)
//    {
//        if ((feedbackAudioSource.isPlaying) && (typeOfAudioPlaying == audioTypeToStop))
//        {
//            Debug.Log("Stop ongoing " + typeOfAudioPlaying + " audio feedback");
//            feedbackAudioSource.Stop();
//        }
//    }


//}
