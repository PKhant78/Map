using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class QuestionSceneProcess : MonoBehaviour
{
    GameObject prefab;          //The prefab it will generate in the scene
    GameObject SecondPrefab;    //The second prefab it will generate in the scene(For specific question)
    Vector3 prefabPosition;     //The position of generated prefabs
    GameObject[] prefabs;       //The array of prefabs, it will contain all prefabs involve with the scenes
    Random rnd = new Random();  //The random function use for question part of the scene
    int QuestionNum = 0;        //Number of question
    float randNum1;             //Random number1 which generate by the random function
    float randNum2;             //Random number2 which generate by the random function
    int userAnswer = 0;         //It represents user's cane hit which panel,
                                //If hit the left panel userAnswer= 1,if hit the right panel userAnswer= 2
    int correctAnswer = 0;      //Answer of the question, it based on the random number.
                                //If an userAnswer = correctAnswer jump to next question, otherwise repeat question.
    int introductionIndex = 0;  //The index of prefabs which will be introudced to user.
    int RightCounter = 0;       //The number of user hit the right panel,
                                //When RightCounter = 1 it will broadcast the content of the option
                                //When RightCounter = 2 it will confirm user's option
    int LeftCounter = 0;        //Similar to the RightCounter

    VerbalManager_General verbalManager_General;// For generate the broadcast, part of the UAP plugin.
    GestureMenu gesturemenu;

    bool requestedSceneJump = false;   // boolean variable indicates whether a scene jump is requested or not. A scene jump happens at the end of this tutorial session
    List<string> objDoNotCallName;

    void Start()
    {
        /* A list to store all the prefabs for generating later */
        prefabs = new GameObject[2];

        //speakerManager = navigationManager.transform.root.GetComponent<NavSpeakerManager>();
        // speakerManager = GameObject.Find("NavigationSystem/NavSpeaker").GetComponent<NavSpeakerManager>();
        gesturemenu = GameObject.Find("User/PortableMenu").GetComponent<GestureMenu>();


        /* An empty gameObject in the scene which stores all the prefab gameObjects */
        GameObject prefabRepo = GameObject.Find("PrefabRepository");

        /* Adding prefabs from repository to the "prefabs" list in the script */
        for (int i = 0; i < prefabs.Length; ++i)
            prefabs[i] = prefabRepo.transform.GetChild(i).gameObject;

        /* Add code for generate inner part for each object
         * so there will be alert when user is inside of an
         * object */
        foreach (GameObject prefab in prefabs)
        {
            /* 1. Add code for generate inner part for each object
                * so there will be alert when user is inside of an
                * object */
            prefab.AddComponent<GenerateEmbeddedObject>();


            /* 2. Adding the name of these objects to "objDoNotCallName"
             * list on cane. So that, the cane won't callout their
             * name when hitting them */
            objDoNotCallName = GameObject.Find("Cane").GetComponent<CaneContact>().objDoNotCallName;
            //objDoNotCallName.Remove("Floor");
        }

        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();


        try
        {
            verbalManager_General.Speak("Welcome to the virtual objects learning tutorial of the ‘MR CANE’ project ");
            //verbalManager_General.Speak("In this project. your iPhone device will be placed on a selfie stick. which should be kept in a shortened position, and held without twisting your wrist. ");
            //verbalManager_General.Speak(
            //    "This tutorial includes 3 sections in total. including “using the panel to switch to the different section of the tutorial”." +
            //    " Interaction feedback between virtual cane and virtual objects. and “the feedback from different virtual objects. Now let the first section start.");

        }
        catch (InvalidOperationException e)
        {
            Debug.Log("Error exists: " + e);
        }

        /* Start with picking one question */
        LearnMaterials(introductionIndex);
        gesturemenu.MenuClose();
    }





    /*LearnMaterials will broadcase the introduction of the prefab and environment in this scene*/
    /*The purpose of this function is to help user to get familiar with this scene*/
    /*The value of the introductionIndex will change when user hit the left panel or right panel*/
    /*When user hit the left panel, the introductionIndex will decrease by 1*/
    /*When user hit the right panel, the introductionIndex will increase by 1*/
    void LearnMaterials(int introductionIndex)
    {
        if (introductionIndex == 0)
        {
            try
            {
                verbalManager_General.Speak("Welcome to section 1. In this tutorial, there will be two panels on your left and right side.");
                //verbalManager_General.Speak("After you reach the current target. you can use your cane to hit the right panels to switch to the next question. and hit the left to go back to the previous question.");
                verbalManager_General.Speak("After you reach the current target. you can use your phone as a cane to hit the right panels to switch to the next question. and hit the left to go back to the previous question.");
                verbalManager_General.Speak("When you first hit the panel. you will hear the choice represent by the panel and feel the vibration. " +
                                            "If you hit the same panel twice. it will confirm your choice.");
                verbalManager_General.Speak("Now try to use your cane to hit the panel. go to the next section");


            }
            catch (InvalidOperationException e)
            {
                Debug.Log("Error exists: " + e);
            }

        }

        if (introductionIndex == 1)
        {
            //string Message = "Well done. Welcome to section 2. In this section. you will learn the feedback when you use your virtual cane to hit the virtual object." +
            //    "Now you are holding the short self-stick in the real world. but it will become the long cane in the virtual world." +
            //    "you can use this virtual cane to hit different virtual objects to get similar sound feedback as in the real world." +
            //    "Now you are stand in the floor. try to use your cane to hit the floor to get the feedback." +
            //    "If you feel 'familiar' enough for the feedback. please hit the right panel to go to visit the new virtual object.";

            string Message = "Well done. Welcome to section 2. In this section. you will learn the feedback when you use your virtual cane to hit the virtual object." +
    "Now you are holding your phone in the real world. but it will become the long cane in the virtual world." +
    "you can use this virtual cane to hit different virtual objects to get similar sound feedback as in the real world." +
    "please hit the right panel to go to visit the new virtual object.";
            try
            {
                verbalManager_General.SpeakWaitAndCallback(Message, () =>
                {
                    CallObjName(true);
                });
            }
            catch (InvalidOperationException e)
            {
                Debug.Log("Error exists: " + e);
            }
        }

        if (introductionIndex == 2)
        {
            CallObjName(false);
            string Message = "Now, let’s try to learn the feedback when your virtual cane hit other virtual object." +
                "There is a virtual cardboard box with a length, width and height of one meter each placed on the floor in front of you." +
                "When you touch the virtual object with the cane, you will hear the sound of the virtual cane touching the virtual object." +
                "the name of the virtual object and feel the vibration." +
                "Please try to use your cane to hit the surface of the virtual card box." +
                "If you feel familiar enough for the feedback. please hit the right panel to go to visit the new virtual object." +
                "or left panel to visit the previous new virtual object.";
            try
            {
                verbalManager_General.SpeakWaitAndCallback(Message, () =>
                {
                    CallObjName(true);
                });
            }
            catch (InvalidOperationException e)
            {
                Debug.Log("Error exists: " + e);
            }

        }

        if (introductionIndex == 3)
        {
            CallObjName(false);
            string Message = "Cardboard box has been destroied." +
                "There is a new virtual plastic chair with a length, width and height of one meter each placed on the floor in front of you." +
                "Please try to use your cane to hit the surface of the virtual plastic plastic chair." +
                "If you feel familiar enough for the feedback. please hit the right panel to end this tutorial";
            try
            {
                verbalManager_General.SpeakWaitAndCallback(Message, () =>
                {
                    CallObjName(true);
                });
            }
            catch (InvalidOperationException e)
            {
                Debug.Log("Error exists: " + e);
            }
        }

        if (introductionIndex == 4)
        {
            CallObjName(false);
            string endMessage = "Excellent! now you finished the virtual object learning tutorial, let’s go back to the main menu.";
            verbalManager_General.SpeakWaitAndCallback(endMessage, () =>
            {
                CallObjName(true);
                SceneJumpHelper.ResetThenSwitchScene("MainMenu");           // do all the resets needed then back to the main menu
            });
        }
    }


    void Update()
    {
        /* Function check if criteria meet ===> if meet, end this tutorial 1 and jump to tutorial 2 */
        //CheckToEndTutorial1();
    }

    void CallObjName(bool state)
    {
        if(state == false)
        {
            objDoNotCallName.Add("Floor");
            objDoNotCallName.Add("CardboardBox");
            objDoNotCallName.Add("TrashCan");
        }
        else
        {
            objDoNotCallName.Remove("Floor");
            objDoNotCallName.Remove("CardboardBox");
            objDoNotCallName.Remove("TrashCan");
        }
    }




    /// <summary>
    /// Function check if "ending criteria" meet or not. If meet end this tutorial section
    /// </summary>
    void CheckToEndTutorial1()
    {
        /*The total amount of questions are 4, every time user answer correct answer, the QuestionNum will increase by 1*/
        /*If QuestionNum >= 4 it will jump to next tutorial scene*/
        if (QuestionNum >= 4 && !requestedSceneJump)
        {
            /* Update the sceneJump bool var immediately to prevent taking these ending actions again */
            requestedSceneJump = true;

            /* The ending message */
            string endMessage = "Congraulation, you passed all the questions! Please turn your face and cane to the front of your body. Let's jump to next tutorial to learn more about way points.";

            /* Convey the ending message and jump to the next scene*/
            //verbalManager_General.Speak(endMessage);
            Debug.Log(endMessage);
            verbalManager_General.SpeakWaitAndCallback(endMessage, () => {

                SceneJumpHelper.ResetThenSwitchScene("Tutorial_WayPoint_Circle");

            });
        }
    }






    /* Things to do after user choosing the Right Option. 
     * The function will be called in "AnswerChoiceManager.cs" */
    public void ChooseRightOption()
    {
        /*Every time, user hit the right panel, the RightCounter will increate by 1*/
        RightCounter++;
        /*If user hit the right panel, the LeftCounter will be reset*/
        if (RightCounter == 1)
        {
            LeftCounter = 0;

            if (introductionIndex != 5)
                verbalManager_General.Speak("You choose the right option, move out the cane and hit again to confirm you choice.");

            //If user are checking the fifth prefab which is last one, if hit right panel again, it will broadcast the special message.
            if (introductionIndex == 5)
                verbalManager_General.Speak("You choose the right option, it's already the last object, do you want to take a test to verify the learning result?" +
                    "if yes please, move out the cane and hit again to confirm you choice.");
        }

        /*If user hit right panel twice, the option confirmed ,compare with the correct answer*/
        if (RightCounter >= 2)
        {
            userAnswer = 2;
            verbalManager_General.Speak("Right option confirmed");
        }

    }


    /* Things to do after user choosing the Left Option. 
     * The function will be called in "AnswerChoiceManager.cs" */
    public void ChooseLeftOption()
    {
        /*Every time, user hit the left panel, the LeftCounter will increate by 1*/
        LeftCounter++;

        /*If user hit the left panel, the RightCounter will be reset*/
        if (LeftCounter == 1)
        {
            RightCounter = 0;

            if (introductionIndex > 0)
                verbalManager_General.Speak("You choose the left option, move out the cane and hit again to confirm you choice.");

            if (introductionIndex == 0)
                verbalManager_General.Speak("You choose the left option, it's already the first object");
        }

        if (LeftCounter >= 2)
        {
            verbalManager_General.Speak("Left option confirmed");
            userAnswer = 1;
        }
    }


    /* Refresh the question after user make their choice */
    public void RefreshQuestion()
    {
        if (LeftCounter == 2 || RightCounter == 2)
        {
            if (introductionIndex <= 4)
            {
                Destroy(prefab, 0);
                checkAnswer(userAnswer);
                LeftCounter = 0;
                RightCounter = 0;
                userAnswer = 0;
                LearnMaterials(introductionIndex);
            }
        }
    }


    void checkAnswer(int UserAnswer)
    {
        Debug.Log("introductionIndex: " + introductionIndex);
        /*When user hit the left panel*/
        if (userAnswer == 1 && introductionIndex == 1)
        {
            introductionIndex -= 1;
        }

          if (userAnswer == 1 && introductionIndex != 0 && introductionIndex != 1 && introductionIndex != 2)
         {
            introductionIndex -= 1;
            Destroy(prefab, 0);
            prefabPosition = new Vector3(0f, 0.5f, 1.65f);
            prefab = Instantiate(prefabs[introductionIndex - 2], prefabPosition, Quaternion.Euler(0, 90, 0));
        }
          if(userAnswer == 1 && (introductionIndex == 2 || introductionIndex == 1))
        {
            introductionIndex -= 1;
        }

          if(userAnswer == 2 && introductionIndex < 4)
        {
            introductionIndex += 1;
        }

            /*When user hit the right panel*/
          if (userAnswer == 2 && introductionIndex < 4 && introductionIndex>=2)
          {
            Destroy(prefab, 0);
            prefabPosition = new Vector3(0f, 0.5f, 1.4f);
            prefab = Instantiate(prefabs[introductionIndex - 2], prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab.transform.name = prefabs[introductionIndex - 2].transform.name.Replace("(clone)", "").Trim();
        }



    }
}






