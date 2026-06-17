using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


public class SceneJumpHelper
{
    /* The wait time for processing everything before back to main (0.05s) */
    static int waitTime = 50;

    /// <summary>
    /// Function resets everything needed before jump the user to another scene.
    /// If don't do these resets & if you uses headphone tracking ===> your App might crash
    /// 
    /// Since to reset as of 06/16/2022:
    /// 1. Reset AR Session to put avatar back to origin position before leaving a scene
    /// 2. Try to reset the headphone tracking if it was is use (this prevents the app crash)
    ///
    /// [Parameter]
    /// sceneName: the scene you want to jump to
    /// 
    /// </summary>
    static public async void ResetThenSwitchScene(string sceneName)
    {
        // /* Get the name of current scene which called this function */
        string currentScene = SceneManager.GetActiveScene().name;

        // /* Try to stop the current headphone tracking if we are using it */
        // GameObject.Find("User").GetComponent<HeadphoneTracker>().TurnOffTracking();

        // /* Reset the AR session*/
        // GameObject.Find("SettingManager").GetComponent<ArSetting>().ResetArSession();

        // /* Wait for the reset steps to complete*/
        // await Task.Delay(waitTime);

        /* Jump to the main menu */
        if (Application.isPlaying && currentScene == SceneManager.GetActiveScene().name)
            SceneManager.LoadScene(sceneName);
    }

}
