using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCaneContact : MonoBehaviour
{
    bool slidedByCane;                                        // if the cane slides on this object or not


    /// <summary>
    /// Function checks if cane stays in contact with the object which this script attached to
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        Collider other = collision.collider;                  // The collider object which this object collides with

        if (other.name == "Cane")
            slidedByCane = true;
    }


    /// <summary>
    /// Function checks if cane leave the object which this script attached to
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        Collider other = collision.collider;                  // The collider object which this object collides with                 

        if (other.name == "Cane")
            slidedByCane = false;
    }


    /// <summary>
    /// Getter of "slidedByCane" bool variable
    /// </summary>
    public bool SlidedByCane
    {
        get { return slidedByCane; }
    }

}

