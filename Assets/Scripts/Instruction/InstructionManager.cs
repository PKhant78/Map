using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


public class InstructionManager : MonoBehaviour
{
    public static bool allowInstruction = false;             // [Default = True] Boolean variable indicates if the system allow providing instruction or not
    bool isRunning;                                         // Bool var indicates whether the instruciton system is running or not

    string currentScene;                                    // record the name of current scene

    AudioSource insAudioSource;                             // AudioSource on instruction system for playing audioClips along with instruction
    List<AudioClip> audiosForInstruction = new();           // A list of AudioClips to play during instruction
    List<int> playAudioAt = new();                          // "Play audio at which instruction" ===> a list of integer means the index of instruction ===> the list is used to identify after which instruction (by their index), we play the audioClip

    int followTime;                                         // how many seconds the instruction system will follow avatar when system started 
    bool startFollow;                                       // a boolean variable decide whether let instruction system follow the avatar or not

    VerbalManager_General verbalManager_General;            // a class for speaking textual instructions
    ObjectsActivenessController activenessController;       // a class which can be used to control the activeness status of interable objects in the scene

    List<string> texts = new();                             // a list of splitted instruction text
    int iterator;                                           // an iterator for tracking which sentence to play

    Vector3 repositoryPos;                                  // the position of the repo for storing instruction system when not using it                 

    Transform instructionSystemTrans;                       // the transform of instantiated instruction system
    Transform gripPointTrans;                               // the transform of user avatar's grip point
    Transform headTrans;                                    // the transform of user avatar's head

    static readonly int countDownSeconds = 3;               // the number of seconds for countdown before ending the instruction session

    /* The class act as an API for controlling user's translation lock in the scene */
    UserTranslationController userTranslationController;

    /* Fixed Sentences to be used in the instruction system */
    readonly string startNotice = "Please don't move, and place the cane in front of your body. Instruction session started.";
    readonly string endNotice = $"Please turn your body and cane back to your initial facing direction. The instruction session will end in {countDownSeconds} seconds.";
    readonly string endConfirmNotice = "Instruction session end.";
    readonly string startMessage = "Using cane to hit panel on the right side of your body to hear next instruction. " + "Hit panel on your left side to hear previous instruction.";
    readonly string noMorePrevNotice = "No more previous instruction";
    readonly string noMoreNextNotice = "No more next instruction. " + "To quit the instruction session, move out the cane and hit the panel again!";


    /// <summary>
    /// Awake is always executed first by Unity.
    /// Put it in awake, so the "verbalManger_General" will be prepared be call in other class's Start() if needed
    ///</summary>
    void Awake()
    {
        /* Set isRunning to false when scene begin, because instruction system is not running by that time */
        isRunning = false;

        /* Get the name of current scene */
        currentScene = SceneManager.GetActiveScene().name;

        /* Initialize variables which adjust user translation lock */
        userTranslationController = GetComponent<UserTranslationController>();

        /* Instantiate an object of "ObjectsActivenessController" class */
        activenessController = new ObjectsActivenessController();

        /* Instantiate the verbal manager general */
        verbalManager_General = GetComponent<VerbalManager_General>();

        /* The AudioSource on instruction system for playing audio to help providing better instruction */
        insAudioSource = transform.GetComponent<AudioSource>();

        /* Instantiate a reference to the Instruction System transfrom */
        instructionSystemTrans = GameObject.Find("InstructionSystem").transform;

        /* Instantiate a reference to the Avatar's gripPoint & head transfrom */
        gripPointTrans = GameObject.Find("User/GripPoint").transform;
        headTrans = GameObject.Find("User/Head").transform;

        /* Initiate the repository position */
        repositoryPos = new Vector3(0, -50, 0);

        /* Instruction don't need to follow when initialize ==> so Flase */
        startFollow = false;

        /* Initialize the "time for instruction system to follow avatar" (i.e. followTime = 3000 ===> 3 seconds) */
        followTime = 5000;

        /* When begin, move the instruction system into repository just in case it won't be used immediately */
        MoveInstructionSystemToRepo();
    }


    /// <summary>
    /// Update is called on every frame
    ///</summary>
    private void Update()
    {
        /* If boolean "startFollow" is True, let instruction system follow the user on every frame */
        if (startFollow)
            MoveInstructionSystemToUser();
    }


