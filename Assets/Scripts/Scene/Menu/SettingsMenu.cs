using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    Dropdown headRotateModeDrop;                         // The dropdown list that for selecting which mode (Airpod or ARFace) is used to control head rotation in this App
    Slider speechRateSlider;                             // Slider for selecting speech rate
    Text speechRateTextBoard;                            // The text board which show the speech rate to user
    Dropdown measureSystemDrop;                          // The dropdown list that for selecting which measurement system to use in this App
    Toggle useInstructionToggle;                         // Toggle for selecting to use in-game instruction system or not
    Dropdown moveControlModeDrop;

    public static string headControlChoice = "Airpod";   // [Default = "Airpod"] how to control avatar's head rotation
    public static string measureSystem = "US";           // [Valid value: "US" or "Imperial". Default = US] The default measurement system in this App is "US"
    public static bool useInstruction = false;            // [Default = TRUE] indicates whether allow to use instruction system or not
    public static string moveControlChoice = "Touch";

    /* A Dictionary records "speed Thresholds for detecting
     * head nodding". The values are different when user
     * select different value for "headControlChoice" */
    static Dictionary<string, float> nodSpeedDict = new()
    { { "Airpod", 80f }, { "ARFace", 40f } };         

    /* Dynamically provide App speech rate for 
     * different platform. At a same speech rate
     * number, the speaking speed is actually 
     * different on different platform. */
#if UNITY_EDITOR
    public static int appSpeechRate = 70;                // [Default = 70] The default speech rate set by our App on Unity Editor. We need this, otherwise when App start, the speechRate will be set to 70, which is UAP's default value
#endif

#if UNITY_IOS && !UNITY_EDITOR
    public static int appSpeechRate = 60;                // [Default = 60] The default speech rate set by our App on IOS platform.
