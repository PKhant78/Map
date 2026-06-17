using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

/**********************************************************************************************
 * ScriptName: MenuBase
 * Purpose: This script is the base class of all gesture menu, it contains the basic
 *          functions of a menu including:
 *                                          1. show menu content(Create object during the runtime)
 *                                          2. show selected items(Color will change)
 *                                          3. Detect the gesture of swiping leftward or rightward
 *                                          4. Move next or previous item to be selected
 *                                          5. Confirm the selected item and call the function
 * Notice: Some options have physical prefabs, when option confirm, the prefab will change.
 *         Some options don't have physical prefabs, it will use the previous prefab.
 * Developer: MRCane team
 * Last changed time: Wed July 12th 2022
 **********************************************************************************************/

public class MenuBase : MonoBehaviour
{
    //---------------------Inheritance Part---------------------
    protected List<UnityAction> FunctionList = new List<UnityAction>(); //The user needs to provide functions that correspond to the menu options
    protected Dictionary<string, bool> MenuOptionDictionary = new Dictionary<string, bool>(); //This dictionary keeps the menu option and the message if it has physical prefab.

    //---------------------GameObject from scene---------------------
    GameObject GripPoint;
    GameObject MenuContent;

    //---------------------Special Component---------------------
    Laser laser;
    protected VerbalManager_General verbalManager_General;
    InstructionManager instructionManagerInstance;

    //---------------------Generic variable---------------------
    int SelectItem = 0;
    int MenuState = 0;
    int ControlState = 0;
    float LastTime;
    float PresentTime;
    float OffSetTime = 0;
    float MaxOffSetTime = 2;
    string PrefabState = "";
    protected bool ShouldMenuOpen = true;


    protected virtual void Start()
    {
        //---------------------Make reference---------------------
        MenuContent = GameObject.Find("User/PortableMenu/Canvas/MenuContent");
        GripPoint = GameObject.Find("User/GripPoint");
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        laser = GameObject.Find("/User/GripPoint/Laser").GetComponent<Laser>();
        //---------------------Make reference---------------------

        if (ShouldMenuOpen)
        {
            ShowMenu();
        }

        //Check if the InstructionSystem are in the active scene.
        foreach (GameObject gm in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (gm.name == "InstructionSystem")
            {
                instructionManagerInstance = GameObject.Find("InstructionSystem").GetComponent<InstructionManager>();
            }
        }
        if (MenuOptionDictionary.ElementAt(SelectItem).Value == true)
            PrefabState = MenuOptionDictionary.ElementAt(SelectItem).Key;

    }

    private void Update()
    {

        if (ShouldMenuOpen)
        {
            if (instructionManagerInstance != null)
            {
                if (!instructionManagerInstance.IsRunning)
                {
                    if (IsMenuOn())
                    {
                        OpenMenu();
                        PresentTime = Time.time;
                        OffSetTime = PresentTime - LastTime;
                        if (OffSetTime > MaxOffSetTime)
                            SwitchNextChoice();
                    }
                    else
                        CloseMenu();
                }
            }
            else
            {
                if (IsMenuOn())
                {
                    OpenMenu();
                    PresentTime = Time.time;
                    OffSetTime = PresentTime - LastTime;
                    if(OffSetTime > MaxOffSetTime)
                        SwitchNextChoice();
                }
                else
                    CloseMenu();
            }
        }

    }

    /// <summary>
    /// Check if user's wrist are bent towards to above direction. If yes return true(Open menu)
    /// Read Only
    /// </summary>
    protected bool IsMenuOn()
    {
        return GripPoint.transform.eulerAngles.x >= 270 && GripPoint.transform.eulerAngles.x < 350;
    }


