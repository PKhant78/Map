using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class ShorelineTutorialProcess_Quiz : MonoBehaviour
{
    ShorelineTutorial_Quiz shorelineTutorial_Quiz;      // Classes for controlling the shoreline exercises "Quiz"
    VerbalManager_General verbalManager_General;        // The class for giving speech
    SceneInitialGuide sceneInitialGuide;                // The class control the initial verbal guide

    /* Messages we want to convey to users */
    string quizEndMessage = "Now, let jump to the Main Menu.";


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
    /// Function for ending shoreline quiz tutorial ===> Jump to the Main Menu
    /// </summary>
    void ToEndQuizTutorial()
    {
        verbalManager_General.SpeakWaitAndCallback(quizEndMessage, () =>
        {
            /* Do all the resets needed then back to the main menu */
            SceneJumpHelper.ResetThenSwitchScene("MainMenu");
        });
    }


    /// <summary>
    /// Function initialize member variables of this class as preparation
    /// </summary>
    void InitVariables()
    {
        /* Classes for controlling the exercises in the tutorial */
        shorelineTutorial_Quiz = GameObject.Find("SceneManager").GetComponent<ShorelineTutorial_Quiz>();

        /* Class for giving speech using UAP */
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();

        /* The initial guide of the scene */
        sceneInitialGuide = GameObject.Find("SceneManager").GetComponent<SceneInitialGuide>();
    }


    /// <summary>
    /// Function kicks off the quiz exercise and wait for it to end
    /// </summary>
    async void RockExerciseProcess()
    {
        await Task.Delay(500);                        // Delay 0.5s to start

        while (sceneInitialGuide.IsRunning)           // Wait for the sceneInitialGuide to finish running 
            await Task.Yield();

        shorelineTutorial_Quiz.StartExercise();       // Start the shorelining quiz exercise

        while (shorelineTutorial_Quiz.IsRunning)      // Wait for the exercise to complete
            await Task.Yield();

        ToEndQuizTutorial();                          // End the shorelineTutorial_Quiz
    }
}
