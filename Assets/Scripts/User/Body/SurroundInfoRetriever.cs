using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;


public class SurroundInfoRetriever : MonoBehaviour
{
    static public bool allowRetriever = true;               // [Default = true] If the system allow the surround info retriever to be used (User will set up this in the settings menu)
    public bool useRetriever = false;                       // Do we use this SurroundInfoRetriever?

    string currentScene;                                    // Record the name of current scene
    string directionMusicName = "MsAndOverHereDirectionSound";    // The name of the directional music to play

    string radarRange_Imp = "2 meters";                     // Range (radius) of the radar in meter
    string radarRange_Us = "6 and half feet";               // Range (radius) of the radar in feet

    SurroundRadar surroundRadar;                            // An reference to the SurroundRadar class ===> the class detects all objects hitted by the "SurroundRadar" gameObject
    HeadNodDetector headNodDetector;                        // A class detects the head-nodding motion
    VerbalManager_General verbalManager_General;            // The class controls text to speech
    MusicPlayerManager musicPlayerManager;                  // The class which manages the MusicPlayer on user prefab
    Transform headTrans;                                    // The transform of user's head

    int currPlayingId = 0;                                   // ID which help with managing ongoing "PlayObjAround()" process  

    List<RadarTarget> radarTargets = new();                 // List stores HitInfo of all meaningful objects hitted by radar
    List<string> radarTargetName = new();                   // The name of target objects hitted by radar

    /* The objects we don't want to detect in this class */
    public List<string> objNotDetect = new List<string>()
    { "Main Camera", "Directional Light", "Floor", "User", "SoundBall", "MusicPlayer",
      "Accessibility Manager", "AR Session Origin", "AR Session", "NavigationSystem",
      "SettingManager", "SceneMenuManager", "InstructionSystem", "SceneManager"};

    /* The dictionary stores the object root name, and the hierarchy 
     * level of that object we want to read once hit it */
    public Dictionary<string, int> specifyObjLevelDict = new()
    { { "Room", 2 } };


