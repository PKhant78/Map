using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;


public class SceneInitialGuide : MonoBehaviour
{
    bool isRunning = false;                                 // bool variable indicates whether a the "Scene Initial Guide" is running or not

    public bool hasInitialInstruction = false;              // variable signify whether this scene has initial instruction session or not
    public bool hasInitialNavigation = false;               // bool variable indicates whether we have an initial navigation session. We will place it right after finishing the initial instruction session

    InstructionManager instructionManager;                  // the instruction manager class which provides interactable in-scene instruction
    NavigationManager navigationManager;                    // the navigation manager

    public TextAsset instructionTextFile;                   // the variable for storing initial instruction text file
    public string signForSplit = "<br>";                    // sign for splitting the instruction text
    string instructionText = "";                            // instruction text (default is empty)

    /* The name of navigation system (GameObject name)
     * which is used for initial navigation session 
     * Why do we ask for this? ===> Because a scene can 
     * have multiple navigation system for different purpose */
    public string navigationSystemName = "NavigationSystem";

    public List<AudioClip> audiosForInstruction = new();    // A list of AudioClips to play during instruction
    public List<int> playAudioAt = new();                   // "Play audio at which instruction" ===> a list of integer means the index of instruction ===> the list is used to identify after which instruction (by their index), we play the audioClip


    // Start is called before the first frame update
    void Start()
    {
        /* Change the initial process running status to TRUE once started the scene
         * Because we will run the SceneInitialGuide immediately! */
        isRunning = true;

        /* Begin the initial guide process (you can choose to implement instruction/navigation or both, or none) 
         * Make it a little bit (e.g., 0.5 sec) later than Start function, so the other things in the scene can 
         * have enough time to setup. It will prevent causing errors like "NullReferenceException". 
         */
        Invoke(nameof(StartInitialGuideProcess), 0.5f);
    }


    /// <summary>
    /// Function kicks off all the initial processes, includes:
    /// 1. Initial instruction session
    /// 2. Initial navigation session
    /// </summary>
    void StartInitialGuideProcess()
    {
        /* Call the function to start the initial instruction */
        StartInitInstruction();

        /* Call an async function to wait until the initial instruction session to finish before starting the initial navigation session */
        AsyncStartInitNavigation();
    }


    /// <summary>
    /// Function which is able to start the initial instruction session when entering the scene
    ///
    /// [Note]
    /// For starting the initial instruction session, the developer must select "hasInitialInstruction == true"
    /// in Unity Editor, under script "InitialInstructionManager.cs".
    /// 
    /// </summary>
    void StartInitInstruction()
    {
        /* If there will be initial instruction as developer stated */
        if (hasInitialInstruction)
        {
            /* Parse the provided initial instruction text */
            instructionText = UsefulTools.ParseTextAsset(instructionTextFile);

            /* Find the Instruction System object and its manager script from the scene */
            instructionManager = GameObject.Find("InstructionSystem").GetComponent<InstructionManager>();

            /* Assign audio instruction related list to variables in instruction system */
            instructionManager.AudiosForInstruction = audiosForInstruction;
            instructionManager.PlayAudioAt = playAudioAt;

            /* Start the initial instruciton session */
            instructionManager.StartInstructionSystem(instructionText, signForSplit);
        }
    }


    /// <summary>
    /// Wait the instruction manager to end before starting the initial navigation session
    ///
    /// [Note]
    /// 1.For the initial navigation session to start after finishing the instruction session, the developer 
    /// must select "FollowedByNavSession = true" in Unity Editor, under script "InitialInstructionManager.cs".
    /// 
    /// 2. If the initial instruction session is never started, and developer selected "FollowedByNavSession = true",
    /// the initial navigation session will start right a way after entering the scene
    /// 
    /// </summary>
    async void AsyncStartInitNavigation()
    {
        /* Wait for the instruction session to finish running */
        while (hasInitialInstruction && instructionManager.IsRunning)
        {
            await Task.Yield();
        }

        /* Call the initial navigation tour right after the instruction session if the developer decided to do so */
        if (hasInitialNavigation)
        {
            /* Find the navigationSystem in the scene and get the manager script */
            navigationManager = GameObject.Find(navigationSystemName).GetComponent<NavigationManager>();

            /* Start the navigation system */
            navigationManager.RestartNavSystem();
        }

        /* Wait for the navigation tour to end before updating the "isRunning" boolean variable */
        while (hasInitialNavigation && navigationManager.IsRunning)
        {
            await Task.Yield();
        }

        isRunning = false;
        Debug.Log("Scene Initial Guide ended!");
    }


    /// <summary>
    /// Getter of the isRunning boolean value
    /// </summary>
    public bool IsRunning
    {
        get { return isRunning; }
    }

}

