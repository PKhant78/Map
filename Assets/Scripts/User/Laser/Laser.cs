using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**********************************************************************************************
 * ScriptName: Laser
 * Purpose: This script should be mounted on the Laser prefab, this is the base script that 
 *          provides functions of the laser, including 
 *          laser broadcast function: When the laser pointer interacts with other objects in
 *          the scene, it will read the interacted object’s name and spatial information.
 * Developer: MRCane team
 * Last changed time: Wed July 6th 2022
 **********************************************************************************************/



[RequireComponent((typeof(LineRenderer)))]
public class Laser : MonoBehaviour
{
    public bool LaserMode = false;              // Create the check box in the laser.cs script
    string message = "";                        // The message used for UAP Plugin to broadcast

    //Transform PreviousPrefab;                 // Check if laser pointer interact with same object, temporary removed, maybe will used in the future
    LineRenderer lr;                            // Adjust the appearance of the laser style, inlcuding direction, length..
    SoundBallMovement soundBallMovement;        // The prefab used to play sound.
    VerbalManager_General verbalManager_General;// For generate the broadcast, part of the UAP plugin.

    Vector3 rayLastPos;                         // ray's hitting position in last frame
    bool rayMoved = false;                      // variable indicates whether the ray's hitting point moved
    float rayMoveDist;
    float rayMoveBenchmarkDist = 0.1f;         // [Default = 0.03f] benchmark value to determine if the ray moved (actually the ray's hitting point)
    float distance;

    GameObject laser;
    GameObject cane;

    void Start()
    {   //---------------------Make reference---------------------
        lr = GetComponent<LineRenderer>();
        soundBallMovement = GameObject.Find("SoundBall").GetComponent<SoundBallMovement>();
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        laser = GameObject.Find("/User/GripPoint/Laser");
        cane = GameObject.Find("/User/GripPoint/Cane");
        //---------------------Make reference---------------------
        
        /// <summary>
        /// Open laser mode before Menu script calls, it used for debugging.
        /// Notice: Do not put LaserManagerInstance.DeactivateLaser() inside of the else, it may cause uap crashes.
        /// </summary>
        if (LaserMode == true)
        {
            //LaserAndCaneManager.DeactivateCane();
            //LaserAndCaneManager.ActivateLaser();
            laser.SetActive(true);
            //cane.SetActive(false);
            //Debug.LogError("===========111111");
        }
        else
        {
            //LaserAndCaneManager.ActivateCane();
            cane.SetActive(true);
            laser.SetActive(false);
        }
    }

    /// <summary>
	/// Mainly used to detect the interaction between the laser and objects in the scene
    /// If exists interactions, use UAP Plugin broadcast the name of the prefab and spatial information
    ///
    /// Notice: UAP Plugin may not play any sound, since speak funtion could be called multiple times,
    ///         in the update function, and when speak function called, it will call stop function first.
	/// </summary>
    void Update()
    {
        try
        {
            if (LaserMode == true)
            {
                lr.SetPosition(0, transform.position);
                RaycastHit hit;
                /// Physics.Raycast(Vector3 origin, Vector3 direction)
                if (Physics.Raycast(transform.position, transform.forward, out hit))
                {
                    Debug.Log("Laser.cs update function: hit!");
                    lr.SetPosition(1, hit.point);
                    MoveSoundBall(hit);
                    DetectIfRayMoved(hit);
                    if (rayMoved)
                    {
                        Debug.Log("Laser.cs update function: RayMoved");
                        BroadcastObject(hit);
                    }
                }
                else
                {
                    lr.SetPosition(1, transform.forward * 200);
                    verbalManager_General.StopSpeak();
                }
            }
        }
        catch (InvalidOperationException e)
        {
            Debug.Log("Error: " + e);
        }
    }

    /// <summary>
	/// Detect if laser stay in the same place in order to erase the annoying repeat broadcasts
	/// </summary>
    private void DetectIfRayMoved(RaycastHit hit)
    {
        Vector3 contactPoint = hit.point;
        rayMoveDist = Vector3.Distance(contactPoint, rayLastPos);
        rayMoved = (rayMoveDist > rayMoveBenchmarkDist);
        rayLastPos = contactPoint;
    }

    /// <summary>
	/// Get Distance from GetDistance(RaycastHit hit), call UAP Plugin function to broadcast
	/// </summary>
    private void BroadcastObject(RaycastHit hit)
    {
        string distStr = GetDistance(hit);
        if(SettingsMenu.measureSystem == "US")
        {
            message = hit.transform.root.name + ". " + distStr + "meters";
        }
        else
        {
            message = hit.transform.root.name + ". " + distStr + "feet";
        }


        verbalManager_General.Speak(message);
    }


    public void CallByDifferentLayers()
    {

    }


    /// <summary>
	/// Get the distance between user and the place where laser interacts with object
    /// Return string type since "." this will be read dot in the UAP Plugin, sounds not clear
	/// </summary>
    private string GetDistance(RaycastHit hit)
    {
        if (SettingsMenu.measureSystem == "US")
        {
            distance = Mathf.Round(hit.distance * 10f) / 10f;
        }
        else
        {
            distance = Mathf.Round((float)(hit.distance * 3.28) * 10f) / 10f;
        }
        string DistanceForUAP = distance.ToString();
        if (DistanceForUAP.Contains("."))
            return DistanceForUAP.Replace(".", " point ");
        return DistanceForUAP;
    }

    /// <summary>
	/// open the laser mode
	/// </summary>
    public void LaserModeOn()
    {
        //LaserAndCaneManager.DeactivateCane();
        //LaserAndCaneManager.ActivateLaser();
        laser.SetActive(true);
        //cane.SetActive(false);
        LaserMode = true;
        //Debug.LogError("===========2222222");
    }

    /// <summary>
	/// Close the laser mode
	/// </summary>
    public void LaserModeOff()
    {
        //LaserAndCaneManager.ActivateCane();
        //LaserAndCaneManager.DeactivateLaser();
        laser.SetActive(false);
        cane.SetActive(true);
        LaserMode = false;
    }

    public void LaserModeShutdown()
    {
        laser.SetActive(false);
        //cane.SetActive(false);
        LaserMode = false;
        //Debug.LogError("===========333333");
    }

    public void LaserModeInit()
    {
        laser.SetActive(true);
        cane.SetActive(true);
        LaserMode = true;
    }

    public bool isLaserModeOn()
    {
        return laser.activeSelf;
    }

    /// <summary>
	/// Detect if laser pointer interact with same object
    /// temporary not used
	/// </summary>
    //private bool IsSameObject(Transform HitObject)
    //{
    //    bool result = false;

    //    if (PreviousPrefab == HitObject)
    //        result = true;

    //    if (PreviousPrefab == null || PreviousPrefab != HitObject)
    //        PreviousPrefab = HitObject.transform.root;

    //    return result;
    //}

    /// <summary>
	/// move sound ball prefab into the interaction place in order to create the more realistic sound
	/// </summary>
    private void MoveSoundBall(RaycastHit hit)
    {
        Vector3 contactPoint = hit.point;
        soundBallMovement.transportSoundBall(contactPoint);
    }


}
