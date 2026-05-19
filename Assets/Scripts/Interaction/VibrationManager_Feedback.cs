using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.NiceVibrations;


public class VibrationManager_Feedback : MonoBehaviour
{
    /* The vibration intensity, sharpness & duration */
    float hitIntensity = 1f;
    float hitSharpness = 1f;
    float hitDuration = 0.1f;

    /* Name and type of vibration that is playing */
    string nameOfVibrationPlaying = "N/A";
    string typeOfVibrationPlaying = "N/A";


    /// <summary>
    /// Start different types of vibration
    ///
    /// [materialAndAction]
    /// The combination of material + action. For example, "Wood hit", "Wood Slide", "VibrationSlow Alert"
    /// </summary>
    public void StartVibration(string materialAndAction)
    {
        /* We don't provide any vibration for floor object */
        if (materialAndAction.Contains("Floor"))
            return;

        /* Stop Previous vibration */
        StopAllVibration();

        /* Play vibration based on material and action */
        if (materialAndAction.Contains("Hit"))
        {
            Debug.Log("Playing hit physical vibration.");
            MMVibrationManager.ContinuousHaptic(hitIntensity, hitSharpness, hitDuration, HapticTypes.Failure, this, true);
        }

        /* Record the name and type of vibration playing */
        nameOfVibrationPlaying = materialAndAction;
        string[] nameSplitList = nameOfVibrationPlaying.Split(" ");
        typeOfVibrationPlaying = nameSplitList[nameSplitList.Length - 1];
    }


    /// <summary>
    /// Function stops all on-going vibrations
    /// </summary>
    public void StopAllVibration()
    {
        MMVibrationManager.StopAllHaptics();
    }


    /// <summary>
    /// Stop a specific type of vibration if it's playing
    /// </summary>
    public void StopVibrationByType(string vibrationTypeToStop)
    {
        /* If "vibration type we want to stop" is same as the "vibration type we are playing", we stop it.
         * Only one vibration will play at one time ===> so we call function to stop all vibration */
        if (vibrationTypeToStop == typeOfVibrationPlaying)
            StopAllVibration();
    }

}

