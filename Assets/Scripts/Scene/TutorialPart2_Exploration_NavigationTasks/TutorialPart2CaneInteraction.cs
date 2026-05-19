using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPart2CaneInteraction : MonoBehaviour
{
    VerbalManager_General verbalManager_General;

     void Start()
    {
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;                                                    // the collider object which the button collides with

        if (other.name == "Cane")                                                               // if the button hitted by cane
        {
            verbalManager_General.Speak("Your cane’ touch the exit door of the room, please step forward.");
         
        }

       
    }
}
