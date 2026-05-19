using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class WayPointTutorialProcess_Walk : MonoBehaviour
{
    WayPointTutorial_Walk wayPointTutorial_Walk;        // Classes for controlling the way point exercises "walk"
    VerbalManager_General verbalManager_General;        // The class for giving speech
    SceneInitialGuide sceneInitialGuide;                // The class control the initial verbal guide

    /* Messages we want to convey to users */
    string walkEndMessage = "Congratulation! You have finished the way point walking tutorial. You did really good job. Now let us go to the next tutorial. To learn about how to do shorelining in this App.";


    private void Awake()
    {
        InitVariables();        // Initialize the member variables
    }


    // Start is called before the first frame update
    void Start()
    {
        WalkExerciseProcess();
    }


    /// <summary>
    /// Function initialize member variables of this class as preparation
    /// </summary>
    void InitVariables()
    {
        /* Classes for controlling the exercises in the tutorial */
        wayPointTutorial_Walk = GameObject.Find("SceneManager").GetComponent<WayPointTutorial_Walk>();

        /* Class for giving speech using UAP */
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();

        /* The initial guide of the scene */
        sceneInitialGuide = GameObject.Find("SceneManager").GetComponent<SceneInitialGuide>();
    }


    /// <summary>
    /// Function for ending wayPoint_Walk tutorial ===> Jump to mainMenu
    /// </summary>
    void ToEndWalkTutorial()
    {
        verbalManager_General.SpeakWaitAndCallback(walkEndMessage, () =>
        {
            /* do all the resets need   ed then back to the main menu */
            SceneJumpHelper.ResetThenSwitchScene("Tutorial_Shoreline_Rock");
        });
    }


    /// <summary>
    /// Function kicks off the circle exercise and wait for it to end
    /// </summary>
    async void WalkExerciseProcess()
    {
        await Task.Delay(500);                        // Delay 0.5s to start

        while (sceneInitialGuide.IsRunning)           // Wait for the sceneInitialGuide to finish running 
            await Task.Yield();

        wayPointTutorial_Walk.StartExercise(true);    // Start the exercise

        while (wayPointTutorial_Walk.IsRunning)       // Wait for the exercise to complete
            await Task.Yield();

        ToEndWalkTutorial();                          // End the circle tutorial
    }
}