#endif

    /* Variable shows whether the SettingMenu scene has been activated at least once or not. 
     * If it has been activated at least once ===> TRUE. Otherwise ===> FALSE 
     * It will be used in "HeadRotateSetting.cs" script to print the source of "headControlChoice" ===> it's value is "Default" or "User choice"
     */
    static public bool beenActivated = false;


    /// <summary>
    /// Function called when the Setting Menu starts.
    /// Setting Menu starts everytime when entering the setting menu scene from the main menu
    /// </summary>
    private void Start()
    {
        /* [Note]
        /* When starting Setting Menu scene, the status of toggleGroup will be set to default.
        /* Now the default is: "UseAirpod is checked" and "UseARFace is unchecked" */

        /* once SettingMenu is activated, update the "beenActivated" variable */
        beenActivated = true;

        /* call out which menu it is */
        string welcomeText = "Welcome to Setting Menu";
        GetComponent<VerbalManager_General>().Speak(welcomeText);

        /* Initialize important variables */
        InitVariables();

        /* Load saved values */
        LoadSavedHeadControlChoice();
        LoadSavedSpeechRateChoice();
        LoadSavedMeasureSystemChoice();
        LoadSavedUseInstructionChoice();
        LoadSaveMoveControlChoice();
    }


    /// <summary>
    /// Function enables immediately apply new speech rate when user updates the rate in settingsMenu scene
    /// </summary>
    private void Update()
    {
        /* Modifiy UAP's speech rate if the speech rate in SettingsMenu.cs is changed */
        if (UAP_AccessibilityManager.GetSpeechRate() != appSpeechRate)
            UAP_AccessibilityManager.SetSpeechRate(appSpeechRate);
    }


    /// <summary>
    /// Function initialize member variables of this class as preparation
    /// </summary>
    void InitVariables()
    {
        headRotateModeDrop = GameObject.Find("Canvas/SettingsMenu/HeadRotateModeDropdown/Dropdown").GetComponent<Dropdown>();
        speechRateSlider = GameObject.Find("Canvas/SettingsMenu/SpeechRateSlider/Slider").GetComponent<Slider>();
        speechRateTextBoard = GameObject.Find("Canvas/SettingsMenu/SpeechRateSlider/Board").GetComponent<Text>();
        measureSystemDrop = GameObject.Find("Canvas/SettingsMenu/MeasureSystemDropdown/Dropdown").GetComponent<Dropdown>();
        useInstructionToggle = GameObject.Find("Canvas/SettingsMenu/UseInstructionToggle/Toggle").GetComponent<Toggle>();
        moveControlModeDrop = GameObject.Find("Canvas/SettingsMenu/MovementMode/Dropdown").GetComponent<Dropdown>();
    }


    /// <summary>
    /// Function set the saved "headControlChoice" value to the dropdown list
    /// </summary>
    void LoadSavedHeadControlChoice()
    {
        /* Assign the value */
        List<Dropdown.OptionData> options = headRotateModeDrop.options;
        for (int i = 0; i < options.Count; ++i)
        {
            if (options[i].text == headControlChoice)
            {
                headRotateModeDrop.value = i;
                break;
            }
        }
    }

    void LoadSaveMoveControlChoice()
    {
        moveControlChoice = PlayerPrefs.GetString("MoveControlMode", "Touch");
        List<Dropdown.OptionData> options = moveControlModeDrop.options;
        for(int i = 0; i < options.Count; ++i)
        {
            if(options[i].text == moveControlChoice)
            {
                moveControlModeDrop.value = i;
                break;
            }
        }
    }


    /// <summary>
    /// Function set the saved "appSpeechRate" value to the slider on UI to reflect the previous user's choice
    /// </summary>
    void LoadSavedSpeechRateChoice()
    {
        /* Set the slider to the saved value */
        speechRateSlider.value = appSpeechRate;

        /* Also set the text board to reflect the saved value */
        speechRateTextBoard.text = appSpeechRate.ToString();
    }


    /// <summary>
    /// Function set the saved "MeasureSystem" value to the dropdown list
    /// </summary>
    void LoadSavedMeasureSystemChoice()
    {
        /* Assign the value */
        List<Dropdown.OptionData> options = measureSystemDrop.options;
        for (int i = 0; i < options.Count; ++i)
        {
            if (options[i].text == measureSystem)
            {
                measureSystemDrop.value = i;
                break;
            }
        }
    }


    /// <summary>
    /// Function set the saved "useInstruction" value to the toggle
    /// </summary>
    void LoadSavedUseInstructionChoice()
    {
        if (useInstruction)
            useInstructionToggle.isOn = true;
        else
            useInstructionToggle.isOn = false;
    }


    /// <summary>
    /// Function brings user back to the MainMenu
    /// </summary>
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }


    /// <summary>
    /// Function brings user to the Advanced Settings Menu
    /// </summary>
    public void AvancedSettingsMenu()
    {
        SceneManager.LoadScene("AdvancedSettingsMenu");
    }


    /// <summary>
    /// Function uses the value in the HeadRotateModeDropdown to set the "headControlChoice" value in this class
    /// </summary>
    public void SetHeadControl()
    {
        /* Set the head rotation control choice */
        int choiceIdx = headRotateModeDrop.value;
        string newChoice = headRotateModeDrop.options[choiceIdx].text;
        bool isSame = headControlChoice == newChoice;
        headControlChoice = newChoice;

        /* Change "nodSpeedThreshold" in advancedSettingsMenu based on different head rotation control mode. 
         * Explaination ===> Head rotation speed on x-axis is slower when using ARFace comparing to Airpod. 
         * To make the experience the same when using these two modes, we need to do this adjustment.
         * Only do the update below when the "headControlChoice" is changed to a different value */
        if (!isSame)
            AdvancedSettingsMenu.nodSpeedThreshold = nodSpeedDict[headControlChoice];

        /* Log the message */
        Debug.Log($"User chooses to use {headControlChoice} to control head rotation!");
    }

    public void SetMoveControl()
    {
        int choiceIdx = moveControlModeDrop.value;
        moveControlChoice = moveControlModeDrop.options[choiceIdx].text;
        PlayerPrefs.SetString("MoveControlMode", moveControlChoice);

        /* Log the message */
        Debug.Log($"User chooses to use {moveControlChoice} to control Movement!");
    }


    /// <summary>
    /// Function uses the value on the SpeechRateSlider to set the "appSpeechRate" variable in this class 
    /// </summary>
    public void SetSpeechRate()
    {
        /* Set the speech rate */
        appSpeechRate = (int)speechRateSlider.value;

        /* Update the visualization of speech rate */
        speechRateTextBoard.text = appSpeechRate.ToString();

        /* Log the message */
        Debug.Log($"User chooses to set App Speech Rate to {appSpeechRate}.");
    }


    /// <summary>
    /// Function uses the value in the MeasureSystemDropdown to set the "measureSystem" value in this class
    /// </summary>
    public void SetMeasureSystem()
    {
        /* Set the measurement choice */
        int choiceIdx = measureSystemDrop.value;
        measureSystem = measureSystemDrop.options[choiceIdx].text;

        /* Log the message */
        Debug.Log($"User chooses to use '{measureSystem}' measurement system.");
    }


    /// <summary>
    /// Function uses the value in the UseInstructionToggle to set the "useInstruction" variable in this class
    /// </summary>
    public void SetUseInstruction()
    {
        useInstruction = useInstructionToggle.isOn;
    }


    /// <summary>
    /// Function plays a sample sentence after user changes the UAP speech rate
    /// Will be called as a callback function under the "Accessible Slider"
    /// </summary>
    public void PlaySampleSentence()
    {
        string messge = "Sample Sentence: To do shorelining in this app, you also need to constantly slide your cane from 'right to left' and 'left to right' to identify the change of sliding pattern.";
        GetComponent<VerbalManager_General>().Speak(messge);
    }
}

