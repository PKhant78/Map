using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class MaterialLearnManager : MonoBehaviour
{
    /* groupSize = number material learning balls int the scene.
     * It stands for the max number of material we want the user to 
     * learn in each individual round/page (before user swipe) */
    int groupSize = 2;

    /* A list stores names of material feedback for user to learn. 
     * eg. Wood, Stone, WoodFloor, StoneTactilePaving */
    List<string> materialFbNames = new List<string>();

    /* A list stores names of functional feedback, and we don't 
     * want user to learn these feedbacks, so we will filter them */
    List<string> functionalFbNames = new List<string>()
        { "VibrationSlow", "VibrationFast",
          "NextButton", "PrevButton" };

    /* The index of the last material name which is used (from "objMatNames") */
    int idxLastUsed = -1;

    /* Number of materials in one group
     * The number is between 1 to groupSize */
    int numInGroup = 0;

    /* Latest name of MatLearnBalls (Material Learning Balls) in the scene.
     * The name of Material Learn Ball will be changed dynamically to support
     * using UAP to read the material names for different group of materials */
    List<string> latestMatLearnBallNames = new List<string>();

    /* Track the No. of page/group of material the user is currently at */
    int currentPage = 0;

    /* Boolean to track if the scene just get started */
    bool sceneStart = true;

    /* This class support TextToSpeech function from UAP package */
    VerbalManager_General verbalManagerG;



    // <summary>
    // In Start Function, we mainly do:
    // 1. Get all Material Names we want user to learn
    // 2. Initialize the 1st group of materials in the MatLearnBalls
    // </summary>
    void Start()
    {
        /* Initialize the verbalManager_scene */
        verbalManagerG = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();

        /* Find names for all the materials feedback for user to learn */
        FindMatFbNames();

        /* Setup names and tags for Material Learning Balls for the 1st time */
        initMatLearnBalls();
    }


    // <summary>
    // Function finds all the name of materials feedback which we want the user
    // to learn. All the names get from the Sound Ball object
    // </summary>
    void FindMatFbNames()
    {
        /* Will get all material feedback names from Sound
         * Ball object, because all of them are added to
         * the "Feedback Audio List" of Sound Ball*/
        AudioManager_Feedback audioManagerFb =
            GameObject.Find("SoundBall").GetComponent<AudioManager_Feedback>();

        /* Besides the "mateiral feedback" audio, the sound ball 
         * also stores "functional feedback" audio. We filters 
         * out the "functional feedback" audio, because we don't
         * want user to learn about these */
        foreach (AudioClip audio in audioManagerFb.feedbackAudioList)
        {
            string material = audio.name.Split(" ")[0];                   
            if (!functionalFbNames.Contains(material) && !materialFbNames.Contains(material))
                materialFbNames.Add(material);             
        }
    }


    // <summary>
    // Function initialize the name and tag for Material Learning Balls in the scene
    // </summary>
    void initMatLearnBalls()
    {
        /* Initialize the list with default MatLearnBall name in the scene */
        for (int i = 0; i < groupSize; ++i)
            latestMatLearnBallNames.Add("MatLearnBall" + (i + 1));

        /* Runing NextMatGroup() function to initiate the 1st group of MatLearnBall */
        NextMatGroup();
    }


    // <summary>
    // Function for switching the materials show in MatLearnBalls to next group of materials.
    // Will be called by "NextButton" GameObjects.
    // </summary>
    public void NextMatGroup()
    {
        /* Check if exist another group of materials or not */
        if (idxLastUsed + 1 >= materialFbNames.Count)                      // if no more material after "idxLastUsed"
        {
            string notification = "No more next page!";
            Debug.Log(notification);
            verbalManagerG.Speak(notification);
            return;
        }
        else                                                               // if exist material in the next group
        {
            /* Switching to a new group of materials, so reset numInGroup to 0 */
            numInGroup = 0;

            /* If exist another group of materials, update them to MatLearnBalls. 
             * If "# of materials in new group" < "groupSize", some matLearnBall
             * will be empty. No feedback will be provided from those balls
             */
            for (int i = 0; i < groupSize; ++i)
            {
                /* Get one material learning ball */
                GameObject tempMatLearnBall = GameObject.Find(latestMatLearnBallNames[i]);

                /* Update name and tag for this material learning ball */
                if (idxLastUsed + 1 < materialFbNames.Count)                // if next material available 
                {
                    tempMatLearnBall.tag = materialFbNames[++idxLastUsed];  // update the tag
                    tempMatLearnBall.name = (i + 1) + ", "                  // update the name 
                        + tempMatLearnBall.tag;   

                    latestMatLearnBallNames[i] = tempMatLearnBall.name;     // update name used for MatLearnBall

                    numInGroup++;                                           // update the number of materials in the group
                }
                else                                                        // if no more material 
                {
                    tempMatLearnBall.name = (i + 1) + ", Empty" +           // update the name with placeholder
                        new string(' ', i + 1);         
                    tempMatLearnBall.tag = "NoMaterial";                    // update the tag with placeholder

                    latestMatLearnBallNames[i] = tempMatLearnBall.name;     // update name used for MatLearnBall
                }
            }

            /* Update the No. of page which user is currently at */
            currentPage++;

            /* Print and Speak the Page/Group of material if its not the beginning of scene */
            if (sceneStart)
                sceneStart = false;
            else
            {
                string notification = "Next, Page " + currentPage;
                Debug.Log(notification);
                verbalManagerG.Speak(notification);
            }


        }
    }


    // <summary>
    // Function for switching the materials show in MatLearnBalls to previous group of materials.
    // Will be called by "PrevButton" GameObjects.
    // </summary>
    public void PrevMatGroup()
    {
        /* Check if exist previous group of materials or not */
        if (idxLastUsed - numInGroup < 0)                                 // if no more material before the 1st element in a group   
        {
            string notification = "No more previous page!";
            Debug.Log(notification);
            verbalManagerG.Speak(notification);
            return;
        }
        else                                                              // if previous page is available
        {
            /* Based on our logic, as long as there exist a previous group of materials,
             * the size of previous group must be the max "groupSize". So, we only
             * apply some easy logic to the code below. */

            idxLastUsed -= numInGroup;                                    // update "idxLastUsed" for previous group
            int idxGroupBegin = idxLastUsed - groupSize + 1;              // get the begin idx of previous group based on its idxLastUsed
            numInGroup = groupSize;                                       // for the previous group, the "numInGroup" = max "groupSize"

            /* If exist previous group of materials, update them to MatLearnBalls */
            for (int i = 0; i < groupSize; ++i)
            {
                /* Get one material learning ball */
                GameObject tempMatLearnBall = GameObject.Find(latestMatLearnBallNames[i]);

                /* Update name and tag for this material learning ball */
                tempMatLearnBall.tag = materialFbNames[idxGroupBegin++];  // update the tag
                tempMatLearnBall.name = (i + 1) + ", "                    // update the name 
                    + tempMatLearnBall.tag;


                latestMatLearnBallNames[i] = tempMatLearnBall.name;       // update name used for MatLearnBall
            }

            /* Update the No. of page which user is currently at */
            currentPage--;

            /* Print and Speak the Page/Group of material if its not the beginning of scene */
            string notification = "Previous, Page " + currentPage;
            Debug.Log(notification);
            verbalManagerG.Speak(notification);
        }

    }



}