    /// <summary>
    /// Function starts an instruction session
    ///
    /// [Parameters]
    /// 1. "instructionText" is the paragraph of instruction developer wants user to hear
    /// 2. "signForSplit" is a string indicates where to split the "instructionText" into short sentences (eg. "||", ".", "\n", etc...)
    /// </summary>
    public async void StartInstructionSystem(string instructionText, string signForSplit)
    {
        /* Don't do anything if the system doesn't allow instruction */
        if (!allowInstruction)
            return;

        /* Once system start to run, turn on isRunning bool immediately */
        isRunning = true;

        /* Temporarily disable user avatar's movement (translation) */
        //userTranslationController.PauseUserTranslation();

        /* Disable other objects in the scene to prevent them inturrupting the instruction system */
        //activenessController.DisableOtherObjs();

        /* Send starting message*/
        Debug.Log(startNotice);
        verbalManager_General.Speak(startNotice);

        /* Split the instruction text into sentences */
        PrepareInstructionTexts(instructionText, signForSplit);

        /* Initialize the iterator */
        iterator = -1;

        /* Update the boolean to True to enable moving instruction system to side of the user and keep following.
         * After X seconds, turn off the boolean to let the system stop following and settle down */
        startFollow = true;
        await Task.Delay(followTime);
        startFollow = false;

        /* Wait for startNotice to finish and speak the startMessage */
        if (Application.isPlaying && currentScene == SceneManager.GetActiveScene().name)
            verbalManager_General.WaitThenSpeak(startMessage);
    }


    /// <summary>
    /// Function ends an instruction session
    /// </summary>
    public void EndInstructionSystem()
    {
        /* move instruction system to repository */
        MoveInstructionSystemToRepo();

        /* Send ending message */
        Debug.Log(endNotice);
        verbalManager_General.SpeakWaitAndCallback(endNotice, async () =>
        {
            /* Just in case if the scene is closed, stop the async process */
            if (currentScene != SceneManager.GetActiveScene().name)
                return;

            /* Countdown the time to end and speak it out (e.g., "three...two...one") */
            await verbalManager_General.SpeakCountdown(countDownSeconds, true);

            /* Just in case if the scene is closed, stop the async process */
            if (currentScene != SceneManager.GetActiveScene().name)
                return;

            /* Speak the confirmation ending notice "Instruction session end" */
            await verbalManager_General.SpeakAndWait(endConfirmNotice);

            /* Just in case if the scene is closed, stop the async process */
            if (currentScene != SceneManager.GetActiveScene().name)
                return;

            /* Other setups to do before ending an instruction session */
            OtherSetupsBeforeEnd();
        });
    }


    /// <summary>
    /// Function for doing all other setups after instruction session ends
    /// run it after instruction session ends and after speaking all the async UAP messages
    /// </summary>
    private void OtherSetupsBeforeEnd()
    {
        /* Clear the audio instruction related lists.
         * Don't use "Clear()", because the list here points to the same
         * list where contains the reference to the audioClip related info*/
        audiosForInstruction = new();
        playAudioAt = new();

        /* Reactivate all other objects */
        activenessController.EnableOtherObjs();

        /* re-enable user avatar's movement (translation) */
        userTranslationController.ResumeUserTranslation();

        /* turn "isRunning" back to false after instruction system is closed */
        isRunning = false;
    }


    /// <summary>
    /// Function clean the given instructionText and split it into a list of short instruction sentences
    /// </summary>
    private void PrepareInstructionTexts(string instructionText, string signForSplit)
    {
        /* Clear the "texts" instruction sentence list */
        texts.Clear();

        /* Clean the input "instructionText" by removing extra space */
        string[] tempArr = instructionText.Split(" ");
        List<string> tempList = new(tempArr);
        tempList.RemoveAll(s => s == "");
        string cleanText = string.Join(" ", tempList);

        /* Prepare the texts list according to if any instruction provided or not */
        if (cleanText == "")
        {
            texts.Add("No instruction available, please quit this instruction session.");
        }
        else
        {
            /* Split cleaned instruction text into a "texts" list */
            string[] splitArr = UsefulTools.SplitText(cleanText, signForSplit);
            texts = new List<string>(splitArr);
            texts.RemoveAll(s => s == "");
        }
    }


    /// <summary>
    /// Function moves the instruction system prefab in the scene to its repository
    /// </summary>
    void MoveInstructionSystemToRepo()
    {
        instructionSystemTrans.position = repositoryPos;
    }


