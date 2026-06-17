using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class WayPointTutorialProcess_Circle : MonoBehaviour
{
    WayPointTutorial_Circle wayPointTutorial_Circle;    // Classes for controlling the way point exercises "walk"
    VerbalManager_General verbalManager_General;        // The class for giving speech
    SceneInitialGuide sceneInitialGuide;                // The class control the initial verbal guide

    /* Messages we want to convey to users */
    string circleEndMessage = "Awesome! You have correctly pointed out the direction of all way points. In the next exercise, you will walk towards the drum beats 3D sound to collect 5 way points.";

   

    private void Awake()
    {
        InitVariables();        // Initialize the member variables
    }


    // Start is called before the first frame update
    void Start()
    {
        CircleExerciseProcess();
    }


    /// <summary>
    /// Function initialize member variables of this class as preparation
    /// </summary>
    void InitVariables()
    {
        /* Classes for controlling the exercises in the tutorial */
        wayPointTutorial_Circle = GameObject.Find("SceneManager").GetComponent<WayPointTutorial_Circle>();

        /* Class for giving speech using UAP */
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();

        /* The initial guide of the scene */
        sceneInitialGuide = GameObject.Find("SceneManager").GetComponent<SceneInitialGuide>();
    }


    /// <summary>
    /// Function for ending wayPoint_Circle tutorial ===> Jump to wayPoint_Walk tutorial
    /// </summary>
    void ToEndCircleTutorial()
    {
        verbalManager_General.SpeakWaitAndCallback(circleEndMessage, () =>
        {
            /* do all the resets needed then back to the main menu */
            SceneJumpHelper.ResetThenSwitchScene("Tutorial_WayPoint_Walk");
        });
    }


    /// <summary>
    /// Function kicks off the circle exercise and wait for it to end
    /// </summary>
    async void CircleExerciseProcess()
    {
        await Task.Delay(500);                          // Delay 0.5s to start

        while (sceneInitialGuide.IsRunning)             // Wait for the sceneInitialGuide to finish running 
            await Task.Yield();

        wayPointTutorial_Circle.StartExercise();        // Start the exercise

        while (wayPointTutorial_Circle.IsRunning)       // Wait for the exercise to complete
            await Task.Yield();

        ToEndCircleTutorial();                          // End the circle tutorial
    }
}