    /// <summary>
    /// Create the menu option as text UI.
    /// </summary>
    private void ShowMenu()
    {
        DisplayMenuItem("Menu", 404, 0.75, 0.9, Color.black);
        switch (MenuOptionDictionary.Count)
        {
            case 1:
                DisplayMenuItem(MenuOptionDictionary.ElementAt(SelectItem).Key, SelectItem, 0.65, 0.82, Color.red);
                break;
            case 2:
                DisplayMenuItem(MenuOptionDictionary.ElementAt(SelectItem).Key, SelectItem, 0.65, 0.82, Color.red);
                DisplayMenuItem(MenuOptionDictionary.ElementAt(SelectItem + 1).Key, SelectItem + 1, 0.75, 0.82, Color.black);
                break;
            case 3:
                DisplayMenuItem(MenuOptionDictionary.ElementAt(SelectItem).Key, SelectItem, 0.65, 0.82, Color.red);
                DisplayMenuItem(MenuOptionDictionary.ElementAt(SelectItem + 1).Key, SelectItem + 1, 0.75, 0.82, Color.black);
                DisplayMenuItem(MenuOptionDictionary.ElementAt(SelectItem + 2).Key, SelectItem + 2, 0.85, 0.82, Color.black);
                break;
        }
        if (MenuOptionDictionary.Count > 3)
        {
            DisplayMenuItem(MenuOptionDictionary.ElementAt(SelectItem).Key, SelectItem, 0.65, 0.82, Color.red);
            DisplayMenuItem(MenuOptionDictionary.ElementAt(SelectItem + 1).Key, SelectItem + 1, 0.75, 0.82, Color.black);
            DisplayMenuItem(MenuOptionDictionary.ElementAt(SelectItem + 2).Key, SelectItem + 2, 0.85, 0.82, Color.black);
        }
    }

    /// <summary>
    /// Set menu as visiable(invisiable by default)
    /// </summary>
    private void OpenMenu()
    {
        if (ControlState == 0)
        {
            MenuContent.GetComponent<CanvasGroup>().alpha = 1;
            laser.LaserModeShutdown();
            try
            {
                //verbalManager_General.Speak("Menu Open");
                //verbalManager_General.Speak(MenuOptionDictionary.ElementAt(SelectItem).Key);
                LastTime = Time.time;
            }
            catch (InvalidOperationException e)
            {
                Debug.Log("Error exists when open the Menu: " + e);
            }
            MenuState = 1;
            ControlState = 1;
        }
    }

    /// <summary>
    /// Set menu as invisiable
    /// </summary>
    private void CloseMenu()
    {
        if (MenuState == 1)
        {
            MenuContent.GetComponent<CanvasGroup>().alpha = 0;
            try
            {
                //verbalManager_General.Speak("Menu Close");
            }
            catch (InvalidOperationException e)
            {
                Debug.Log("Error exists when close the Menu: " + e);
            }
            if (MenuState == 1)
                MenuState = 2;
            LastTime = Time.time;
            ControlState = 0;
            ConfirmChoice();

            if (MenuOptionDictionary.ElementAt(SelectItem).Value == true)
                PrefabState = MenuOptionDictionary.ElementAt(SelectItem).Key;
        }
    }

    /// <summary>
    /// If the menu option displays on the screen are less or equal to 3, change the color only
    /// to show which item has been selected.
    /// If the menu option displyas on the screen are greater than 3, change the text content and
    /// color to show more options and selected items.
    /// </summary>
    protected void SwitchNextChoice()
    {

        if (MenuOptionDictionary.Count <= 3)
        {
            if (SelectItem < MenuOptionDictionary.Count - 1)
            {
                ChangeMenuItemColor(SelectItem, Color.black);
                ChangeMenuItemColor(SelectItem + 1, Color.red);
            }

            if (SelectItem >= MenuOptionDictionary.Count - 1)
            {
                ChangeMenuItemColor(SelectItem, Color.black);
                ChangeMenuItemColor(0, Color.red);
                SelectItem = -1;
            }
            SelectItem++;
        }
        else
        {
            if (SelectItem < 2)
            {
                ChangeMenuItemColor(SelectItem, Color.black);
                ChangeMenuItemColor(SelectItem + 1, Color.red);
            }

            if (SelectItem >= 2)
            {
                if (SelectItem < MenuOptionDictionary.Count - 1)
                {
                    ChangeMenuItemName(0, SelectItem - 1);
                    ChangeMenuItemName(1, SelectItem);
                    ChangeMenuItemName(2, SelectItem + 1);
                    ChangeMenuItemColor(2, Color.red);
                }

                if (SelectItem >= MenuOptionDictionary.Count - 1)
                {
                    Debug.Log("SelectItem: " + SelectItem);
                    ChangeMenuItemColor(2, Color.black);
                    ChangeMenuItemName(0, 0);
                    ChangeMenuItemName(1, 1);
                    ChangeMenuItemName(2, 2);
                    ChangeMenuItemColor(0, Color.red);
                    SelectItem = -1;
                }
            }
            SelectItem++;
        }

        try
        {
            verbalManager_General.Speak(MenuOptionDictionary.ElementAt(SelectItem).Key);
        }
        catch (InvalidOperationException e)
        {
            Debug.Log("Error: " + e);
        }
        LastTime = Time.time;
    }


