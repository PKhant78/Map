using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[RequireComponent(typeof(HeadCaster))]
[RequireComponent(typeof(HeadNodDetector))]

public class FrontInfoRetriever : MonoBehaviour
{
    static public bool allowRetriever = true;       // [Default = true] If the system allow the front info retriever to be used (User will set up this in the settings menu)
    public bool useRetriever = false;               // Do we use this FrontInfoRetriever?

    HeadCaster headCaster;                          // An reference to the HeadCaster class ===> the class cast an absolute front capsule along the facing direction
    HeadNodDetector headNodDetector;                // A class detects the head-nodding motion
    VerbalManager_General verbalManager_General;    // The class controls text to speech
    SurroundInfoRetriever surroundInfoRetriever;    // The class provides directional information about objects around. We need this class here, so we can stop it when needed 

    List<HitTarget> hitTargets = new();             // A list of "HitTarget" object. It stores the hitted object and their distance to avatar's head
    List<string> hitTargetName = new();             // The name of hitted target object

    readonly int maxObjsSpeakNum = 6;               // [Default = 6] The maximum number of objects to speaks out loud

    /* The objects we don't want to detect in this class*/
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


    // Update is called once per frame
    void Update()
    {
        if (allowRetriever && useRetriever && (Input.GetKeyDown(KeyCode.X) || headNodDetector.HeadNodded))
        {
            surroundInfoRetriever.StopCurrPlayObjsAround();     // Stop playing "surround objects info" if the user calls the front info retriever
            CollectObjectFromAbsFrontHit();                     // Collect the objects in front of user's head
            SpeakObjsInFront();                                 // Speak out loud the objects in front of user's head
        }
    }


    /// <summary>
    /// Function initializes all important member variables
    /// </summary>
    void InitVariables()
    {
        headCaster = transform.GetComponent<HeadCaster>();
        headNodDetector = transform.GetComponent<HeadNodDetector>();
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        surroundInfoRetriever = GameObject.Find("User/Body/SurroundRadar").GetComponent<SurroundInfoRetriever>();
    }


    /// <summary>
    /// Function gets all objects hitted by absolute front capsule
    /// </summary>
    void CollectObjectFromAbsFrontHit()
    {
        /* Clear the existing data from the list */
        hitTargets.Clear();
        hitTargetName.Clear();

        /* Get transforms of all hitted objects */
        RaycastHit[] hits = headCaster.AbsFrontCapsuleHits;
        if (hits != null)
        {
            for (int i = 0; i < hits.Length; ++i)
            {
                Transform hitTrans = hits[i].transform;                     // Transform of hitted gameObject
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
                 * 2. Don't collect a same object twice 
                 * 3. The hit point is not (0.00, 0.00, 0.00) ===> If colliders overlaps the sphere at the start of the sweep, the hitpoint will be set to ZERO, so we filter this out 
                 */
                if (!objNotDetect.Contains(rootName) && !hitTargetName.Contains(nametag) && hits[i].point != Vector3.zero)
                {
                    /* Adding the nametag list to prevent duplicated recording transform */
                    hitTargetName.Add(nametag);

                    /* Get the distance between head and hit point in both numeric and string format */
                    float distToHead = -1f;
                    string distToHeadStr = "None";
                    GetHeadToPosDist(hits[i].point, ref distToHead, ref distToHeadStr);

                    /* Record the target gameObjects and 2D distance between their hitted point and avatar's head */
                    hitTargets.Add(new HitTarget { trans = targetTrans, dist = distToHead, distStr = distToHeadStr });
                }
            }

            /* Sort the target object list by their distance to avatar's head */
            List<HitTarget> sortedHitTargets = hitTargets.OrderBy(x => x.dist).ToList();
            hitTargets = sortedHitTargets;
        }
    }


    /// <summary>
    /// Function speaks out loud the object hitted by absolute front capsule 
    /// </summary>
    void SpeakObjsInFront()
    {
        if (hitTargets.Count != 0)
        {
            /* Construct message */
            int SIZE = Mathf.Min(maxObjsSpeakNum, hitTargets.Count);
            string message = "Objects in your facing direction: ";

            for (int i = 0; i < SIZE; ++i)
                message += $"{hitTargets[i].trans.name}, {hitTargets[i].distStr}. ";

            /* Speak the message */
            Debug.Log(message);
            verbalManager_General.Speak(message);
        }
        else
        {
            /* Give notice when there's no objects */
            string message = "No object in your facing direction!";
            Debug.Log(message);
            verbalManager_General.Speak(message);
        }
    }


    /// <summary>
    /// Function gets the distance (normalized) from head to a 3D position in both "number" & "string" format
    /// Parameters "dist" and "distStr" are passed by reference ===> They will be modified accordingly
    /// </summary>
    void GetHeadToPosDist(Vector3 position, ref float dist, ref string distStr)
    {
        if (SettingsMenu.measureSystem == "US")
        {
            dist = RelativePositionHelper.GetDistanceNum(transform.position, position, digitsAfterDecimal: 0);
            distStr = RelativePositionHelper.GetDistance(transform.position, position, digitsAfterDecimal: 0);
        }
        else if (SettingsMenu.measureSystem == "Imperial")
        {
            dist = RelativePositionHelper.GetDistanceNum(transform.position, position, digitsAfterDecimal: 0, useFeet: false);
            distStr = RelativePositionHelper.GetDistance(transform.position, position, digitsAfterDecimal: 0, useFeet: false);
        }
    }

}


/// <summary>
/// The class which holds the transform of hitted objects
/// and their distance to our avatar's head 
/// </summary>
public class HitTarget
{
    public Transform trans { get; set; }                // The transform of hitted object
    public float dist { get; set; }                     // The raw number distance from head to hitted object
    public string distStr { get; set; }                 // The distance in string of head to hitted object
}

