using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialMenu : MonoBehaviour
{
    public void Start()
    {
        // call out which menu it is
        string welcomeText = "Welcome to Tutorial Menu";
        GetComponent<VerbalManager_General>().Speak(welcomeText);
    }


    /// <summary>
    /// Function brings user to the interactive question tutorial scene to learn objects
    /// </summary>
    public void ObjectTutorial()
    {
        SceneManager.LoadScene("TutorialPart1_InteractiveQuestionGame");
    }


    /// <summary>
    /// Function brings user to the navigation & exploration tutorial scene
    /// </summary>
    public void NavigationTutorial()
    {
        SceneManager.LoadScene("TutorialPart2_Exploration_NavigationTasks");
    }


    /// <summary>
    /// Function brings user to the way point circle tutorial
    /// </summary>
    public void WayPointCircleTutorial()
    {
        SceneManager.LoadScene("Tutorial_WayPoint_Circle");
    }


    /// <summary>
    /// Function brings user to the way point walk tutorial
    /// </summary>
    public void WayPointWalkTutorial()
    {
        SceneManager.LoadScene("Tutorial_WayPoint_Walk");
    }


    /// <summary>
    /// Function brings user to the shoreline Simple tutorial
    /// </summary>
    public void ShorelineSimpleTutorial()
    {
        SceneManager.LoadScene("Tutorial_Shoreline_Simple");
    }


    /// <summary>
    /// Function brings user to the TrickyScenario training scene
    /// </summary>
    public void PlayTrickyScenario()
    {
        SceneManager.LoadScene("TrickyScenarioWithShoreline");
    }


    /// <summary>
    /// Function brings user to the Head Nod tutorial
    /// </summary>
    public void HeadNodTutorial()
    {
        SceneManager.LoadScene("Tutorial_HeadNod");
    }


    public void PreScavengerHuntShoreline()
    {
        SceneManager.LoadScene("PreScavengerHuntShoreline");
    }


    /// <summary>
    /// Function brings user to the ScavengerHunt training scene
    /// </summary>
    public void PreScavengerHuntTraining()
    {
        SceneManager.LoadScene("PreScavengerHuntTraining");
    }


    /// <summary>
    /// Function brings user to the shoreline Rock tutorial
    /// </summary>
    public void ShorelineRockTutorial()
    {
        SceneManager.LoadScene("Tutorial_Shoreline_Rock");
    }


    /// <summary>
    /// Function brings user to the shoreline Quiz tutorial
    /// </summary>
    public void ShorelineQuizTutorial()
    {
        SceneManager.LoadScene("Tutorial_Shoreline_Quiz");
    }

    /// <summary>
    /// Function brings user to the GestureAndLaser tutorial
    /// </summary>
    public void GestureAndLaserTutorial()
    {
        SceneManager.LoadScene("Tutorial_GestureAndLaser");
    }


    /// <summary>
    /// Function brings user back to the MainMenu
    /// </summary>
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