    protected void SwitchPreviousChoice()
    {
        if (MenuOptionDictionary.Count <= 3)
        {
            if (SelectItem >= 1)
            {
                ChangeMenuItemColor(SelectItem, Color.black);
                ChangeMenuItemColor(SelectItem - 1, Color.red);
            }
            if (SelectItem == 0)
            {
                ChangeMenuItemColor(SelectItem, Color.black);
                ChangeMenuItemColor(MenuOptionDictionary.Count - 1, Color.red);
                SelectItem = MenuOptionDictionary.Count;
            }
            SelectItem--;
        }
        else
        {
            Debug.Log("SelectItem: " + SelectItem);
            if (SelectItem <= 2 && SelectItem > 0)
            {
                ChangeMenuItemColor(SelectItem, Color.black);
                ChangeMenuItemColor(SelectItem - 1, Color.red);
            }

            if (SelectItem > 2)
            {
                ChangeMenuItemName(0, SelectItem - 3);
                ChangeMenuItemName(1, SelectItem - 2);
                ChangeMenuItemName(2, SelectItem - 1);
            }

            if (SelectItem == 0)
            {
                ChangeMenuItemColor(0, Color.black);
                ChangeMenuItemName(0, MenuOptionDictionary.Count - 3);
                ChangeMenuItemName(1, MenuOptionDictionary.Count - 2);
                ChangeMenuItemName(2, MenuOptionDictionary.Count - 1);
                ChangeMenuItemColor(2, Color.red);
                SelectItem = MenuOptionDictionary.Count;
            }
            SelectItem--;
        }
        verbalManager_General.Speak(MenuOptionDictionary.ElementAt(SelectItem).Key);
    }


    /// <summary>
    /// Confirm option when user close the menu, the selected item will be choosed.
    /// </summary>
    private void ConfirmChoice()
    {
        verbalManager_General.Speak(MenuOptionDictionary.ElementAt(SelectItem).Key + " start");
        FunctionList[SelectItem]();
        MenuState = 0;
        if (MenuOptionDictionary.ElementAt(SelectItem).Value == true)
            PrefabState = MenuOptionDictionary.ElementAt(SelectItem).Key;
        Debug.Log("PrefabState: "+ PrefabState);
        switch (PrefabState)
        {
            case "CaneMode":
                laser.LaserModeOff();
                break;
            case "LaserMode":
                laser.LaserModeOn();
                break;
        }

    }


    /// <summary>
    /// Display Function - create content
    /// </summary>
    private void DisplayMenuItem(string name, int OptionNum, double w, double h, Color color)
    {
        float width = Screen.width * (float)w;
        float height = Screen.height * (float)h;
        GameObject newText = new GameObject("Option" + OptionNum);
        newText.transform.SetParent(MenuContent.transform);
        Text TextContent = newText.AddComponent<Text>();
        TextContent.text = name;
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        TextContent.font = ArialFont;
        TextContent.color = color;
        TextContent.material = ArialFont.material;
        TextContent.fontSize = 25;
        TextContent.rectTransform.sizeDelta = new Vector2(160, 30);
        TextContent.alignment = TextAnchor.MiddleCenter;
        TextContent.transform.position = new Vector3(width, height, 0);
        TextContent.verticalOverflow = VerticalWrapMode.Overflow;
    }

    /// <summary>
    /// Display Function - change color
    /// </summary>
    private void ChangeMenuItemColor(int SelectItem, Color color)
    {
        GameObject Item = GameObject.Find("User/PortableMenu/Canvas/MenuContent/Option" + SelectItem);
        Item.GetComponent<Text>().color = color;
    }

    /// <summary>
    ///  Display Function - change text content
    /// </summary>
    private void ChangeMenuItemName(int OptionNum, int SelectNum)
    {
        GameObject Item = GameObject.Find("User/PortableMenu/Canvas/MenuContent/Option" + OptionNum);
        Item.GetComponent<Text>().text = MenuOptionDictionary.ElementAt(SelectNum).Key;
    }

}
