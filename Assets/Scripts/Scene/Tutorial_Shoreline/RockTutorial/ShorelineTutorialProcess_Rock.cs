using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class ShorelineTutorialProcess_Rock : MonoBehaviour
{
    ShorelineTutorial_Rock shorelineTutorial_Rock;      // Classes for controlling the shoreline exercises "Rock"
    VerbalManager_General verbalManager_General;        // The class for giving speech
    SceneInitialGuide sceneInitialGuide;                // The class control the initial verbal guide
    NavigationManager navigationManager;                // The class manages the Navigation System

    /* Messages we want to convey to users */
    string rockEndMessage = "Now, let jump to the 'advanced' shorelining tutorial.";


    private void Awake()
    {
        InitVariables();        // Initialize the member variables
    }


    // Start is called before the first frame update
    void Start()
    {
        RockExerciseProcess();
    }


    /// <summary>
    /// Function for ending shoreline rock tutorial ===> Jump to the next shorelineTutorial
    /// </summary>
    void ToEndRockTutorial()
    {
        verbalManager_General.SpeakWaitAndCallback(rockEndMessage, () =>
        {
            /* (Temporary) do all the resets needed then back to the main menu */
            SceneJumpHelper.ResetThenSwitchScene("Tutorial_Shoreline_Quiz");
        });
    }


    /// <summary>
    /// Function initialize member variables of this class as preparation
    /// </summary>
    void InitVariables()
    {
        /* Classes for controlling the exercises in the tutorial */
        shorelineTutorial_Rock = GameObject.Find("SceneManager").GetComponent<ShorelineTutorial_Rock>();

        /* Class for giving speech using UAP */
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();

        /* The initial guide of the scene */
        sceneInitialGuide = GameObject.Find("SceneManager").GetComponent<SceneInitialGuide>();

        /* Class manages the NavigationSystem */
        navigationManager = GameObject.Find("NavigationSystem").GetComponent<NavigationManager>();
    }


    /// <summary>
    /// Function kicks off the rock exercise and wait for it to end
    /// </summary>
    async void RockExerciseProcess()
    {
        await Task.Delay(500);                        // Delay 0.5s to start

        while (sceneInitialGuide.IsRunning)           // Wait for the sceneInitialGuide to finish running 
            await Task.Yield();

        navigationManager.RestartNavSystem();         // Start the navigation system to activate the way point. However, it's not for navigation purpose, so I didn't selected "HasNavigation" in SceneInitialGuide class
        shorelineTutorial_Rock.StartExercise();       // Start the shorelining rock exercise

        while (shorelineTutorial_Rock.IsRunning)      // Wait for the exercise to complete
            await Task.Yield();

        ToEndRockTutorial();                          // End the shorelineTutorial_Rock
    }
}

