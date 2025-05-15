using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



public class GenerateEmbeddedObject : MonoBehaviour
{
    /// <summary>
    /// The padding size (in Meter) between "Inside" and "Outside" on one axis.
    /// padValue is the sum of padding on both side of an axis.
    /// padValue will be used when "size >= sizeForUsingPct".
    /// Default: 0.3f
    /// </summary>
    float padValue = 0.3f;

    /// <summary>
    /// X% value which will be used to calculate the padding size (in Meter) between "Inside" and "Outside" objects.
    /// padPct will be used when "size <= sizeForUsingPct"
    /// Default: 0.8f
    /// </summary>
    float padPct = 0.8f;

    /// <summary>
    /// A threshold value: if size below it, use "padPct", and if not, use "padValue"
    /// Default: 0.2f
    /// </summary>
    float sizeForUsingPct = 0.2f;

    /// <summary>
    /// A dictionary store the standard Actual-Size (in Meter) of different types of collider
    /// </summary>
    Dictionary<string, Vector3> dictStdActualSize = new Dictionary<string, Vector3>()
    {
        { "Cube", new Vector3(1f, 1f, 1f)},
        { "Sphere", new Vector3(1f, 1f, 1f)},
        { "Capsule", new Vector3(1f, 2f, 1f)},
        { "Cylinder", new Vector3(1f, 2f, 1f)},
    };



    /// <summary>
    /// Generate all the embedded structures at the time the App awake 
    /// </summary>

    void Awake()
    {
        RecGenerateForOutsideObjs(transform);
    }


    /// <summary>
    /// A Recursive function for finding all descendents named "Outside" for current obj
    /// and generate embedded structure for all the obj parts.
    /// </summary>

    void RecGenerateForOutsideObjs(Transform currObj)
    {
        if (currObj.name == "Outside")
            GenerateForOneOutsideObj(currObj);
        else if (currObj.childCount == 0)
            return;
        else
        {
            foreach (Transform child in currObj)
                RecGenerateForOutsideObjs(child);
        }
    }


    /// <summary>
    /// Function generates "inside obj" for one "outside obj" to create the embedded structure
    /// </summary>

    void GenerateForOneOutsideObj(Transform outsideObj)
    {
        /* Step 1. Make a copy of "Outside" Obj -> name it "Inside" */
        Transform insideObj = Instantiate(outsideObj);                  // Duplicate the "OutsideObj" and name it as "InsideObj"
        insideObj.name = "Inside";

        /* Step 2. Move the "Outside" Obj to the same position as "Outside" Obj */
        insideObj.SetParent(outsideObj.transform);                      // Temporarily set the outsideObj as the parent of insideObj
        insideObj.localPosition = Vector3.zero;                         // Sync the insideObj to the position of OutsideObj

        /* Step 3. Set the sacle for OutsideObj */
        SetInsideScale(ref insideObj, ref outsideObj);                  // Set scale for the Inside object

        /* Step 4. Sync the Position and Rotaion (EulerAngles) */
        insideObj.localPosition = outsideObj.localPosition;
        insideObj.localEulerAngles = outsideObj.localEulerAngles;
    }


    /// <summary>
    /// Function for setting scale for generated inside object
    /// </summary>

    protected virtual void SetInsideScale(ref Transform insideObj, ref Transform outsideObj)
    {
        Vector3 outActualSize = GetActualSize(outsideObj);              // Actual-Size (in Meter) of the Outside object 
        Vector3 outLocalScale = outsideObj.localScale;                  // Local-Scale (in Unity unit) of the Outside object
        Vector3 inLocalScale = new Vector3(ScaleCalc(outActualSize.x, outLocalScale.x),
            ScaleCalc(outActualSize.y, outLocalScale.y),
            ScaleCalc(outActualSize.z, outLocalScale.z));
        insideObj.SetParent(outsideObj.parent);                         // Set the parent of "OutsideObj" to be the parent of "InsideObj", so that they become parallel objects
        insideObj.localScale = inLocalScale;
    }


    /// <summary>
    /// Function gets Actual-Size of an object (in Meter)
    /// </summary>

    protected Vector3 GetActualSize(Transform obj)
    {
        /* Get the shape type of the object (Cube, Sphere, Cylinder, Capsule) */
        string objShapeType = obj.GetComponent<MeshFilter>().mesh.name.Replace(" Instance", "");

        /* Get the standard Actual-Size of the current object based on its shape type */
        Vector3 stdActualSize = dictStdActualSize[objShapeType];

        /* Get scale of current object */
        Vector3 scale = obj.localScale;
        Transform tempTrans = obj.transform;
        while (tempTrans != tempTrans.root)
        {
            scale = Vector3.Scale(scale, tempTrans.parent.localScale);  // Multiply two vectors
            tempTrans = tempTrans.parent;
        }

        /* Transform the scale of this object to Actual-Size (in Meter) */
        Vector3 objActualSize = Vector3.Scale(scale, stdActualSize);

        return objActualSize;
    }


    /// <summary>
    /// A calculator return an appropraite size for an axis of Inside object,
    /// given the size of one axis of an Outside object. 
    /// </summary>

    protected float ScaleCalc(float oldActualSize, float oldLocalScale)
    {
        float newLocalScale = -1f;

        if (oldActualSize <= sizeForUsingPct)                              // If the size is too small
        {
            newLocalScale = oldLocalScale * (1 - padPct);
        }
        else                                                               // If the size big enough
        {
            float ratio = (oldActualSize - padValue) / oldActualSize;      // ratio of "new size" / "old size"
            newLocalScale = oldLocalScale * ratio;
        }
        return newLocalScale;
    }


}