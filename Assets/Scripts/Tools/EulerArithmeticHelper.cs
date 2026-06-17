using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EulerArithmeticHelper
{
    /// <summary>
    /// Function subtract an euler angle from another.
    /// 
    /// In one axis of EulerAngel, the range of value is from 0 to 360.
    /// One RHS of 0 could be "0.23", while on LHS of 0 could be "359.72", etc...
    /// Because of the property above, we can't use normal number subtraction when subtract two eular angle.
    /// We need this function specialized for doing eulerAngle subtraction.
    /// 
    /// </summary>
    public static float SubtractEulerAngle(float minuend, float subtrahend)
    {
        if (minuend - subtrahend >= 0)
            return minuend - subtrahend;
        else
            return 360 - (subtrahend - minuend);
    }


    /// <summary>
    /// Function subtracts one Euler rotation from another
    /// </summary>
    public static Vector3 SubtractEulerRotation(Vector3 minuend, Vector3 subtrahend)
    {
        /* (x,y,z) difference after using "minuend" rotation minus "subtrahend" rotation */
        float xRotDiff = SubtractEulerAngle(minuend.x, subtrahend.x);
        float yRotDiff = SubtractEulerAngle(minuend.y, subtrahend.y);
        float zRotDiff = SubtractEulerAngle(minuend.z, subtrahend.z);
        Vector3 rotDiff = new(xRotDiff, yRotDiff, zRotDiff);

        return rotDiff;
    }


    /// <summary>
    /// Function checks if the rotation1 is near to the rotation2.
    /// If difference of (x,y,z) value of rotation1 and rotation2 are within "rotNearErrorMargin" ==> They are near
    /// </summary>
    public static bool IsEulerRotNear(Vector3 rotation1, Vector3 rotation2, float rotNearErrorMargin)
    {
        /* [Note]
         * 
         * The range of rotation in Unity is from 0-360. 
         * On RHS of origin "0", it's number like 0.034.
         * On LHS of origin "0", it's number like 359.88, etc...
         * 
         * So, to get the "distance" between two rotation values from one axis (e.g., x-axis value in rotation),
         * we can't just brutely subtract these two value and take an absolute value from it
         * 
         * To get the difference, we need to image these two rotation values are two points on a circle, 
         * we need to find the shorest "arc path" between them ===> so I used below algorithm to get the difference 
         */

        float xDiff = Mathf.Min(Mathf.Abs(rotation1.x - rotation2.x), 360 - Mathf.Abs(rotation1.x - rotation2.x));
        float yDiff = Mathf.Min(Mathf.Abs(rotation1.y - rotation2.y), 360 - Mathf.Abs(rotation1.y - rotation2.y));
        float zDiff = Mathf.Min(Mathf.Abs(rotation1.z - rotation2.z), 360 - Mathf.Abs(rotation1.z - rotation2.z));

        /* Checks if two rotations' x,y,z are "near" one-by-one */
        bool xNear = xDiff < rotNearErrorMargin;
        bool yNear = yDiff < rotNearErrorMargin;
        bool zNear = zDiff < rotNearErrorMargin;

        /* x,y,z all need to be "near" to make two rotations "near" */
        return xNear && yNear && zNear;
    }

}

