using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickyScenarioTargetProcess : MonoBehaviour
{
    bool isLast = false;
    int RepeatNum = 0;
    List<string> objDoNotCallName;
    VerbalManager_General verbalManager_General;
    string Message = "";
    bool[] isRepeat;

    void Start()
    {
        isRepeat = new bool[5];
        for (int i = 0; i < 4; i++)
        {
            isRepeat[i] = false;
        }
        isRepeat[4] = true;
        objDoNotCallName = GameObject.Find("Cane").GetComponent<CaneContact>().objDoNotCallName;
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
    }

    private void Update()
    {
        Debug.Log("counternum: " + RepeatNum);
    }


    private void OnCollisionEnter(Collision collision)
    {
        Transform other = collision.collider.transform;                                         // the collider object which the button collides with
        if (other.name == "Cane")                                                               // if the button hitted by cane
        {
            switch (transform.name)
            {
                case "Target1":
                    Debug.Log("HitTarget1");
                    Debug.Log("isrepeat: " + isRepeat[0]);
                    InstructionProcess(1);
                    isRepeat[4] = false;
                    break;
                case "Target2":
                    Debug.Log("HitTarget2");
                    InstructionProcess(2);
                    break;
                case "Target3":
                    Debug.Log("HitTarget3");
                    InstructionProcess(3);
                    break;
                case "Target4":
                    Debug.Log("HitTarget4");
                    InstructionProcess(4);
                    break;
                case "Target5":
                    Debug.Log("HitTarget5");
                    InstructionProcess(5);
                    isRepeat[0] = false;
                    break;
            }
        }
    }



    void InstructionProcess(int InstructionNum)
    {
        if (InstructionNum == 1 && isRepeat[InstructionNum - 1] == false)
        {
            if (isLast == false)
            {
                Message = "Please try to use shoreline strategy to go left along the wall.";
                StartCoroutine(CallInstructions());
                isRepeat[InstructionNum - 1] = true;
                Debug.Log("Something happen");
            }
            else
            {
                string endMessage = "Well done, you passed all tricky test in this room, let's go back to main room.";
                verbalManager_General.SpeakWithoutDisturb(endMessage);
                verbalManager_General.SpeakWaitAndCallback("", () =>
                {
                    SceneJumpHelper.ResetThenSwitchScene("MainMenu");
                });
            }
        }

        if (InstructionNum == 2 && isRepeat[InstructionNum - 1] == false)
        {
            Message = "Now you are facing the junction of the two walls. please try to continue walking down the wall. " +
                "if you encounter any problem. please signal the instructor";
            StartCoroutine(CallInstructions());
            isRepeat[InstructionNum - 1] = true;
            Debug.Log("Something happen");
        }

        if (InstructionNum == 3 && isRepeat[InstructionNum - 1] == false)
        {
            Message = "Now, there is an obstacle on your way. this obstacle is next to the wall. " +
                "please try to lift your virtual cane from the bottom up to find the name of this object. " +
                "and try to bypass this obstacle and continue along wall walking." +
                "Feel free to signal the instructor if you encounter any problem.";
            StartCoroutine(CallInstructions());
            isRepeat[InstructionNum - 1] = true;
            Debug.Log("Something happen");
        }

        if (InstructionNum == 4 && isRepeat[InstructionNum - 1] == false)
        {
            Message = "There is another obstacle beside you." +
                " there are two obstacles beside you placed at the junction of two walls." +
                " try to use the same method to find the name of this virtual object." +
                " and try to avoid it and continue walking along the wall.";
            StartCoroutine(CallInstructions());
            isRepeat[InstructionNum - 1] = true;
            Debug.Log("Something happen");
        }

        if (InstructionNum == 5 && isRepeat[InstructionNum - 1] == false)
        {
            Message = "There is an obstacle on your way. this obstacle is next to the wall. " +
                "please try to lift your virtual cane from the bottom up to find the name of this object. " +
                "and try to bypass this obstacle and continue along wall walking." +
                "Feel free to signal the instructor if you encounter any problem.";
            StartCoroutine(CallInstructions());
            isLast = true;
            isRepeat[InstructionNum - 1] = true;
            Debug.Log("Something happen");
        }

    }

    IEnumerator CallInstructions()
    {
        yield return new WaitForSeconds(5);
        CallObjName(false);
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
