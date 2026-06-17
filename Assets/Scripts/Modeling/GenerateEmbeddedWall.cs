using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GenerateEmbeddedWall : GenerateEmbeddedObject
{
    /// <summary>
    /// The only difference between "generating embedded object" vs. "generating embedded wall"
    /// is how they calculate the scale of the object (x,y,z)
    /// </summary>

    protected override void SetInsideScale(ref Transform insideObj, ref Transform outsideObj)
    {
        Vector3 outActualSize = GetActualSize(outsideObj);              // Actual-Size (in Meter) of the Outside object 
        Vector3 outLocalScale = outsideObj.localScale;                  // Local-Scale (in Unity unit) of the Outside object

        /* Find out the axis (x, y, z) which represents the wall thickness */
        int idxOfMin = 0;
        float minVal = outActualSize[idxOfMin];
        for (int i = 0; i < 3; ++i)
        { 
            if (outActualSize[i] < minVal)
            {
                minVal = outActualSize[i];
                idxOfMin = i;
            }
        }

        /* Only change the wall thickness for the Inside object (Don't change Height and Length) */
        Vector3 inLocalScale = outLocalScale;
        for (int i = 0; i < 3; ++i)
        {
            if (i == idxOfMin)
                inLocalScale[i] = ScaleCalc(outActualSize[i], outLocalScale[i]);
        }
        insideObj.SetParent(outsideObj.parent);                         // Set the parent of "OutsideObj" to be the parent of "InsideObj", so that they become parallel objects
        insideObj.localScale = inLocalScale;

    }

}
