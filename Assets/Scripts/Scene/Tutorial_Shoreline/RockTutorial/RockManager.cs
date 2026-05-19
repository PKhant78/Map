using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockManager : MonoBehaviour
{
    GameObject rockInside;                  // A reference to the inside part of Rock

    // Update is called once per frame
    void Start()
    {
        /* Find the Inside Part and assign script to it */
        rockInside = transform.Find("Inside").gameObject;
        rockInside.AddComponent<RockInsideDetector>();
    }
}