    /// <summary>
    /// Function moves the instruction system prefab in the scene to the user
    /// </summary>
    void MoveInstructionSystemToUser()
    {
        /* [Important Note]
         * 
         * 1. Don't make "User" GameObject in the scene as parent of "instructionSystem", because in the IOS 
         *    ARControl, the ARCamera movement & rotaion only affect movement & rotation of GripPoint, Head,
         *    and Body. The "User" empty GameObject will stay at the starting point forever! I know using "User"
         *    will work under "Unity Editor Control" because in that script, the movment is implemented on "User"
         *    gameObject.
         *    
         * 2. For now, don't use "Body" as a all-in-one gameObject for sync both rotation and position. This is
         *    because that in the "IOS AR Control", the body's rotation won't change forever... It won't rotate
         *    when the cane points to another direction. We did this because we didn't need it to rotate, but this
         *    might be changed in the future if needed. By that time, body can be used as a all-in-one resource.
         */


        /* Set GripPoint as temporary parent of instructionSystem to adjust instructionSystem's rotation */
        instructionSystemTrans.SetParent(gripPointTrans);
        instructionSystemTrans.localEulerAngles = Vector3.zero;

        /* Set Avatar's Head as temporary parent of instructionSystem to adjust instructionSystem's position */
        instructionSystemTrans.SetParent(headTrans);
        instructionSystemTrans.localPosition = Vector3.zero;

        /* set instructionSystem as child of the scene */
        instructionSystemTrans.SetParent(null);

        /* change the positional y-axis of instructionSystemTrans to 0 to put the panel on the ground */
        instructionSystemTrans.position = Vector3.Scale(instructionSystemTrans.position, new Vector3(1, 0, 1));

        /* change the rotational x-axis of instructionSystemTrans to 0 to make it perpendicular to the ground */
        instructionSystemTrans.eulerAngles = Vector3.Scale(instructionSystemTrans.eulerAngles, new Vector3(0, 1, 1));
    }


    /// <summary>
    /// Function reads the current sentence which iterator is pointing to
    /// </summary>
    async void ReadCurrInstruction()
    {
        /* Speak the instructional text */
        verbalManager_General.Speak(texts[iterator]);

        /* When the list has audioClips, and developer specified "place to play" for all audioClips.
         * If we arrive at the instruction where developer want to add audioClip, do the following */
        if (audiosForInstruction.Count != 0 && audiosForInstruction.Count == playAudioAt.Count && playAudioAt.Contains(iterator))
        {
            /* Save the value of current iterator */
            int saveIterator = iterator;

            /* Wait for the UAP to finish speech */
            while (await verbalManager_General.IsSeaking_Improved())
                await Task.Yield();

            /* If the scene is still active and the user stays at the current instruction (didn't hit panel) */
            if (currentScene == SceneManager.GetActiveScene().name && iterator == saveIterator)
            {
                /* Find the index of the audio we want to play */
                int audioIdx = playAudioAt.IndexOf(iterator);

                /* Play the audioClip */
                insAudioSource.PlayOneShot(audiosForInstruction[audioIdx]);
            }
        }
    }


    /// <summary>
    /// Function reads next instruction from splited instruction list.
    /// Return True if able to read next instruction. Otherwise, return False.
    /// </summary>
    public bool ReadNextInstruction()
    {
        if (HasNext())
        {
            ToNext();                 // move iterator to next
            ReadCurrInstruction();    // read instruction sentence which iterator currently points to
            return true;
        }

        Debug.Log(noMoreNextNotice);
        verbalManager_General.Speak(noMoreNextNotice);
        return false;
    }


    /// <summary>
    /// Function reads previous instruction from splited instruction list.
    /// Return True if able to read previous instruction. Otherwise, return False.
    /// </summary>
    public bool ReadPrevInstruction()
    {
        if (HasPrev())
        {
            ToPrev();                 // move iterator to previous
            ReadCurrInstruction();    // read instruction sentence which iterator currently points to
            return true;
        }

        Debug.Log(noMorePrevNotice);
        verbalManager_General.Speak(noMorePrevNotice);
        return false;
    }


    /// <summary>
    /// Function moves iterator to point to the next sentence in the splitted text list
    /// </summary>
    void ToNext()
    {
        iterator++;
    }


    /// <summary>
    /// Function moves iterator to point to the previous sentence in the splitted text list
    /// </summary>
    void ToPrev()
    {
        iterator--;
    }


    /// <summary>
    /// Function checks whether there is next sentence in the splitted text list
    /// </summary>
    bool HasNext()
    {
        return iterator + 1 <= texts.Count - 1;
    }


    /// <summary>
    /// Function checks whether there is previous sentence in the splitted text list
    /// </summary>
    bool HasPrev()
    {
        return iterator - 1 >= 0;
    }


    /// <summary>
    /// Getter of the isRunning boolean value
    /// </summary>
    public bool IsRunning
    {
        get { return isRunning; }
    }


    /// <summary>
    /// Setter for "audiosForInstruction" list.
    /// Shallow copy...
    /// </summary>
    public List<AudioClip> AudiosForInstruction
    {
        set { audiosForInstruction = value; }
    }


    /// <summary>
    /// Setter for "playAudioAt" list.
    /// Shallow copy...
    /// </summary>
    public List<int> PlayAudioAt
    {
        set { playAudioAt = value; }
    }

}

