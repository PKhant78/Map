using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorDetector : MonoBehaviour
{
    bool floorSlided;                                         // if the cane slides on floor or not


    /// <summary>
    /// Function checks if cane stays in contact with floor
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        Collider other = collision.collider;                  // The collider object which the floor collides with

        if (other.name == "Cane")
            floorSlided = true;
    }


    /// <summary>
    /// Function checks if cane leave the floor
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        Collider other = collision.collider;                  // The collider object which the floor collides with                 

        if (other.name == "Cane")
            floorSlided = false;
    }


    /// <summary>
    /// Getter of "caneSlide" bool variable
    /// </summary>
    public bool FloorSlided
    {
        get { return floorSlided; }
    }

}

