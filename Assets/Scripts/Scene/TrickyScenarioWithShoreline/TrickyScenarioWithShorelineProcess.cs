using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickyScenarioWithShorelineProcess : MonoBehaviour
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
    GameObject prefab1;
    GameObject prefab2;
    GameObject User;
    int counter = 1;
    bool isUserMove = true;
    Vector3 initialUserPosition;
    List<string> objDoNotCallName;
    void Start()
    {
        WayPoint = GameObject.Find("NavigationSystem/WayPoints/WayPoint");
        WayPoints = GameObject.Find("NavigationSystem/WayPoints");
        CardboardBox = GameObject.Find("CardboardBox");
        PlantPot = GameObject.Find("PlantPot");
        Cube = GameObject.Find("Cube");
        User = GameObject.Find("User");
        WayPointArray = new GameObject[7];
        initialUserPosition = new Vector3(0, 0, 0);
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        navigationManager = GameObject.Find("NavigationSystem").GetComponent<NavigationManager>();
        objDoNotCallName = GameObject.Find("Cane").GetComponent<CaneContact>().objDoNotCallName;
        navigationManager.RestartNavSystem();
        activateTest(0);
    }

    void Update()
    {
        if (WayPoint.activeSelf == false && counter == 1)
        {
            Debug.Log("waypoint destroied");
            CallObjName(false);
            String Message = "The box do not near the wall has been destroied. Please go to the left and walk around the room";
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
            activateTest(1);
        }
        Debug.Log("prefab1 position: " + prefab1.transform.position);
        Debug.Log("prefab2 position: " + prefab2.transform.position);
    }

    void activateTest(int testnumber)
    {
        if (testnumber == 0)
        {
            prefabPosition = new Vector3(8.63f, 0f, 6.5f);
            prefab1 = Instantiate(Cube, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab1.transform.name = "box1";
            prefabPosition = new Vector3(7f, 0f, 6.5f);
            prefab2 = Instantiate(Cube, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab2.transform.localScale = new Vector3(1, 1, 0.5f);
            prefab2.transform.name = "box2";
            Debug.Log("prefab1 position: "+ prefab1.transform.position);
            Debug.Log("prefab2 position: " + prefab2.transform.position);
        }

        if (testnumber == 1)
        {
            Destroy(prefab2, 0);
            prefabPosition = new Vector3(5.6f, 0f, 3.811f);
            prefab1 = Instantiate(CardboardBox, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab1.transform.name = CardboardBox.transform.name.Replace("(clone)", "").Trim();
            prefabPosition = new Vector3(4.645f, 0f, 5f);
            prefab2 = Instantiate(PlantPot, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab2.transform.name = PlantPot.transform.name.Replace("(clone)", "").Trim();
            counter--;
        }


    }

    void CallObjName(bool state)
    {
        if (state == false)
        {
            objDoNotCallName.Add("Cube");
            objDoNotCallName.Add("box1");
            objDoNotCallName.Add("Room");
            objDoNotCallName.Add("Floor");
            objDoNotCallName.Add("Wall1");
            objDoNotCallName.Add("Wall2");
            objDoNotCallName.Add("Wall3");
            objDoNotCallName.Add("Wall4");
            objDoNotCallName.Add("PlantPot");
            objDoNotCallName.Add("CardboardBox");
        }
        else
        {
            objDoNotCallName.Remove("Cube");
            objDoNotCallName.Remove("box1");
            objDoNotCallName.Remove("Room");
            objDoNotCallName.Remove("Floor");
            objDoNotCallName.Remove("Wall1");
            objDoNotCallName.Remove("Wall2");
            objDoNotCallName.Remove("Wall3");
            objDoNotCallName.Remove("Wall4");
            objDoNotCallName.Remove("PlantPot");
            objDoNotCallName.Remove("CardboardBox");
        }
    }

}
