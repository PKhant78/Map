using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreScavengerHuntTrainingProcess : MonoBehaviour
{
    VerbalManager_General verbalManager_General;// For generate the broadcast, part of the UAP plugin.
    NavigationManager navigationManager;
    GameObject WayPoint;
    GameObject WayPoints;
    GameObject[] WayPointArray;
    GameObject GripPoint;
    GameObject CardboardBox;
    GameObject PlantPot;
    GameObject Floor;
    GameObject Cube;
    Vector3 prefabPosition;
    GameObject prefab;
    int counter = 1;
    void Start()
    {
        WayPoint = GameObject.Find("NavigationSystem/WayPoints/WayPoint");
        WayPoints = GameObject.Find("NavigationSystem/WayPoints");
        GripPoint = GameObject.Find("/User/GripPoint");
        CardboardBox = GameObject.Find("CardboardBox");
        PlantPot = GameObject.Find("PlantPot");
        Floor = GameObject.Find("Floor");
        Cube = GameObject.Find("Cube");
        WayPointArray = new GameObject[5];
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        for(int i =0; i<5;i++)
        {
            WayPointArray[i] = WayPoints.transform.GetChild(i).gameObject;
        }
        navigationManager = GameObject.Find("NavigationSystem").GetComponent<NavigationManager>();
        navigationManager.RestartNavSystem();
    }

    private void Update()
    {
        for(int i = 0; i< 5; i++)
        {
            if(WayPointArray[i].activeSelf == true && counter == 1)
            {
                activateTest(i);
            }
        }
        if (WayPointArray[4].activeSelf == true && counter == 0)
        {
            counter = 1;
        }
    }

    void activateTest(int testnumber)
    {
        if(testnumber == 2)
        {
            prefabPosition = new Vector3(-4.254f, 0.195f, -3.918f);
            prefab = Instantiate(CardboardBox, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab.transform.name = CardboardBox.transform.name.Replace("(clone)", "").Trim();

            prefabPosition = new Vector3(-3.485f, 0.195f, -4.785f);
            prefab = Instantiate(PlantPot, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab.transform.name = PlantPot.transform.name.Replace("(clone)", "").Trim();
            counter--;
        }

        if (testnumber == 4)
        {
            prefabPosition = new Vector3(-6.04f, 0.195f, -7.04f);
            prefab = Instantiate(Cube, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab.transform.name = Cube.transform.name.Replace("(clone)", "").Trim().ToString()+"1";

            prefabPosition = new Vector3(-4.52f, 0.195f, -7.04f);
            prefab = Instantiate(Cube, prefabPosition, Quaternion.Euler(0, 90, 0));
            prefab.transform.name = Cube.transform.name.Replace("(clone)", "").Trim().ToString() + "2"; ;
            counter--;
            counter = 2;
        }
    }
}
