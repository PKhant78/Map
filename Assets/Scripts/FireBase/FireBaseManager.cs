//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Firebase.Extensions;
//using Firebase.Firestore;
//using UnityEngine;
//using UnityEngine.SceneManagement;

///**********************************************************************************************
// * ScriptName: FireBaseManager
// * Purpose: This script is used for Record the User prefab's positions, rotations, 
// *          the GripPoint prefab's rotation, and the head prefab's rotation data for TrajectoryReplay function.
// *          Request:
// *                  1. Create the empty object in Hierarchy named "Firebase manager," and attatch this script,
// *                      this is for standardize.
// *                  2. If you want the replay to become smooth, increase the value of SamplingRateHz in Hierarchy
// *                  
// *                  3. In order to use firebase function, import firebase_unity_skd/dotnet4/FirebaseFirestore.unitypackage
// * Developer: MRCane team
// * Last changed time: Wed July 27th 2022
// **********************************************************************************************/



//public class FireBaseManager : MonoBehaviour
//{
//    //---------------- General Variable ----------------//
//    float timer;                               // Save the local time value, when positions and rotations' data have been collected.
//    int key;                                   // Data ID in firebase database
//    string date;                               // Save local date value
//    string UserName = "Temporary";
//    Vector3 lastpos;                           // keep the value of user's last position
//    Quaternion lastrot;                        // keep the value of user's last rotation
//    public int SamplingRateHz = 20;             // 
//    Dictionary<string, object> trajectoryData; //
//    GameObject User;                           //
//    GameObject GripPoint;                      // Three Gameobject that need to be tracked
//    GameObject Head;                           //
//    //---------------- Firebase Component ----------------//
//    FirebaseFirestore db;


//    // Start is called before the first frame update
//    void Start()
//    {
//        //---------------- Add reference ----------------//
//        trajectoryData = new Dictionary<string, object>();
//        User = GameObject.Find("User");
//        GripPoint = GameObject.Find("User/GripPoint");
//        Head = GameObject.Find("User/Head");
//        db = FirebaseFirestore.DefaultInstance;

//        //---------------- Initialize variable ----------------//
//        date = System.DateTime.Now.ToString("yyyy_MM_dd-H:mm:ss"); //For DocumentID
//        key = 0;                                                   //For data ID
//        timer = 0f;
//        Debug.Log("SamplingRateHz: " + SamplingRateHz);
//        InvokeRepeating("logTrajectory", 1f, 1f / SamplingRateHz);

//        db = FirebaseFirestore.DefaultInstance;

//        SceneManager.sceneUnloaded += uploadDataOnLeftScene;

//        InvokeRepeating("UploadRecordedTrajectory", 15f, 120f);
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        timer += Time.deltaTime;
//    }

//    public void setUserName(string UserName)
//    {
//        if (UserName == "")
//            this.UserName = "Temporary";
//        else
//            this.UserName = UserName;
//    }


//    /// <summary>
//    /// Save all user's positions, rotations, GripPoint's rotations, and head's rotations to the array.
//    /// Once the current scene is destroyed(left or quit the application) the array will upload to the Firebase
//    /// </summary>
//    void logTrajectory()
//    {
//        if (User.transform.position == lastpos && User.transform.rotation == lastrot)
//            return;

//        string logLine = System.Math.Round(timer, 2) + ", " +
//        System.Math.Round(User.transform.position.x, 3) + ", " +
//        System.Math.Round(User.transform.position.y, 3) + ", " +
//        System.Math.Round(User.transform.position.z, 3) + ", " +
//        System.Math.Round(User.transform.rotation.eulerAngles.x, 3) + ", " +
//        System.Math.Round(User.transform.rotation.eulerAngles.y, 3) + ", " +
//        System.Math.Round(User.transform.rotation.eulerAngles.z, 3) + ", " +
//        System.Math.Round(GripPoint.transform.rotation.eulerAngles.x, 3) + ", " +
//        System.Math.Round(GripPoint.transform.rotation.eulerAngles.y, 3) + ", " +
//        System.Math.Round(GripPoint.transform.rotation.eulerAngles.z, 3) + ", " +
//        System.Math.Round(Head.transform.rotation.eulerAngles.x, 3) + ", " +
//        System.Math.Round(Head.transform.rotation.eulerAngles.y, 3) + ", " +
//        System.Math.Round(Head.transform.rotation.eulerAngles.z, 3);
//        trajectoryData.Add(key.ToString("000000000"), logLine);
//        key++;
//        lastpos = User.transform.position;
//        lastrot = GripPoint.transform.rotation;
//        Debug.Log("Userposition: " + User.transform.position);
//        Debug.Log("Userrotation: " + User.transform.rotation);
//        Debug.Log("Record! logTrajectory");
//        Debug.Log("trajectoryData1: " + trajectoryData.Count);
//    }

//    /// <summary>
//    /// upload data to the Firestore database.
//    /// Notice: The speed of uploading data to the Firestore database has limits, one request per second.
//    ///         If Deal with large data, better save them into data structures and upload together

//    /// </summary>
//    /// <param name="data"></param>
//    /// <param name="collection"></param>
//    /// <param name="doc"></param>
//    public void uploadData(Dictionary<string, object> data, string collection, string doc)
//    {
//        DocumentReference docRef = db.Collection(collection).Document(doc);

//        docRef.SetAsync(data).ContinueWithOnMainThread(task =>
//        {
//            Debug.Log("Added trajectory data to the document in the collection.");
//        });
//    }

//    /// <summary>
//    /// If the application is quit, upload all the data saved on the array to the Firestore database.
//    /// </summary>
//    private void OnApplicationQuit()
//    {
//        UploadRecordedTrajectory();
//    }

//    /// <summary>
//    /// Upload data
//    /// </summary>
//    public void UploadRecordedTrajectory()
//    {
//        Scene scene = SceneManager.GetActiveScene();
//        if (scene.name == "FormalExperiment" || scene.name == "ScavengerHunt")
//        {
//            uploadData(trajectoryData, "DataRecording", UserName + "-" + scene.name + "-" + date);
//            string DocumentID = UserName + "-" + date;
//            Debug.Log(DocumentID);
//        }
//    }

//    /// <summary>
//    /// If users leave the current scene, upload all the data saved on the array to the Firestore database.
//    /// </summary>
//    void uploadDataOnLeftScene<Scene>(Scene scene)
//    {
//        UploadRecordedTrajectory();
//        Debug.Log("trajectoryData2: " + trajectoryData.Count);
//    }

//}