    /// <summary>
    /// Awake function is the first function called in an Unity execution loop
    /// </summary>
    private void Awake()
    {
        InitVariables();
    }


    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        /* If "ReverseHeadNod" happended */
        if (allowRetriever && useRetriever && (Input.GetKeyDown(KeyCode.Z) || headNodDetector.ReverseHeadNodded))
        {
            /* Doing tasks below to stop the last ongoing "PlayObjsAround()" if any */
            StopCurrPlayObjsAround();

            /* Collect objects in radar and show them using audio + speech */
            CollectObjectFromSurroundRadar();
            PlayObjsAround();
            //SpeakObjsAround();
        }
    }


    /// <summary>
    /// Funtion attempts to stop the current on-going "PlayObjsAround()" async process
    /// </summary>
    public void StopCurrPlayObjsAround()
    {
        currPlayingId++;
        musicPlayerManager.StopSpaAudio();
    }


    /// <summary>
    /// Function initializes all important member variables
    /// </summary>
    void InitVariables()
    {
        surroundRadar = transform.GetComponent<SurroundRadar>();
        headNodDetector = GameObject.Find("User/Head").GetComponent<HeadNodDetector>();
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        headTrans = GameObject.Find("User/Head").transform;
        musicPlayerManager = GameObject.Find("User/MusicPlayer").GetComponent<MusicPlayerManager>();

        currentScene = SceneManager.GetActiveScene().name;
    }


    /// <summary>
    /// Function gets all meaningful objects that are within the range of radar
    /// </summary>
    void CollectObjectFromSurroundRadar()
    {
        /* Clear the existing data from the list */
        radarTargets.Clear();
        radarTargetName.Clear();

        /* Get info of all objects hitted by radar */
        List<HitInfo> hits = surroundRadar.WithinRadar;

        if (hits.Count != 0)
        {
            foreach (HitInfo hitInfo in hits)
            {
                Transform hitTrans = hitInfo.trans;                         // Transform of hitted gameObject
                string rootName = hitTrans.root.name;                       // The root name of hitted gameObject

                Transform targetTrans = hitTrans.root;                      // The transform of the target level of gameObject. Initialize it with the transform of the root object
                string targetName = targetTrans.name;                       // Initialize variable for storing name of "targetTrans"

                /* If rootName is a key in dictionary, update the
                 * "targetTrans" according to info from dictionary */
                if (specifyObjLevelDict.ContainsKey(rootName))
                {
                    targetTrans = AccessColliderInfo.GetNthLevelTrans(hitTrans, specifyObjLevelDict[rootName]);
                    targetName = targetTrans.name;
                }

                /* Generate a "nametag" to prevent a situation that two 
                 * different objects has the same name at Nth level*/
                string nametag = $"{rootName} || {targetName}";

                /* 1. Only collect objects which we want to detect 
                 * 2. Don't collect a same object twice */
                if (!objNotDetect.Contains(rootName) && !radarTargetName.Contains(nametag))
                {
                    /* Adding the nametag list to prevent duplicated recording transform */
                    radarTargetName.Add(nametag);

                    /* Get the clock direction of object's hit point to user's head */
                    Vector3 targetHitPoint = hitInfo.hitPoint;
                    float clockDirObjToHead = RelativePositionHelper.GetClockDirection(headTrans.position, targetHitPoint, headTrans.forward);
                    string strClockDirObjToHead = RelativePositionHelper.GetSimpleAndPrettyClockDirection(headTrans.position, targetHitPoint, headTrans.forward);

                    /* Add the target object transform & clock direction to the list */
                    radarTargets.Add(new RadarTarget
                    {
                        trans = targetTrans,
                        hitPoint = targetHitPoint,
                        clockDir = clockDirObjToHead,
                        strClockDir = strClockDirObjToHead,
                        numForSort = RegularizeClockDir(9f, clockDirObjToHead)
                    });
                }
            }

            /* Sort the target object list by their clock direction relative to avatar's head */
            List<RadarTarget> sortedRadarTargets = radarTargets.OrderBy(x => x.numForSort).ToList();
            radarTargets = sortedRadarTargets;
        }
    }


    /// <summary>
    /// Function speak the name of objects around and their direction relative to user's head.
    /// This is only a "Backup" plan for showing user the information around them.
    /// We prefer using "PlayObjsAround()" function.
    /// </summary>
    void SpeakObjsAround()
    {
        if (radarTargets.Count != 0)
        {
            /* Construct message */
            int SIZE = radarTargets.Count;
            string message = $"Objects within {GetRadarRange()}.";

            for (int i = 0; i < SIZE; ++i)
                message += $"{radarTargets[i].trans.name}, {radarTargets[i].strClockDir} o'clock. ";

            /* Speak the message */
            Debug.Log(message);
            verbalManager_General.Speak(message);
        }
        else
        {
            HandleNoObjectWithinRadar();
        }
    }


    /// <summary>
    /// Function plays 3D music & speak the name of objects around
    /// </summary>
    async void PlayObjsAround()
    {
        if (radarTargets.Count != 0)
        {
            /* [Important Note]
             * We use "enterId" and "currPlayingId" to managing the process created
             * by this async "PlayObjsAround()" function. "enterId" is generated at once
             * entering this function, and it's same as the "currPlayingId" by then. 
             * Each time when users do a new "reverse head nod", we will update the "currPlayingId"
             * ===> so the "currPlayingId" and "enterId of this process" are not same anymore
             * ===> Then, the logic below will ensure actions within this process will stop
             */

            /* Record the PlayingID when jumpping into this async function */
            int enterId = currPlayingId;

            /* Number of targets to play */
            int SIZE = radarTargets.Count;

            /* Speak the opening message */
            string startMessage = $"Objects within {GetRadarRange()}.";
            verbalManager_General.Speak("");                                 // This is not a best way, but we used this line of code to temporary avoid "startMessage" not speak out issue related to UAP process...
            verbalManager_General.Speak(startMessage);

            /* Play 3D music and speak object name one-by-one */
            for (int i = 0; i < SIZE; ++i)
            {
                /* Get the target and the hitting position */
                RadarTarget target = radarTargets[i];
                Vector3 hitPos = target.hitPoint;

                /* ------ If audio is ongoing, wait for it then speak about object ------ */
                while (musicPlayerManager.IsSpaAudioPlaying())
                    await Task.Yield();

                if (currPlayingId != enterId || currentScene != SceneManager.GetActiveScene().name)
                    return;

                string objMessage = $"{radarTargets[i].trans.name}";
                verbalManager_General.Speak(objMessage);

                /* ------ If speech is ongoing, wait for it and play directional music ------ */
                while (await verbalManager_General.IsSeaking_Improved())
                    await Task.Yield();

                if (currPlayingId != enterId || currentScene != SceneManager.GetActiveScene().name)
                    return;

                if (musicPlayerManager)
                {
                    musicPlayerManager.TransportMusicPlayer(hitPos);
                    musicPlayerManager.PlaySpaAudio(directionMusicName);
                }
            }
        }
        else
        {
            HandleNoObjectWithinRadar();
        }
    }


    /// <summary>
    /// Function regularize the clock direction based on "clockCenter" user provided.
    /// Usually, the clock center is 12/0 o'clock. In this function, you can set 9 o'clock as the center, 
    /// which means 0 o'clock. then the 8 o'clock becomes 11 o'clock, and 10 o'clock becomes 1 o'clock, etc... 
    /// </summary>
    private float RegularizeClockDir(float clockCenter, float clockDir)
    {
        float clockDiff = clockDir - clockCenter;
        if (clockDiff < 0)
            return 12 + clockDiff;

        return clockDiff;
    }


    /// <summary>
    /// Function returns radar's range based on different measurement system
    /// </summary>
    private string GetRadarRange()
    {
        if (SettingsMenu.measureSystem == "US")
            return radarRange_Us;
        else if (SettingsMenu.measureSystem == "Imperial")
            return radarRange_Imp;
        else
            return "";
    }


    /// <summary>
    /// Function handles the case that no object is within the radar's range
    /// </summary>
    private void HandleNoObjectWithinRadar()
    {
        string message = $"No object within your {GetRadarRange()}!";
        Debug.Log(message);
        verbalManager_General.Speak(message);
    }

}


