using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class TutorialPart2Process : MonoBehaviour
{
    VerbalManager_General verbalManager_General;// For generate the broadcast, part of the UAP plugin.
    NavigationManager navigationManager;
    //string Verbal_direction;
    GameObject User;
    GameObject WayPoint;
    GameObject WayPoints;
    GameObject OneWayPoint;
    GameObject LastWayPoint;
    GameObject Corridor;
    GameObject GripPoint;
    int counter = 0;
    int counter2 = 0;
    int counter3 = 0;
    int counter4 = 0; // too ugly here, need to be fixed
    int counter5 = 0;
    float timeDelay = 2f;
    float time = 0f;



    void Start()
    {
        User = GameObject.Find("User");
        WayPoint = GameObject.Find("NavigationSystem/WayPoints/WayPoint");
        WayPoints = GameObject.Find("NavigationSystem/WayPoints");
        Corridor = GameObject.Find("DumbbellRoom/RightsdieWall2");
        GripPoint = GameObject.Find("/User/GripPoint");
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        OneWayPoint = WayPoints.transform.GetChild(0).gameObject;

        navigationManager = GameObject.Find("NavigationSystem").GetComponent<NavigationManager>();
        navigationManager.RestartNavSystem();
    }


    void Update()
    {
        //WayPoint.transform.GetChild(1)
        //WayPoint.activeSelf == true
        if (OneWayPoint.activeSelf == false && counter == 1)
        {
            Debug.Log("update");
            verbalManager_General.Speak("Now you are near the corridor. There is a new waypoint in the exit of the corridor. Try to find it and pass through the corridor" +
                "and find the room exit");
            counter++;
        }

        if (GripPoint.transform.rotation.y > 0 && GripPoint.transform.rotation.y < 180 && Vector3.Distance(GripPoint.transform.position, Corridor.transform.position) < 1.5f && User.transform.position.z > -0.3)
        {
            time = time + 1f * Time.deltaTime;

            if (time > timeDelay)
            {
                counter2 = 0;
                time = 0f;
            }

            // Debug.Log("Repeat the warning"); 
            if (counter2 == 0)
            {
                verbalManager_General.Speak("wrong direction, turn back");
                counter2++;
                Debug.Log("Repeat the warning");
            }

        }
        //Debug.Log("user position: " + User.transform.position);
        if (User.transform.position.z >= 3.0 && (User.transform.position.x <= -1.8 && User.transform.position.x >= -3.2) && counter3 == 0)
        {
            counter3++;
            Debug.Log("EXIT ROOM");
            OneWayPoint = WayPoints.transform.GetChild(4).gameObject;
            verbalManager_General.Speak("Congratulation! You sucessfully leave the test room! Now You only need to follow the drum beat to find three object.");

        }

        for (int i = 0; i <= 5; i++)
        {
            OneWayPoint = WayPoints.transform.GetChild(i).gameObject;
            if (OneWayPoint.activeSelf == false)
            {
                counter4++;
            }
        }

        if (counter4 != 6)
        {
            counter4 = 0;
        }

        if (counter4 == 6 && counter5 == 0)
        {
            counter5++;
            string endMessage = "Congratulation! You have sucessfully finished all tutorial sessions. You did really good job. Now let us go back the menu.";
            verbalManager_General.SpeakWaitAndCallback(endMessage, () =>
            {
                SceneJumpHelper.ResetThenSwitchScene("MainMenu");           // do all the resets needed then back to the main menu
            });
        }
    }

}


