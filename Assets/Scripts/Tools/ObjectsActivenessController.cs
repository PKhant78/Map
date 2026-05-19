using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsActivenessController
{
    /* A list of game object we definitely don't want to disable in a game scene */
    List<string> objsDoNotDisable = new List<string>()
    { "Main Camera", "Directional Light", "SoundBall", "User", "Floor",
      "Accessibility Manager", "AR Session Origin", "AR Session",
      "SettingManager", "SceneMenuManager", "InstructionSystem", "SceneManager" };

    /* A list to record object disabled by the controller. We will use this list to reactivate them later */
    List<GameObject> disabledObjs = new List<GameObject>();

    /* A list record the original activeness status of objects which will be disabled by this controller */
    List<bool> originalActiveness = new List<bool>();


    /// <summary>
    /// Function temporarily disable all interactable objects in the scene
    /// Non-interactable objects like user avatar, instruction manager won't be disabled.
    /// A list of objects which we don't disable are in the list "objsDoNotDisable"
    /// 
    /// [Example of Usage]
    /// When we use UAP to speak an instruction, any name callout due to "cane hit an object" will
    /// cut off the speech of the instruction. So, we want to disable the interactable objects in the scene first
    /// </summary>
    public void DisableOtherObjs()
    {
        /* Find all root GameObjects which are in the scene */
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject obj in rootObjects)
        {
            string objName = obj.name;
            if (!objsDoNotDisable.Contains(objName))
            {
                /* Added the gameObjects which will be deactivated into a list */
                disabledObjs.Add(obj);

                /* Record the original activeness */
                originalActiveness.Add(obj.activeSelf);

                /* Try to deactivate the objects */
                if (obj)
                    obj.SetActive(false);
            }
        }
    }


    /// <summary>
    /// Function reactivate all interactable objects in the scene, which were previously disabled
    /// using "DisableOtherObjs()" function
    /// </summary>
    public void EnableOtherObjs()
    {
        /* Reactivate objects previously disabled by the system */
        for (int i = 0; i < disabledObjs.Count; ++i)
        {
            /* Get the object which we want to reactivate */
            GameObject obj = disabledObjs[i];

            /* If the obj is available (it's not destroyed or similar) 
             * 
             * [Note]
             * Why do we check if the "obj" exist or not? In UserMovement_IOS.cs,
             * we temporarily generated a GameObject and destroyed it immediately.
             * However, in the very short period of time it exists, it could be added
             * into the "disabledObjs" list. Since it's been destroyed immediately,
             * we can't access it and try to set it to active here. Thus we need to check
             * if an object exists before setting it to active
             */
            if (obj)
                obj.SetActive(originalActiveness[i]);
        }

        /* Clear the list which stores disabled objects */
        disabledObjs.Clear();

        /* Clear the list which stores original activeness of disabled objects */
        originalActiveness.Clear();
    }
}