/// <summary>
/// The class which holds the transform of hitted objects
/// and their clock angle to avatar's head
/// </summary>
public class RadarTarget
{
    public Transform trans { get; set; }                    // The transform of hitted object (at target level)
    public Vector3 hitPoint { get; set; }                   // The 3D point of the objects which hitted by the radar
    public float clockDir { get; set; }                     // The raw number clock direction from hitted object to avatar's head
    public string strClockDir { get; set; }                 // The string version of simple & pretty clock direction
    public float numForSort { get; set; }                   // A number that used for sorting the list 
}




























//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Linq;
//using System.Threading.Tasks;
//using System;
//using UnityEngine.SceneManagement;


//public class SurroundInfoRetriever : MonoBehaviour
//{
//    static public bool allowRetriever = true;               // [Default = true] If the system allow the surround info retriever to be used (User will set up this in the settings menu)
//    public bool useRetriever = false;                       // Do we use this SurroundInfoRetriever?

//    string currentScene;                                    // Record the name of current scene
//    string directionMusicName = "MsAndOverHereDirectionSound";    // The name of the directional music to play

//    string radarRange_Imp = "2 meters";                     // Range (radius) of the radar in meter
//    string radarRange_Us = "6 and half feet";               // Range (radius) of the radar in feet

//    SurroundRadar surroundRadar;                            // An reference to the SurroundRadar class ===> the class detects all objects hitted by the "SurroundRadar" gameObject
//    HeadNodDetector headNodDetector;                        // A class detects the head-nodding motion
//    VerbalManager_General verbalManager_General;            // The class controls text to speech
//    MusicPlayerManager musicPlayerManager;                  // The class which manages the MusicPlayer on user prefab
//    Transform headTrans;                                    // The transform of user's head

//    int currPlayingId = 0;                                   // ID which help with managing ongoing "PlayObjAround()" process  

//    List<RadarTarget> radarTargets = new();                 // List stores HitInfo of all meaningful objects hitted by radar
//    List<string> radarTargetName = new();                   // The name of target objects hitted by radar

//    /* The objects we don't want to detect in this class */
//    public List<string> objNotDetect = new List<string>()
//    { "Main Camera", "Directional Light", "Floor", "User", "SoundBall", "MusicPlayer",
//      "Accessibility Manager", "AR Session Origin", "AR Session", "NavigationSystem",
//      "SettingManager", "SceneMenuManager", "InstructionSystem", "SceneManager"};

//    /* The dictionary stores the object root name, and the hierarchy 
//     * level of that object we want to read once hit it */
//    public Dictionary<string, int> specifyObjLevelDict = new()
//    { { "Room", 2 } };


//    /// <summary>
//    /// Awake function is the first function called in an Unity execution loop
//    /// </summary>
//    private void Awake()
//    {
//        InitVariables();
//    }


//    /// <summary>
//    /// Update is called once per frame
//    /// </summary>
//    void Update()
//    {
//        /* If "ReverseHeadNod" happended */
//        if (allowRetriever && useRetriever && (Input.GetKeyDown(KeyCode.Z) || headNodDetector.ReverseHeadNodded))
//        {
//            /* Doing tasks below to stop the last ongoing "PlayObjsAround()" if any */
//            StopCurrPlayObjsAround();

