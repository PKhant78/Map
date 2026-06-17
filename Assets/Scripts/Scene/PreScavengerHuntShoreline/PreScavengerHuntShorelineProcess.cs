using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreScavengerHuntShorelineProcess : MonoBehaviour
{
    VerbalManager_General verbalManager_General;// For generate the broadcast, part of the UAP plugin.
    NavigationManager navigationManager;
    GameObject WayPoint;
    GameObject WayPoints;
    GameObject[] WayPointArray;
    GameObject GripPoint;
    Vector3 prefabPosition;
    GameObject prefab;
    int counter = 1;
    List<string> objDoNotCallName;


    void Start()
    {
        WayPoint = GameObject.Find("NavigationSystem/WayPoints/WayPoint");
        WayPoints = GameObject.Find("NavigationSystem/WayPoints");
        GripPoint = GameObject.Find("/User/GripPoint");
        WayPointArray = new GameObject[2];
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        objDoNotCallName = GameObject.Find("Cane").GetComponent<CaneContact>().objDoNotCallName;
        for (int i = 0; i < 2; i++)
        {
            WayPointArray[i] = WayPoints.transform.GetChild(i).gameObject;
        }
        navigationManager = GameObject.Find("NavigationSystem").GetComponent<NavigationManager>();
        navigationManager.RestartNavSystem();
    }


    private void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            if (WayPointArray[i].activeSelf == true && counter == 1)
            {
                activateTest(i);
            }
        }
    }


    void activateTest(int testnumber)
    {
        
        if (testnumber == 1)
        {
            CallObjName(false);
            string Message = "Congratulations, You successfully find the target. Now please try to go back to the start point";
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
            counter = 2;
        }
    }


    void CallObjName(bool state)
    {
        if (state == false)
        {
            objDoNotCallName.Add("Wall1");
            objDoNotCallName.Add("Wall2");
            objDoNotCallName.Add("Wall3");
            objDoNotCallName.Add("Room");
            objDoNotCallName.Add("CardboardBox");
            objDoNotCallName.Add("TrashCan");
        }
        else
        {
            objDoNotCallName.Remove("Wall1");
            objDoNotCallName.Remove("Wall2");
            objDoNotCallName.Remove("Wall3");
            objDoNotCallName.Remove("Room");
        }
    }
}
