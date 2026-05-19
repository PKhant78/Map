//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Firebase.Firestore;
//using System.Threading.Tasks;
//using System.Linq;
//using System.Threading;

///**********************************************************************************************
// * ScriptName: TrajectoryReplay
// * Purpose: This script is used for replaying the scene activities based on the data saved in the firebase database.
// *          Request:
// *                  1. Change the value of Document ID in Hierarchy, the value should be same as Document ID in the firebase database
// * Developer: MRCane team
// * Last changed time: Wed July 27th 2022
// **********************************************************************************************/


//public class TrajectoryReplay : MonoBehaviour
//{

//    //---------------- General Variable ----------------//
//    [SerializeField]
//    string documentId;
//    float time;                                       // keep the local time, in order to compare with the time saved in
//    bool startTimer;                                  // The flag for time counter start
//    GameObject User;                                  //
//    GameObject GripPoint;                             // Three Gameobject that need to be tracked
//    GameObject Head;                                  //
//    protected string collectionPath = "DataRecording"; //name of the collection container that documents get stored into
//    bool operationInProgress;

//    //---------------- Firebase Component ----------------//
//    FirebaseFirestore db;

//    //---------------- Thread related Component ----------------//
//    Task previousTask;
//    CancellationTokenSource cancellationTokenSource;


//    /// <summary>
//    /// Get CollectionID
//    /// </summary>
//    /// <returns></returns>
//    private CollectionReference GetCollectionReference()
//    {
//        return db.Collection(collectionPath);
//    }

//    /// <summary>
//    /// Get DocumentID
//    /// </summary>
//    /// <returns></returns>
//    private DocumentReference GetDocumentReference()
//    {
//        return GetCollectionReference().Document(documentId);
//    }

//    private void Start()
//    {
//        //---------------- Add reference ----------------//
//        db = FirebaseFirestore.DefaultInstance;
//        Head = GameObject.Find("User/Head");
//        User = GameObject.Find("User");
//        GripPoint = GameObject.Find("User/GripPoint");
//        cancellationTokenSource = new CancellationTokenSource();
//        time = 0f;
//        startTimer = false;

//        //---------------- Start Replay Function ----------------//
//        StartCoroutine(ReadDoc(GetDocumentReference()));
//    }

//    private void Update()
//    {
//        if (startTimer)
//        {
//            time += Time.deltaTime;
//        }
//    }

//    /// <summary>
//    /// Get all the data from firestore database and save in the array, then change positions and rotations one by one.
//    /// </summary>
//    /// <param name="doc"></param>
//    /// <returns></returns>
//    protected IEnumerator ReadDoc(DocumentReference doc)
//    {
//        Task<DocumentSnapshot> getTask = doc.GetSnapshotAsync();

//        yield return new WaitForTaskCompletion(this, getTask);

//        if (!(getTask.IsFaulted || getTask.IsCanceled))
//        {
//            // Gets a snapshot of the Firestore database at the time this is called (start)

//            DocumentSnapshot snap = getTask.Result;

//            // converts snapshot to dictionary of <string, object>

//            IDictionary<string, object> resultData = snap.ToDictionary();

//            // converts dictionary<string,object> to dictionary<string,string>

//            Dictionary<string, string> dString = resultData.ToDictionary(k => k.Key, k => k.Value.ToString());

//            SortedDictionary<string, string> SortedDic = new SortedDictionary<string, string>(dString);

//            yield return new WaitForSeconds(3);

//            if (!startTimer)
//                startTimer = true;

//            Debug.Log("Data read from Firebase");




//            foreach (KeyValuePair<string, string> pair in SortedDic)
//            {
//                if (pair.Key != "-1: Placed Map Transform")
//                {
//                    string[] sAr = pair.Value.Split(',');
//                    Vector3 nextpos = new Vector3(
//                        float.Parse(sAr[1]),
//                        float.Parse(sAr[2]),
//                        float.Parse(sAr[3]));
//                    yield return new WaitUntil(() => time >= float.Parse(sAr[0]));

//                    Debug.Log("time: " + float.Parse(sAr[0]));

//                    User.transform.rotation = Quaternion.Euler(
//                       float.Parse(sAr[4]),
//                       float.Parse(sAr[5]),
//                       float.Parse(sAr[6]));

//                    GripPoint.transform.rotation = Quaternion.Euler(
//                        float.Parse(sAr[7]),
//                        float.Parse(sAr[8]),
//                        float.Parse(sAr[9]));

//                    Head.transform.rotation = Quaternion.Euler(
//                        float.Parse(sAr[10]),
//                        float.Parse(sAr[11]),
//                        float.Parse(sAr[12]));

//                    User.transform.position = nextpos;
//                }
//            }
//        }
//    }

//    /// <summary>
//    /// implement custom yield instructions to suspend coroutine execution until an event happens
//    /// </summary>
//    class WaitForTaskCompletion : CustomYieldInstruction
//    {
//        Task task;
//        TrajectoryReplay dbReader;
//        protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

//        // Create an enumerator that waits for the specified task to complete.
//        public WaitForTaskCompletion(TrajectoryReplay dbReader, Task task)
//        {
//            dbReader.previousTask = task;
//            dbReader.operationInProgress = true;
//            this.dbReader = dbReader;
//            this.task = task;
//        }

//        // Wait for the task to complete.
//        public override bool keepWaiting
//        {
//            get
//            {
//                if (task.IsCompleted)
//                {
//                    dbReader.operationInProgress = false;
//                    dbReader.cancellationTokenSource = new CancellationTokenSource();
//                    if (task.IsFaulted)
//                    {
//                        string s = task.Exception.ToString();
//                        Debug.Log(s);
//                    }
//                    return false;
//                }
//                return true;
//            }
//        }
//    }
//}