//            /* Collect objects in radar and show them using audio + speech */
//            CollectObjectFromSurroundRadar();
//            PlayObjsAround();
//            //SpeakObjsAround();
//        }
//    }


//    /// <summary>
//    /// Funtion attempts to stop the current on-going "PlayObjsAround()" async process
//    /// </summary>
//    public void StopCurrPlayObjsAround()
//    {
//        currPlayingId++;
//        musicPlayerManager.StopSpaAudio();
//    }


//    /// <summary>
//    /// Function initializes all important member variables
//    /// </summary>
//    void InitVariables()
//    {
//        surroundRadar = transform.GetComponent<SurroundRadar>();
//        headNodDetector = GameObject.Find("User/Head").GetComponent<HeadNodDetector>();
//        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
//        headTrans = GameObject.Find("User/Head").transform;
//        musicPlayerManager = GameObject.Find("User/MusicPlayer").GetComponent<MusicPlayerManager>();

//        currentScene = SceneManager.GetActiveScene().name;
//    }


//    /// <summary>
//    /// Function gets all meaningful objects that are within the range of radar
//    /// </summary>
//    void CollectObjectFromSurroundRadar()
//    {
//        /* Clear the existing data from the list */
//        radarTargets.Clear();
//        radarTargetName.Clear();

//        /* Get info of all objects hitted by radar */
//        List<HitInfo> hits = surroundRadar.WithinRadar;

//        if (hits.Count != 0)
//        {
//            foreach (HitInfo hitInfo in hits)
//            {
//                Transform hitTrans = hitInfo.trans;                         // Transform of hitted gameObject
//                string rootName = hitTrans.root.name;                       // The root name of hitted gameObject

//                Transform targetTrans = hitTrans.root;                      // The transform of the target level of gameObject. Initialize it with the transform of the root object
//                string targetName = targetTrans.name;                       // Initialize variable for storing name of "targetTrans"

//                /* If rootName is a key in dictionary, update the
//                 * "targetTrans" according to info from dictionary */
//                if (specifyObjLevelDict.ContainsKey(rootName))
//                {
//                    targetTrans = AccessColliderInfo.GetNthLevelTrans(hitTrans, specifyObjLevelDict[rootName]);
//                    targetName = targetTrans.name;
//                }

//                /* Generate a "nametag" to prevent a situation that two 
//                 * different objects has the same name at Nth level*/
//                string nametag = $"{rootName} || {targetName}";

//                /* 1. Only collect objects which we want to detect 
//                 * 2. Don't collect a same object twice */
//                if (!objNotDetect.Contains(rootName) && !radarTargetName.Contains(nametag))
//                {
//                    /* Adding the nametag list to prevent duplicated recording transform */
//                    radarTargetName.Add(nametag);

//                    /* Get the clock direction of object's hit point to user's head */
//                    Vector3 targetHitPoint = hitInfo.hitPoint;
//                    float clockDirObjToHead = RelativePositionHelper.GetClockDirection(headTrans.position, targetHitPoint, headTrans.forward);
//                    string strClockDirObjToHead = RelativePositionHelper.GetSimpleAndPrettyClockDirection(headTrans.position, targetHitPoint, headTrans.forward);

//                    /* Add the target object transform & clock direction to the list */
//                    radarTargets.Add(new RadarTarget
//                    {
//                        trans = targetTrans,
//                        hitPoint = targetHitPoint,
//                        clockDir = clockDirObjToHead,
//                        strClockDir = strClockDirObjToHead,
//                        numForSort = RegularizeClockDir(9f, clockDirObjToHead)
//                    });
//                }
//            }

//            /* Sort the target object list by their clock direction relative to avatar's head */
//            List<RadarTarget> sortedRadarTargets = radarTargets.OrderBy(x => x.numForSort).ToList();
//            radarTargets = sortedRadarTargets;
//        }
//    }


//    /// <summary>
//    /// Function speak the name of objects around and their direction relative to user's head.
//    /// This is only a "Backup" plan for showing user the information around them.
//    /// We prefer using "PlayObjsAround()" function.
//    /// </summary>
//    void SpeakObjsAround()
//    {
//        if (radarTargets.Count != 0)
//        {
//            /* Construct message */
//            int SIZE = radarTargets.Count;
//            string message = $"Objects within {GetRadarRange()}.";

