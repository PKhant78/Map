/*****************************************************************/
/* Programmer: MRCane Development Team                           */
/* Date: July 26th, 2022                                         */
/* Class: HeadNodDetector                                        */
/* Purpose:                                                      */
/* A class designed to detect everything hitted by the surround  */
/* radar on user's body                                          */
/*****************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class SurroundRadar : MonoBehaviour
{
    /* A list tracks the "HitInfo" of all objects that are within the "surroundRadar" */
    List<HitInfo> withinRadar = new();


    /// <summary>
    /// Function do setup when awake
    /// </summary>
    private void Awake()
    {
        /* Force turning off the "isTrigger" for the SurroundRadar */
        GetComponent<Collider>().isTrigger = false;
    }


    /// <summary>
    /// The getter of withinRadar list
    /// </summary>
    public List<HitInfo> WithinRadar
    {
        get { return withinRadar; }
    }


    /// <summary>
    /// Function detects if the "surroundRadar" is in contact with an object
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        /* Get basic information from hitted object */
        Transform objTrans = collision.collider.transform;
        Vector3 hitPoint = collision.GetContact(0).point;

        /* Try to add to the "withinRadar" list */
        AddToWithinRadar(objTrans, hitPoint);
    }


    /// <summary>
    /// Function detects if the "surroundRadar" left contact with an object
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        /* Get transform information from hitted object */
        Transform objTrans = collision.collider.transform;

        /* Try to remove from the "withinRadar" list */
        RemoveFromWithinRadar(objTrans);
    }


    /// <summary>
    /// Function adds object transform into the "withinRadar" list
    /// </summary>
    private void AddToWithinRadar(Transform objTrans, Vector3 hitPoint)
    {
        /* Try to remove an HitInfo from the list if its transform is in there already */
        RemoveFromWithinRadar(objTrans);

        /* Add the new HitInfo object to the "withinRadar" list 
         * The hitPoint changes on every frame ===> so, even for the same transform, 
         * we refresh the HitInfo on every frame by doing "removing" and "adding" */
        HitInfo tempHitInfo = new HitInfo(objTrans, hitPoint);
        withinRadar.Add(tempHitInfo);
    }


    /// <summary>
    /// Function removes an "HitInfo" object from the "withinRadar" list, based on the transform we provided
    /// </summary>
    private void RemoveFromWithinRadar(Transform transform)
    {
        var item = withinRadar.SingleOrDefault(x => x.trans == transform);
        if (item != null)
            withinRadar.Remove(item);
    }

}


/// <summary>
/// The class which holds the transform and hit point information of the objects hitted by radar
/// </summary>
public class HitInfo
{
    public Transform trans { get; set; }                // The transform of objects in contact with the "surroundRadar"
    public Vector3 hitPoint { get; set; }               // The 3D point of the objects which hitted by the radar

    /* The constructor */
    public HitInfo(Transform trans, Vector3 hitPoint)
    {
        this.trans = trans;
        this.hitPoint = hitPoint;
    }
}

