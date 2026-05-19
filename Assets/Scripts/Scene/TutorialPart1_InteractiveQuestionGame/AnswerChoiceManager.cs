using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswerChoiceManager : MonoBehaviour
{
    /* Get the QuestionSceneProcess class to call function for making choice */
    QuestionSceneProcess questionSceneProcess;


    private void Start()
    {
        /* Initialize the QuestionSceneProcess */
        questionSceneProcess = GameObject.Find("SceneManager").GetComponent<QuestionSceneProcess>();

    }


    /// <summary>
    /// The action (Making choice for question) the button takes after hitted by user's cane
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        Transform other = collision.collider.transform;                                         // the collider object which the button collides with

        if (other.name == "Cane")                                                               // if the button hitted by cane
        {
            if (AccessColliderInfo.GetRootName(transform) == "RightQuestionOption")             // user hit right option
            {
                questionSceneProcess.ChooseRightOption();                                       // do the things which needed when user choose right option
            }
            else if (AccessColliderInfo.GetRootName(transform) == "LeftQuestionOption")         // user hit left button
            {
                questionSceneProcess.ChooseLeftOption();                                        // do the things which needed when user choose left option
            }
        }
    }


    /// <summary>
    /// The action (Refresh question) the button takes after cane leave the button
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        Transform other = collision.collider.transform;            // the collider object which the button collides with

        if (other.name == "Cane")                                  // if the button hitted by cane
        {
            questionSceneProcess.RefreshQuestion();                // refresh the question after user's cane moves away from option button
        }
    }
}
