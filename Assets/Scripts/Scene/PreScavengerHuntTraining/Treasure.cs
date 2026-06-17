using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
 
    private void OnCollisionEnter(Collision collision)
    {
        Transform other = collision.collider.transform;                                         // the collider object which the button collides with

        if (other.name == "Cane")                                                               // if the button hitted by cane
        {
            if (AccessColliderInfo.GetRootName(transform) == "CardboardBox")             // user hit right option
            {
                Debug.Log("YOU FOUND CARDBOARD BOX");
            }
        }
    }
}
