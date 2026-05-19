using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class AdvancedSettingsMenu : MonoBehaviour
{
    Toggle airpodEnhanceToggle;                                 // Toggle for selecting doing airpod control enhancement or not
    Slider rotationsNearAngleSlider;                            // Slider for selecting "rotations are near" threshold angle
    Text rotationsNearAngleTextBoard;                           // The text board which show "rotations are near" threshold angle to user

    Toggle nodInfoRetrieverToggle;                              // Toggle for selecting if the system allow using "head nod info retriever" or not
    Dropdown headNodMethodDrop;                                 // The dropdown which for selecting which method to use for head nodding detection in this App
    Slider nodSpeedThresholdSlider;                             // Slider for selecting "Head nod speed" threshold
    Text nodSpeedThresholdTextBoard;                            // The text board which "Head nod speed" threshold to user
    Slider nodFramesThresholdSlider;                            // Slider for selecting "Head nod frames" threshold
    Text nodFramesThresholdTextBoard;                           // The text board which "Head nod frames" threshold to user
    Slider nodWaitTimeThresholdSlider;                          // Slider for selecting "Head nod wait time" threshold
    Text nodWaitTimeThresholdTextBoard;                         // The text board which "Head nod wait time" threshold to user

    static public bool doAirpodControlEnhance = false;          // [Default = FALSE] If we should use data from ARFace to enhance the headphone control 
    static public float rotationsNearAngle = 15f;               // [Default = 15f] If the x,y,z of an euler rotation is within "rotIsDiffAngleThreshold" distance from another euler rotation ===> We say these two rotations are near ===> Will be used to modify the "HeadphoneTrackEnhancer.cs" class
    static public bool allowNodInfoRetriever = true;            // [Default = TRUE] If we allow user to use "head nod information retriever or not"
    static public string nodDetectMethod = "Speed";             // [Valid value: "Speed" or "Threshold". Default = Speed] The default method for checking head-nodding in this App is "Speed"
    static public float nodSpeedThreshold = 80f;                // [Default = 80f] A speed threshold, above which we start to check user's potential head nodding motion. Please see the "HeadNodDetector.cs" class for more details
    static public int nodFramesThreshold = 2;                   // [Default = 2] A frames threshold, above which we detect the users are lowering their head or raising their head for head nodding motion
    static public float nodWaitTimeThreshold = 0.5f;            // [Default = 0.5f] A time threshold, the maximum time we will wait for the user to raise their head after user complete the lowering head motion

    /* Variable shows whether the AdvancedSettingMenu scene has been activated at least once or not. 
     * If it has been activated at least once ===> TRUE. Otherwise ===> FALSE 
     */
    static public bool beenActivated = false;


    /// <summary>
    /// Function called when the Setting Menu starts.
    /// Setting Menu starts everytime when entering the setting menu scene from the main menu
    /// </summary>
    private void Start()
    {
        /* Once SettingMenu is activated, update the "beenActivated" variable */
        beenActivated = true;

        /* Call out which menu it is */
        string welcomeText = "Welcome to Advanced Setting Menu";
        GetComponent<VerbalManager_General>().Speak(welcomeText);

        /* Initialize important variables */
        InitVariables();

        /* Load the saved values to UI */
        LoadSavedAirpodEnhanceChoice();
        LoadSavedRotationsNearAngle();
        LoadSavedAllowNodInfoRetriever();
        LoadSavedHeadNodMethodChoice();
        LoadSavedNodSpeedThreshold();
        LoadSavedNodFramesThreshold();
        LoadSavedNodWaitTimeThreshold();
    }


    /// <summary>
    /// Function initialize member variables of this class as preparation
    /// </summary>
    void InitVariables()
    {
        airpodEnhanceToggle = GameObject.Find("AirpodEnhanceToggle").transform.Find("Toggle").GetComponent<Toggle>();
        rotationsNearAngleSlider = GameObject.Find("RotationsNearSlider").transform.Find("Slider").GetComponent<Slider>();
        rotationsNearAngleTextBoard = GameObject.Find("RotationsNearSlider").transform.Find("Board").GetComponent<Text>();

        nodInfoRetrieverToggle = GameObject.Find("AllowNodInfoRetrieverToggle").transform.Find("Toggle").GetComponent<Toggle>();
        headNodMethodDrop = GameObject.Find("HeadNodMethodDropdown").transform.Find("Dropdown").GetComponent<Dropdown>();
        nodSpeedThresholdSlider = GameObject.Find("NodSpeedThresholdSlider").transform.Find("Slider").GetComponent<Slider>();
        nodSpeedThresholdTextBoard = GameObject.Find("NodSpeedThresholdSlider").transform.Find("Board").GetComponent<Text>();
        nodFramesThresholdSlider = GameObject.Find("NodFramesThresholdSlider").transform.Find("Slider").GetComponent<Slider>();
        nodFramesThresholdTextBoard = GameObject.Find("NodFramesThresholdSlider").transform.Find("Board").GetComponent<Text>();
        nodWaitTimeThresholdSlider = GameObject.Find("NodWaitTimeThresholdSlider").transform.Find("Slider").GetComponent<Slider>();
        nodWaitTimeThresholdTextBoard = GameObject.Find("NodWaitTimeThresholdSlider").transform.Find("Board").GetComponent<Text>();
    }


    /// <summary>
    /// Function set the saved "doAirpodControlEnhance" value to the toggle to reflect the previous user's choice
    /// </summary>
    void LoadSavedAirpodEnhanceChoice()
    {
        if (doAirpodControlEnhance)
            airpodEnhanceToggle.isOn = true;
        else
            airpodEnhanceToggle.isOn = false;
    }


    /// <summary>
    /// Function set the save "rotationsNearAngle" to slider and it's text board
    /// </summary>
    void LoadSavedRotationsNearAngle()
    {
        /* Set the slider to the saved value */
        rotationsNearAngleSlider.value = rotationsNearAngle;

        /* Also set the text board to reflect the saved value */
        rotationsNearAngleTextBoard.text = rotationsNearAngle.ToString();
    }


    /// <summary>
    /// Function set the saved "allowNodInfoRetriever" value to the toggle to reflect the previous user's choice
    /// </summary>
    void LoadSavedAllowNodInfoRetriever()
    {
        if (allowNodInfoRetriever)
            nodInfoRetrieverToggle.isOn = true;
        else
            nodInfoRetrieverToggle.isOn = false;
    }


    /// <summary>
    /// Function set the saved "headNodMethod" value to the dropdown list
    /// </summary>
    void LoadSavedHeadNodMethodChoice()
    {
        /* Assign the value */
        List<Dropdown.OptionData> options =  headNodMethodDrop.options;
        for (int i = 0; i < options.Count; ++i)
        {
            if (options[i].text == nodDetectMethod)
            {
                headNodMethodDrop.value = i;
                break;
            }
        }
    }


    /// <summary>
    /// Function set the save "nodSpeedThreshold" to slider and it's text board
    /// </summary>
    void LoadSavedNodSpeedThreshold()
    {
        /* Set the slider to the saved value */
        nodSpeedThresholdSlider.value = nodSpeedThreshold;

        /* Also set the text board to reflect the saved value */
        nodSpeedThresholdTextBoard.text = nodSpeedThreshold.ToString();
    }


    /// <summary>
    /// Function set the save "nodFramesThreshold" to slider and it's text board
    /// </summary>
    void LoadSavedNodFramesThreshold()
    {
        /* Set the slider to the saved value */
        nodFramesThresholdSlider.value = nodFramesThreshold;

        /* Also set the text board to reflect the saved value */
        nodFramesThresholdTextBoard.text = nodFramesThreshold.ToString();
    }


    /// <summary>
    /// Function set the save "nodWaitTimeThreshold" to slider and it's text board
    /// </summary>
    void LoadSavedNodWaitTimeThreshold()
    {
        /* Set the slider to the saved value */
        nodWaitTimeThresholdSlider.value = nodWaitTimeThreshold;

        /* Also set the text board to reflect the saved value */
        nodWaitTimeThresholdTextBoard.text = nodWaitTimeThreshold.ToString();
    }


    /// <summary>
    /// Function brings user back to the SettingsMenu
    /// </summary>
    public void SettingsMenu()
    {
        SceneManager.LoadScene("SettingsMenu");
    }


    /// <summary>
    /// Function set value to "doAirpodControlEnhance" boolean variable
    /// </summary>
    public void SetAirpodEnhance()
    {
        doAirpodControlEnhance = airpodEnhanceToggle.isOn;
    }


    /// <summary>
    /// Function set value to "rotationNearAngle" variable
    /// </summary>
    public void SetRotationsNearAngle()
    {
        /* Set the "ratations are near" angle */
        rotationsNearAngle = rotationsNearAngleSlider.value;

        /* Update the visualization of the "ratations are near" angle */
        rotationsNearAngleTextBoard.text = rotationsNearAngle.ToString();
    }


    /// <summary>
    /// Function set value to "allowNodInfoRetriever" boolean variable
    /// </summary>
    public void SetAllowNodInfoRetriever()
    {
        allowNodInfoRetriever = nodInfoRetrieverToggle.isOn;
    }


    /// <summary>
    /// Function uses the value in the HeadNodMethodDropdown to set the "headNodMethod" value in this class
    /// </summary>
    public void SetHeadNodMethod()
    {
        /* Set the measurement choice */
        int choiceIdx = headNodMethodDrop.value;
        nodDetectMethod = headNodMethodDrop.options[choiceIdx].text;
    }


    /// <summary>
    /// Function sets value to "nodSpeedThreshold" variable
    /// </summary>
    public void SetNodSpeedThreshold()
    {
        /* Set the value for nodSpeedThreshold */
        nodSpeedThreshold = nodSpeedThresholdSlider.value;

        /* Update the visualization of the nodSpeedThreshold */
        nodSpeedThresholdTextBoard.text = nodSpeedThreshold.ToString();
    }


    /// <summary>
    /// Function sets value to "nodFramesThreshold" variable
    /// </summary>
    public void SetNodFramesThreshold()
    {
        /* Set the value for nodFramesThreshold */
        nodFramesThreshold = (int) nodFramesThresholdSlider.value;

        /* Update the visualization of the nodSpeedThreshold */
        nodFramesThresholdTextBoard.text = nodFramesThreshold.ToString();
    }


    /// <summary>
    /// Function sets value to "nodWaitTimeThreshold" variable
    /// </summary>
    public void SetNodWaitTimeThreshold()
    {
        /* Set the value for nodWaitTimeThreshold */
        nodWaitTimeThreshold = Mathf.Round(nodWaitTimeThresholdSlider.value * 10) / 10;

        /* Update the visualization of the nodWaitTimeThreshold */
        nodWaitTimeThresholdTextBoard.text = nodWaitTimeThreshold.ToString();
    }

}

