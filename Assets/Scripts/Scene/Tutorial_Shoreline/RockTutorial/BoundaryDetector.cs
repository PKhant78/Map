using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryDetector : MonoBehaviour
{
    BoundaryManager boundaryManager;                          // The BoundaryManager class 


    /// <summary>
    /// Awake is the 1st function been called in an Unity execution loop
    /// </summary>
    private void Awake()
    {
        /* Instantiate a reference to the BoundaryManager class on Boundary object */
        boundaryManager = transform.parent.GetComponent<BoundaryManager>();
    }


    /// <summary>
    /// Function checks if cane stays in contact with a side of boundary
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        Collider other = collision.collider;                  // The collider object which a side of boundary collides with

        if (other.name == "Cane")
            boundaryManager.BoundarySlided = true;
    }


    /// <summary>
    /// Function checks if cane leave a side of boundary
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        Collider other = collision.collider;                  // The collider object which a side of boundary collides with                 

        if (other.name == "Cane")
            boundaryManager.BoundarySlided = false;
    }

}

