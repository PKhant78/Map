using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickyScenarioProcess : MonoBehaviour
{
    VerbalManager_General verbalManager_General;// For generate the broadcast, part of the UAP plugin.
    NavigationManager navigationManager;
    GameObject WayPoint;
    GameObject WayPoints;
    GameObject[] WayPointArray;
    GameObject CardboardBox;
    GameObject PlantPot;
    GameObject Cube;
    Vector3 prefabPosition;
    GameObject prefab;
    GameObject User;
    int counter = 1;
    bool isUserMove = true;
    Vector3 initialUserPosition;
    void Start()
    {
        WayPoint = GameObject.Find("NavigationSystem/WayPoints/WayPoint");
        WayPoints = GameObject.Find("NavigationSystem/WayPoints");
        CardboardBox = GameObject.Find("CardboardBox");
        PlantPot = GameObject.Find("PlantPot");
        Cube = GameObject.Find("Cube");
        User = GameObject.Find("User");
        WayPointArray = new GameObject[7];
        initialUserPosition = new Vector3(0,0,0);
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        for (int i = 0; i < 7; i++)
        {
            Debug.Log("number: " + i);
            WayPointArray[i] = WayPoints.transform.GetChild(i).gameObject;
        }
        navigationManager = GameObject.Find("NavigationSystem").GetComponent<NavigationManager>();
        navigationManager.RestartNavSystem();
    }

    void Update()
    {
        for (int i = 0; i < 7; i++)
        {
            if (WayPointArray[i].activeSelf == true && counter == 1)
            {
                activateTest(i);
            }
        }

        if (WayPointArray[5].activeSelf == true && counter == 0)
        {
            counter = 1;
        }


        if (counter == 2 && WayPointArray[4].activeSelf == false && WayPointArray[5].activeSelf == false && WayPointArray[6].activeSelf == false)
        {
            string endMessage = "Well done, you passed all tricky test in this room, let's go back to main room.";
            try
            {
                verbalManager_General.SpeakWithoutDisturb(endMessage);
                verbalManager_General.SpeakWaitAndCallback("", () =>
                {
                    SceneJumpHelper.ResetThenSwitchScene("MainMenu");           // do all the resets needed then back to the main menu
                });
            }
            catch (InvalidOperationException e)
            {
                Debug.Log("Error exists: " + e);
            }
            Debug.Log("Tricky sceneario scene done!");
            counter = 3;
        }
    }



    //bool DectectUserMove()
    //{
    //    if(initialUserPosition.x == 0 && initialUserPosition.y == 0 && initialUserPosition.z == 0 )
    //    {
    //        initialUserPosition = User.transform.position;
    //    }
    //    else
    //    {
    //        if (RelativePositionHelper.GetDistanceNum(User.transform.position, initialUserPosition, 1, false) > 1)
    //        {
    //            Debug.Log("Greater than 1");
    //        }
    //        else
    //        {
    //            Debug.Log("Less than 1");
    //        }

    //        initialUserPosition = User.transform.position;
    //    }
    //    return isUserMove;
    //}

    void activateTest(int testnumber)
    {
        if (testnumber == 2)
        {
            prefabPosition = new Vector3(-4.804f, 0.195f, -3.801f);
            prefab = Instantiate(CardboardBox, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab.transform.name = CardboardBox.transform.name.Replace("(clone)", "").Trim();

            prefabPosition = new Vector3(-4.499f, 0.195f, -4.34f);
            prefab = Instantiate(PlantPot, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab.transform.name = PlantPot.transform.name.Replace("(clone)", "").Trim();
            counter--;
            Debug.Log("something happen");
        }

        if (testnumber == 5)
        {
            prefabPosition = new Vector3(-7.77f, 0.195f, -6.74f);
            prefab = Instantiate(Cube, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab.transform.name = Cube.transform.name.Replace("(clone)", "").Trim().ToString() + "1";

            prefabPosition = new Vector3(-6.14f, 0.195f, -6.74f);
            prefab = Instantiate(Cube, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab.transform.name = Cube.transform.name.Replace("(clone)", "").Trim().ToString() + "2"; ;
            counter = 2;
            Debug.Log("something happen");
        }
        if (testnumber == 6)
        {
            counter = 2;
        }

    }
}