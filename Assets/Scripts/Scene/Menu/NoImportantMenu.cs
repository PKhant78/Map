using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NoImportantMenu : MonoBehaviour
{
    public void Start()
    {
        // call out which menu it is
        string welcomeText = "Welcome to NoImportant Menu";
        GetComponent<VerbalManager_General>().Speak(welcomeText);
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
    /// Function brings user back to the MainMenu
    /// </summary>
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Function allows user to quit the game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Function brings user to the Room3 scene
    /// </summary>
    public void PlayMaterialLearning()
    {
        SceneManager.LoadScene("MaterialLearning");
    }

}
