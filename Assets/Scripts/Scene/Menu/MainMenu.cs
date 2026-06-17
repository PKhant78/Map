using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {
        // call out which menu it is
        string welcomeText = "Welcome to Main Menu";
        GetComponent<VerbalManager_General>().Speak(welcomeText);
    }


    /// <summary>
    /// Function brings user back to the SettingsMenu
    /// </summary>
    public void SettingsMenu()
    {
        SceneManager.LoadScene("SettingsMenu");
    }


    /// <summary>
    /// Function brings user to the tutorialMenu
    /// </summary>
    public void TutorialMenu()
    {
        SceneManager.LoadScene("TutorialMenu");
    }


    /// <summary>
    /// Function brings user to the NoImportantMenu
    /// </summary>
    public void NoImportantMenu()
    {
        SceneManager.LoadScene("NoImportantMenu");
    }


    /// <summary>
    /// Function brings user to the Room1 scene
    /// </summary>
    public void PlayRoom1()
    {
        SceneManager.LoadScene("ExplorationPart");
    }


    /// <summary>
    /// Function brings user to the Room2 scene
    /// </summary>
    public void PlayRoom2()
    {
        SceneManager.LoadScene("ExplorationPartV2");
    }


    /// <summary>
    /// Function brings user to the Room3 scene
    /// </summary>
    public void PlayRoom3()
    {
        SceneManager.LoadScene("ExplorationPartV3");
    }


    /// <summary>
    /// Function brings user to the FormalExperiment scene
    /// </summary>
    public void PlayFormalExperiment()
    {
        SceneManager.LoadScene("FormalExperiment");
    }


    /// <summary>
    /// Function brings user to the ScavengerHunt scene
    /// </summary>
    public void PlayScavengerHunt()
    {
        SceneManager.LoadScene("ScavengerHunt");
    }


    /// <summary>
    /// Function brings user to the StreetExploration scene
    /// </summary>
    public void PlayStreetExploration()
    {
        SceneManager.LoadScene("StreetExploration");
    }


    /// <summary>
    /// Function brings user to the Room3 scene
    /// </summary>
    public void PlayMaterialLearning()
    {
        SceneManager.LoadScene("MaterialLearning");
    }

    public void PlayLayoutLearning()
    {
        SceneManager.LoadScene("LayoutLearning");
    }

    public void PlayTouchMethod()
    {
        SceneManager.LoadScene("FormalExperimentTouch");
    }

    public void PlaySwingMethod()
    {
        SceneManager.LoadScene("FormalExperimentSwing");
    }


    /// <summary>
    /// Function allows user to quit the game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
