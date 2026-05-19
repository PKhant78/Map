using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**********************************************************************************************
 * ScriptName: GestureMenu
 * Purpose: This script is the gesture menu template, it needs to provide:
 *          1. MenuOption in MenuOptionDictionary, and the value will be true, if option has physical prefab
 *          2. Function for each menu option in FunctionList
 *                                     
 * Notice: Some options have physical prefabs, when option confirm, the prefab will change.
 *         Some options don't have physical prefabs, it will use the previous prefab.
 * Developer: MRCane team
 * Last changed time: Wed July 12th 2022
 **********************************************************************************************/



public class GestureMenu : MenuBase
{

    protected override void Start()
    {

        MenuOptionDictionary.Add("CaneMode", true);
        MenuOptionDictionary.Add("LaserMode", true);
        MenuOptionDictionary.Add("Replay", false);
        MenuOptionDictionary.Add("Hint", false);

        FunctionList.Add(FunctionForCaneMode);
        FunctionList.Add(FunctionForLaserMode);
        FunctionList.Add(FunctionForReplay);
        FunctionList.Add(FunctionForHint);
        base.Start();
    }



    void FunctionForCaneMode()
    {
        Debug.Log("this is function for cane mode");
    }

    void FunctionForLaserMode()
    {
        Debug.Log("this is function for laser mode");
    }

    void FunctionForReplay()
    {
        Debug.Log("islasermodeOn ");
        verbalManager_General.Speak("This is a replay example");

    }

    void FunctionForHint()
    {
        Debug.Log("this is function for hint");
        verbalManager_General.Speak("This is a hint exampleß");
    }


    public void MenuOpen()
    {
        base.ShouldMenuOpen = true;
    }

    public void MenuClose()
    {
        base.ShouldMenuOpen = false;
    }

}

