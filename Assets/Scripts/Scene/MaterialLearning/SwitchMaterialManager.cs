using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchMaterialManager : MonoBehaviour
{
    MaterialLearnManager materialLearnManager;      // The MaterialLearnManager class to call function for page switching
    GestureMenu gestureMenu;                        // A reference to the "normal gesture menu" class on User prefab


    /// <summary>
    /// Awake is the first function be called in Unity Execution loop
    /// </summary>
    void Awake()
    {
        /* Initialize the member variables */
        InitVariables();

        /* Disable the normal gesture menu to prevent unwanted actions */
        DisableNormalGestureMenu();
    }


    /// <summary>
    /// Function initialize member variables
    /// </summary>
    void InitVariables()
    {
        materialLearnManager = GameObject.Find("SceneManager").GetComponent<MaterialLearnManager>();
        gestureMenu = GameObject.Find("User/PortableMenu").GetComponent<GestureMenu>();
    }


    /// <summary>
    /// The action (Page switching) the button takes after hitted by user's cane
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        Transform other = collision.collider.transform;                                 // the collider object which the button collides with

        if (other.name == "Cane")                                                       // if the button hitted by cane
        {
            if (AccessColliderInfo.GetRootName(transform) == "NextButton")              // update MatLearnBalls with materials from next group
            {
                materialLearnManager.NextMatGroup();                                    // switch to next group of materials
            }
            else if (AccessColliderInfo.GetRootName(transform) == "PrevButton")         // update MatLearnBalls with materials from last group
            {  
                materialLearnManager.PrevMatGroup();                                    // switch to previous group of materials
            }
        }
    }


    /// <summary>
    /// Function disable the normal gesture menu on User prefab.
    /// Because we don't need it in this scene. We don't want the user
    /// accidentally turn on menu when we give instruction 
    /// </summary>
    void DisableNormalGestureMenu()
    {
        gestureMenu.enabled = false;
    }
}