//            for (int i = 0; i < SIZE; ++i)
//                message += $"{radarTargets[i].trans.name}, {radarTargets[i].strClockDir} o'clock. ";

//            /* Speak the message */
//            Debug.Log(message);
//            verbalManager_General.Speak(message);
//        }
//        else
//        {
//            HandleNoObjectWithinRadar();
//        }
//    }


//    /// <summary>
//    /// Function plays 3D music & speak the name of objects around
//    /// </summary>
//    async void PlayObjsAround()
//    {
//        if (radarTargets.Count != 0)
//        {
//            /* [Important Note]
//             * We use "enterId" and "currPlayingId" to managing the process created
//             * by this async "PlayObjsAround()" function. "enterId" is generated at once
//             * entering this function, and it's same as the "currPlayingId" by then. 
//             * Each time when users do a new "reverse head nod", we will update the "currPlayingId"
//             * ===> so the "currPlayingId" and "enterId of this process" are not same anymore
//             * ===> Then, the logic below will ensure actions within this process will stop
//             */

//            /* Record the PlayingID when jumpping into this async function */
//            int enterId = currPlayingId;

//            /* Number of targets to play */
//            int SIZE = radarTargets.Count;

//            /* Speak the opening message */
//            string startMessage = $"Objects within {GetRadarRange()}.";
//            verbalManager_General.Speak(startMessage);

//            /* Play 3D music and speak object name one-by-one */
//            for (int i = 0; i < SIZE; ++i)
//            {
//                /* Get the target and the hitting position */
//                RadarTarget target = radarTargets[i];
//                Vector3 hitPos = target.hitPoint;

//                /* ------ If audio is ongoing, wait for it then speak about object ------ */
//                while (musicPlayerManager.IsSpaAudioPlaying())
//                    await Task.Yield();

//                if (currPlayingId != enterId || currentScene != SceneManager.GetActiveScene().name)
//                    return;

//                string objMessage = $"{radarTargets[i].trans.name}";
//                verbalManager_General.Speak(objMessage);

//                /* ------ If speech is ongoing, wait for it and play directional music ------ */
//                while (await verbalManager_General.IsSeaking_Improved())
//                    await Task.Yield();

//                if (currPlayingId != enterId || currentScene != SceneManager.GetActiveScene().name)
//                    return;

//                if (musicPlayerManager)
//                {
//                    musicPlayerManager.TransportMusicPlayer(hitPos);
//                    musicPlayerManager.PlaySpaAudio(directionMusicName);
//                }
//            }
//        }
//        else
//        {
//            HandleNoObjectWithinRadar();
//        }
//    }


//    /// <summary>
//    /// Function regularize the clock direction based on "clockCenter" user provided.
//    /// Usually, the clock center is 12/0 o'clock. In this function, you can set 9 o'clock as the center, 
//    /// which means 0 o'clock. then the 8 o'clock becomes 11 o'clock, and 10 o'clock becomes 1 o'clock, etc... 
//    /// </summary>
//    private float RegularizeClockDir(float clockCenter, float clockDir)
//    {
//        float clockDiff = clockDir - clockCenter;
//        if (clockDiff < 0)
//            return 12 + clockDiff;

//        return clockDiff;
//    }


//    /// <summary>
//    /// Function returns radar's range based on different measurement system
//    /// </summary>
//    private string GetRadarRange()
//    {
//        if (SettingsMenu.measureSystem == "US")
//            return radarRange_Us;
//        else if (SettingsMenu.measureSystem == "Imperial")
//            return radarRange_Imp;
//        else
//            return "";
//    }


//    /// <summary>
//    /// Function handles the case that no object is within the radar's range
//    /// </summary>
//    private void HandleNoObjectWithinRadar()
//    {
//        string message = $"No object within your {GetRadarRange()}!";
//        Debug.Log(message);
//        verbalManager_General.Speak(message);
//    }

//}


///// <summary>
///// The class which holds the transform of hitted objects
///// and their clock angle to avatar's head
///// </summary>
//public class RadarTarget
//{
//    public Transform trans { get; set; }                    // The transform of hitted object (at target level)
//    public Vector3 hitPoint { get; set; }                   // The 3D point of the objects which hitted by the radar
//    public float clockDir { get; set; }                     // The raw number clock direction from hitted object to avatar's head
//    public string strClockDir { get; set; }                 // The string version of simple & pretty clock direction
//    public float numForSort { get; set; }                   // A number that used for sorting the list 
//}

