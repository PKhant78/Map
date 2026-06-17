using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectUserContact : MonoBehaviour
{
    bool hitByUser = false;                                   // if the object is hitted by the user or not


    /// <summary>
    /// Function checks if the object is hitted by user's body
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;                  // The collider object which the object collided with

        if (other.transform.root.name == "User" && other.name == "Body")
            hitByUser = true;
    }

    /// <summary>
    /// Getter of "hitByUser" bool variable
    /// </summary>
    public bool HitByUser
    {
        get { return hitByUser; }
    }

}

