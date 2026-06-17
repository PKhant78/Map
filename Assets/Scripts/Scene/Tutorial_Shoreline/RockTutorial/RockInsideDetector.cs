using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockInsideDetector : MonoBehaviour
{
    bool rockInsideSlided;                                    // if the cane slides on "rock inside" or not


    /// <summary>
    /// Function checks if cane stays in contact with "rock inside"
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        Collider other = collision.collider;                  // The collider object which the "rock inside" collides with

        if (other.name == "Cane")
            rockInsideSlided = true;
    }


    /// <summary>
    /// Function checks if cane leave the "rock inside"
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        Collider other = collision.collider;                  // The collider object which the "rock inside" collides with                 

        if (other.name == "Cane")
            rockInsideSlided = false;
    }


    /// <summary>
    /// Getter of "caneSlide" bool variable
    /// </summary>
    public bool RockInsideSlided
    {
        get { return rockInsideSlided; }
    }
}
