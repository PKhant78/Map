using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class GestureAndLaserTutorialProcess : MonoBehaviour
{
    GameObject Cube;
    GameObject User;
    GameObject GripPoint;
    GameObject Cane;
    GameObject Target1;
    GameObject Target2;
    GameObject Target3;
    Vector3 TargetPosition;
    float randnum1;
    float randnum2;
    float randnum3;

    VerbalManager_General verbalManager_General;
    GestureMenu gesturemenu;

    Random rnd = new Random();

    string instructions = "";
    int GestureState = 0;
    float LastTime;
    float PresentTime;
    float OffSetTime = 0;
    float MaxOffSetTime = 15;
    int DialogueOrder = 0;
    int CheckOrder = 0;
    int DialogueCounter = 1;
     void Start()
    {
        Cube = GameObject.Find("Cube");
        User = GameObject.Find("User");
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        gesturemenu = GameObject.Find("User/PortableMenu").GetComponent<GestureMenu>();
        GripPoint = GameObject.Find("User/GripPoint");
        Cane = GameObject.Find("/User/GripPoint/Cane");
        instructions = "Welcome to the tutorial of Laser tool and gesture menu." +
            "In this tutorial, you will learn how to use laser pointer which can give back the  spatial information ";
        //Cane.SetActive(false);
        gesturemenu.MenuClose();
    }

     void Update()
    {
        DetectGesture();
        PresentTime = Time.time;
        OffSetTime = PresentTime - LastTime;

        if (OffSetTime > MaxOffSetTime && CheckOrder < 7)
        {
            Debug.Log("in");
            Dialogue(DialogueOrder);
            LastTime = Time.time;
        }

        GestureCheckProcess();



        if (CheckOrder == 8 )
        {
            instructions = "Congratulations! you successfully found the cube most closed to you, let's jump back to the main menu ";

            if(Vector3.Distance(User.transform.position,Target3.transform.position)< 4f)
            {
                CheckOrder++;
                try
                {
                    verbalManager_General.SpeakWaitAndCallback(instructions, () =>
                    {
                        /* Do all the resets needed then back to the main menu */
                        SceneJumpHelper.ResetThenSwitchScene("MainMenu");
                    });

                }
                catch (InvalidOperationException e)
                {
                    Debug.Log("Error exists: " + e);
                }
            }
            
        }



    }

    void LaserModeCheck()
    {
        //create 3 prefabs
        TargetPosition = new Vector3(8, -0.5f, 0);
        Target1 = Instantiate(Cube, TargetPosition, Quaternion.identity);
        Target1.transform.localScale = new Vector3(2f, 2f, 2f);

        TargetPosition = new Vector3(1, -0.5f, 11);
        Target2 = Instantiate(Cube, TargetPosition, Quaternion.identity);
        Target2.transform.localScale = new Vector3(2f, 2f, 2f);

        TargetPosition = new Vector3(-5, -0.5f, 0);
        Target3 = Instantiate(Cube, TargetPosition, Quaternion.identity);
        Target3.transform.localScale = new Vector3(2f, 2f, 2f);

        Target1.name = "cube1";
        Target2.name = "cube2";
        Target3.name = "cube3";


        try
        {
            verbalManager_General.Speak("In this section, there will be three cubes at your 9 o'clock, 12 o'clock and 3 o'clock. Find the small cubes which has shortest distance with you");
            verbalManager_General.Speak("'Hint:' you can use open menu switch to laser tool which can shoot a laser. It can be extended indefinitely until it touches an object. " +
                "and it can tell you the length between the endpoint of the laser pointer and the object it touches. Then you can switch to cane tool to explore." +
                "Once you find the most close cube, you will be tranfered to main menu.");
        }
        catch (InvalidOperationException e)
        {
            Debug.Log("Error exists when open the Menu: " + e);
        }
        CheckOrder++;
        Debug.Log("CheckOrder: "+ CheckOrder);
    }


    void GestureCheckProcess()
    {
        //Check if user knows how to open menu
        if (CheckOrder == 0)
        {
            if (DialogueCounter == 1)
            {
                Dialogue(DialogueOrder);
                DialogueCounter--;
                DialogueOrder++;
                LastTime = Time.time;
            }

            if (IsMenuOn())
            {
                LastTime = Time.time;
                DialogueCounter = 1;
                DialogueOrder = 2;
                LastTime = Time.time;
                Dialogue(DialogueOrder);
                DialogueOrder++;
                CheckOrder++;
                LastTime = Time.time;
            }
        }

        //Check if user knows how to close menu
        if (CheckOrder == 1)
        {
            if (DialogueCounter == 1)
            {
                Dialogue(DialogueOrder);
                DialogueCounter--;
                DialogueOrder++;
            }

            if (!IsMenuOn())
            {
                CheckOrder++;
                LastTime = Time.time;
                DialogueCounter = 1;
                DialogueOrder = 5;
                Dialogue(DialogueOrder);
                DialogueOrder++;
            }
        }

        if(CheckOrder == 2)
        {
            if (DialogueCounter == 1)
            {
                Dialogue(DialogueOrder);
                DialogueCounter--;
                DialogueOrder++;
            }

           if(IsMenuOn())
            {
                if (GestureState == 2)
                {
                    CheckOrder++;
                    LastTime = Time.time;
                    DialogueCounter = 1;
                    DialogueOrder = 8;
                    Dialogue(DialogueOrder);
                    DialogueOrder++;
                    GestureState = 0;
                }

                if (GestureState == 3 || GestureState == 4)
                {
                    Dialogue(DialogueOrder);
                    GestureState = 0;
                }
            }
        }

        if (CheckOrder == 3)
        {
            if (DialogueCounter == 1)
            {
                Dialogue(DialogueOrder);
                DialogueCounter--;
                DialogueOrder++;
            }

            if (!IsMenuOn())
            {
                CheckOrder++;
                LastTime = Time.time;
                DialogueCounter = 1;
                DialogueOrder = 11;
                Dialogue(DialogueOrder);
                DialogueOrder++;
            }
        }

        if(CheckOrder == 4)
        {
            if (DialogueCounter == 1)
            {
                Dialogue(DialogueOrder);
                DialogueCounter--;
                DialogueOrder++;
            }

            if (IsMenuOn())
            {
                if (GestureState == 4)
                {
                    CheckOrder++;
                    LastTime = Time.time;
                    DialogueCounter = 1;
                    DialogueOrder = 14;
                    Dialogue(DialogueOrder);
                    DialogueOrder++;
                    GestureState = 0;
                }

                if (GestureState == 1 || GestureState == 2)
                {
                    Dialogue(DialogueOrder);
                    GestureState = 0;
                }
            }
        }

        if (CheckOrder == 5)
        {
            Debug.Log("CheckOrder = 5");
            Debug.Log("DialogueCounter: "+ DialogueCounter);
            if (DialogueCounter == 1)
            {
                Dialogue(DialogueOrder);
                DialogueCounter--;
                DialogueOrder++;
            }

            if (!IsMenuOn())
            {
                Debug.Log("menu is close");
                CheckOrder++;
                LastTime = Time.time;
                DialogueCounter = 1;
                DialogueOrder = 17;
                Dialogue(DialogueOrder);
                DialogueOrder++;
            }
        }

        if (CheckOrder == 6)
        {
            Debug.Log("CheckOrder = 6");
            Dialogue(DialogueOrder);
            DialogueOrder++;
            CheckOrder++;
        }

        if (CheckOrder == 7)
        {
            if (DialogueCounter == 1)
            {
                
               // Cane.SetActive(true);
                gesturemenu.MenuOpen();
                DialogueCounter--;
                LaserModeCheck();
            }
        }
    }



    void Dialogue(int DialogueOrder)
    {
        switch (DialogueOrder)
        {
            case 0:
                instructions = "Please try to open the menu by moving the hand holding the cane to the front of your body, and turn your wrist, so that the cane moves upward over the horizontal plane with your wrist as the fulcrum";
                break;
            case 1:
                instructions = "Don't worry, lets do it again. Raise your cane, but keep the cane facing the same direction as your body";
                break;
            case 2:
                instructions = "Great! You successfully open the menu.";
                break;
            case 3:
                instructions = "When you close the menu, the select function will be confirmed, Please try to lower your cane to close the menu.";
                break;
            case 4:
                instructions = "Don't worry, lets do it again. Lower your cane, but keep the cane facing the same direction as your body";
                break;
            case 5:
                instructions = "Great! You successfully close the menu and confirm the function";
                break;
            case 6:
                instructions = "Now let's practice how to move to select the next function. Please try to open the menu and swipe to right and back to the center. And keep menu open";
                break;
            case 7:
                instructions = "Don't worry, lets do it again. Open the menu first, then swipe your cane to right and back to center, don't swipe to left";
                break;
            case 8:
                instructions = "Great! You successfully move to next selection";
                break;
            case 9:
                instructions = "Please try to close the menu to confirm the function";
                break;
            case 10:
                instructions = "Don't worry, lets do it again.";
                break;
            case 11:
                instructions = "Great! You successfully close the menu and confirm the function";
                break;
            case 12:
                instructions = "Now let's practice how to move to select the previous function. Please try to open the menu and swipe to left and back to the center. And keep menu open";
                break;
            case 13:
                instructions = "Don't worry, lets do it again. Open the menu first, then swipe your cane to left and back to center, don't swipe to right";
                break;
            case 14:
                instructions = "Great! You successfully move to previous selection";
                break;
            case 15:
                instructions = "Please try to close the menu to confirm the function.";
                break;
            case 16:
                instructions = "Don't worry, let's do it again.";
                break;
            case 17:
                instructions = "Great! You successfully close the menu and confirm the function";
                break;
            case 18:
                instructions = "Congratulations, you successfully passed the gesture menu tutorials, lets jump to complex training";
                break;
        }
        try
        {
            verbalManager_General.StopSpeak();
            verbalManager_General.Speak(instructions);
        }
        catch (InvalidOperationException e)
        {
            Debug.Log("Error exists when open the Menu: " + e);
        }
    }


    protected bool IsMenuOn()
    {
        return GripPoint.transform.eulerAngles.x >= 270 && GripPoint.transform.eulerAngles.x < 350;
    }


    private void DetectGesture()
    {
        if (GestureState == 0 && GripPoint.transform.eulerAngles.y > 15 && GripPoint.transform.eulerAngles.y < 90)
        {
            Debug.Log("GestureState = 1");
            GestureState = 1;
        }
        if (GestureState == 1 && (GripPoint.transform.eulerAngles.y <= 10 || GripPoint.transform.eulerAngles.y > 350))
        {
            Debug.Log("GestureState = 2");
            GestureState = 2;
        }

        if (GestureState == 0 && GripPoint.transform.eulerAngles.y > 270 && GripPoint.transform.eulerAngles.y < 345)
        {
            Debug.Log("GestureState = 3");
            GestureState = 3;
        }
        if (GestureState == 3 && (GripPoint.transform.eulerAngles.y <= 10 || GripPoint.transform.eulerAngles.y > 350))
        {
            Debug.Log("GestureState = 4");
            GestureState = 4;
        }
    }

    float Difference(float num1, float num2)
    {
        float difference = Mathf.Abs(num1 - num2);
        return difference;
    }

    IEnumerator WaitForEnd()
    {
        yield return new WaitForSeconds(2);
        instructions = "Congratulations! you successfully found the cube most closed to you, let's jump back to the main menu ";
        //verbalManager_General.SpeakWaitAndCallback(instructions, () => { SceneJumpHelper.ResetThenSwitchScene("MainMenu"); });
       
    }



}
