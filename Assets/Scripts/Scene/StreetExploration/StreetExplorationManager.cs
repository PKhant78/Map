using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetExplorationManager : MonoBehaviour
{
    CaneContact caneContact;                        // A reference to the CaneContact class
    FrontInfoRetriever frontInfoRetriever;          // A reference to the FrontInfoRetriever class
    SurroundInfoRetriever surroundInfoRetriever;    // A reference to the SurroundInfoRetriever class

    List<string> streets = new()                    // A list of street names which we don't want to provide physical vibration for
    {
        "GreenwichStreet",
        "CrossroadOfChambers&Greenwich",
        "ChambersStreet",
        "CrossroadOfGreenwich&Warren",
        "WarrenStreet"
    };


    private void Awake()
    {
        InitVariables();

        /* Preparations */
        AddToNotVibrate();
        AddToFrontNotDetect();
        AddToSurroundNotDetect();
        SpecifyLevelOfDetection();
    }


    /// <summary>
    /// Initialize important member variables
    /// </summary>
    void InitVariables()
    {
        caneContact = GameObject.Find("User/GripPoint/Cane").GetComponent<CaneContact>();
        frontInfoRetriever = GameObject.Find("User/Head").GetComponent<FrontInfoRetriever>();
        surroundInfoRetriever = GameObject.Find("User/Body/SurroundRadar").GetComponent<SurroundInfoRetriever>();
    }


    /// <summary>
    /// Function adds object to "NotVibrate" list ===> so the system won't provide vibartion when cane hits these objects
    /// </summary>
    void AddToNotVibrate()
    {
        foreach (string street in streets)
            caneContact.objDoNotVibrate.Add(street);
    }


    /// <summary>
    /// Function adds object to "NotDetect" list in FrontInfoRetriever.cs
    /// </summary>
    void AddToFrontNotDetect()
    {
        foreach (string street in streets)
            frontInfoRetriever.objNotDetect.Add(street);
    }


    /// <summary>
    /// Function adds object to "NotDetect" list in SurroundInfoRetriever.cs
    /// </summary>
    void AddToSurroundNotDetect()
    {
        foreach (string street in streets)
            surroundInfoRetriever.objNotDetect.Add(street);
    }


    /// <summary>
    /// Function help with specifying level of detection, for objects in the scene,
    /// when using front info retriever and surround info retriever
    /// </summary>
    void SpecifyLevelOfDetection()
    {
        frontInfoRetriever.specifyObjLevelDict.Add("City", 2);
        surroundInfoRetriever.specifyObjLevelDict.Add("City", 2);
    }

}

