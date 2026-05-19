using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCaster : MonoBehaviour
{
#region Absolute Front CapsuleCast

    public bool doAbsFrontCapsuleCast = true;          // Boolean decide whether run the absolute front capsule cast or not
    readonly float capsuleHeight = 2.5f;               // [Default = 2.5f] The height of the capsule
    readonly float capsuleRadius = 0.25f;              // [Default = 0.25f] The radius of the capsule 
    readonly float maxDistance = 300;                  // [Default = 300] Maximum distance of the capsule cast
    RaycastHit[] absFrontCapsuleHits;                  // Array records the objects hitted by absolute front capsule cast

#endregion

#region Draw Absolute Front CapsuleCast

    public bool drawAbsFrontCapsuleCast = false;        // If we want to draw the capsule cast from user's head
    bool isAbsFrontCapsuleCastDrawn = false;            // Is the capsule drawn already
    CaneContact caneContact;                            // CaneContact class

#endregion


    // Update is called once per frame
    void Update()
    {
        if (doAbsFrontCapsuleCast)
            AbsFrontCapsuleCast();
    }


    /// <summary>
    /// Run the function to throw a capsule cast from the (x&z) position where user's head
    /// is at, and throw to the absolute front direction of the head.
    /// </summary>
    void AbsFrontCapsuleCast()
    {
        Vector3 headDirection = transform.forward;     // the facing direction of user's head
        Vector3 headPos = transform.position;          // the position of user's head
        
        /* The absolute front direction of head by removing the y-axis' value of direction
         * So raise head and lower head won't make any difference to this value */
        Vector3 absFrontHeadDirection = new Vector3(headDirection.x, 0, headDirection.z);

        /* The center of upper and lower spheres which make up the capsule */
        Vector3 upSphereCenter = new Vector3(headPos.x, capsuleHeight - capsuleRadius, headPos.z);
        Vector3 downSphereCenter = new Vector3(headPos.x, 0, headPos.z);

        /* Try to draw the abs front capsuleCast (only visualize its shape once, won't move) */
        if (drawAbsFrontCapsuleCast && !isAbsFrontCapsuleCastDrawn)
        {
            DrawAbsFrontCapsuleCast(upSphereCenter, downSphereCenter);
            isAbsFrontCapsuleCastDrawn = true;
        }

        /* Get all hits information from absolute front capsule cast */
        absFrontCapsuleHits = Physics.CapsuleCastAll(upSphereCenter, downSphereCenter,
                                      capsuleRadius, absFrontHeadDirection, maxDistance);
    }


    /* Function for debugging purpose. It visualizes the shape of capsule case from user's head */
    void DrawAbsFrontCapsuleCast(Vector3 upSphereCenter, Vector3 downSphereCenter)
    {
        /* Create the capsule */
        GameObject absFrontCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        absFrontCapsule.name = "DrawAbsFrontCapsuleCast";

        /* Calculate capsule center */
        Vector3 absFrontCapsuleCenter = (upSphereCenter + downSphereCenter) / 2;

        /* Get the scale values for capsule */
        float xScale = capsuleRadius * 2;
        float zScale = capsuleRadius * 2;
        float yScale = (upSphereCenter.y - downSphereCenter.y + 2 * capsuleRadius) / 2;

        /* Adjust the capsule */
        absFrontCapsule.transform.position = absFrontCapsuleCenter;
        absFrontCapsule.transform.localScale = new Vector3(xScale, yScale, zScale);

        /* Try to add the capsule to cane not detect list */
        caneContact = GameObject.Find("User/GripPoint/Cane").GetComponent<CaneContact>();
        if (!caneContact.objDoNotDetect.Contains(absFrontCapsule.name))
            caneContact.objDoNotDetect.Add(absFrontCapsule.name);
    }


    /// <summary>
    /// Getter for the absolute front capsule hit
    /// </summary>
    public RaycastHit[] AbsFrontCapsuleHits
    {
        get { return absFrontCapsuleHits; }
    }

}
