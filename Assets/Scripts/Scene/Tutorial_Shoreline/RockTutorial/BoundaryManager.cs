using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    bool boundarySlided;                               // if the cane slides on a side of boundary or not


    /// <summary>
    /// Getter and Setter of "boundarySlided" bool variable
    /// </summary>
    public bool BoundarySlided
    {
        get { return boundarySlided; }
        set { boundarySlided = value; }
    }
}
