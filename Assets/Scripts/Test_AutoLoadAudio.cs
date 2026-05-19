//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;
//using UnityEngine.Networking;




//public class Test_AutoLoadAudio : MonoBehaviour
//{

//    public List<AudioClip> audioClips = new List<AudioClip>();

//    public AudioSource audioSource;
//    public AudioClip audioClip;
//    public string soundPath;

//    int loadMax = 0;
//    int loadCount = 0;
//    bool loadFinish = false;



//    // Start is called before the first frame update
//    void Start()
//    {

//        audioSource = GameObject.Find("SoundBall").GetComponent<AudioSource>();

//        /* path of all the material audioClips (both .wav and .wav.meta are there...)*/
//        string path = Application.streamingAssetsPath.Replace("/StreamingAssets", "") + "/Sounds/Feedbacks/Material";
//        string[] clipPaths = Directory.GetFiles(path);

//        /* count number of .wav files, this is total num of files we want to load */
//        foreach (string clipPath in clipPaths)
//        {
//            if (clipPath.EndsWith(".wav"))
//                loadMax++;
//        }

//        /* load the audioClips into list */
//        foreach (string clipPath in clipPaths)
//        {
//            if (clipPath.EndsWith(".wav"))
//            {
//                soundPath = "file://" + clipPath;
//                StartCoroutine(LoadAudio());
//            }
//        }

//    }


//    private IEnumerator LoadAudio()
//    {
//        WWW request = GetAudioFromFile(soundPath);
//        yield return request;

//        audioClip = request.GetAudioClip();
//        audioClip.name = "Halaluya";            // need to change the name...

//        audioClips.Add(audioClip);
//        loadCount++;                            // track the number of audioClips loaded
//    }


//    private WWW GetAudioFromFile(string soundPath)
//    {
//        WWW request = new WWW(soundPath);
//        return request;
//    }


//    private void PlayAudioFile(int idx)
//    {
//        if (!audioSource.isPlaying)
//            audioSource.PlayOneShot(audioClips[idx]);
//    }


//    // Update is called once per frame
//    void Update()
//    {
//        //Debug.Log("Max: " + loadMax + ", Count: " + loadCount);

//        ///* We only play audioClips from the list after loading couroutin is finished */
//        //if (loadCount == loadMax)
//        //{
//        //    Debug.Log(audioClips.Count);
//        //    PlayAudioFile(11);
//        //}
//    }



//}
