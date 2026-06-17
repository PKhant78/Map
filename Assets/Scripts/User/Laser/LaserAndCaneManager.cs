using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************************
 * ScriptName: LaserManager
 * Purpose: This script should be mounted on the SettingManager prefab, Based on the design 
 *          principle, Cane and Laser these two prefab's activation and deactivation statements 
 *          should not contain in base scripts.
 * Developer: MRCane team
 * Last changed time: Wed July 6th 2022
 **********************************************************************************************/


public class LaserAndCaneManager : MonoBehaviour
{
    static GameObject Cane;
    static GameObject Laser;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Awake()
    {
        Cane = GameObject.Find("User/GripPoint/Cane");
        Laser = GameObject.Find("User/GripPoint/Laser");
    }

    /// <summary>
    /// Activate the prefab named cane 
    /// </summary>
    public static void ActivateCane()
    {
        Cane.SetActive(true);
    }

    /// <summary>
    /// Deactivate the prefab named cane 
    /// </summary>
    public static void DeactivateCane()
    {
        Cane.SetActive(false);
    }

    /// <summary>
    /// Activate the prefab named laser 
    /// </summary>
    public static void ActivateLaser()
    {
        Laser.SetActive(true);
    }

    /// <summary>
    /// Deactivate the prefab named laser 
    /// </summary>
    public static void DeactivateLaser()
    {
        Laser.SetActive(false);
    }

    public static bool GetCaneActivity()
    {
        return Cane.activeSelf;
    }

    public static bool GetLaserActivity()
    {
        return Laser.activeSelf;
    }
}
