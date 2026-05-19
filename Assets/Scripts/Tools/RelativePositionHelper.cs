using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RelativePositionHelper
{
    #region Public Functions

    /// <summary>
    /// Function can tell the obj2Position is at what direction of obj1Position, assuming the object1
    /// is facing the direction "obj1Facing".The direction is specified by using "degree/angle".
    ///
    /// [Detail]
    /// 1 ===> The functions returns an angle value X, which indicates that the object2 is
    ///        at X degree of object1, if object1 is the center of the coordinate.
    /// 2 ===> In the coordinate, "forward" direction of object1 is 0 degree. "right" direction of
    ///        object1 is 90 degree, and so on... The degree increasing along the clockwise direction
    /// </summary>
    public static float GetDegreeDirection(Vector3 obj1Position, Vector3 obj2Position, Vector3 obj1Facing)
    {
        /* The position of object 1 & 2 */
        Vector2 obj1PosV2 = new Vector2(obj1Position.x, obj1Position.z);
        Vector2 obj2PosV2 = new Vector2(obj2Position.x, obj2Position.z);

        /* A 2D vector from object1 to object2 */
        Vector2 vectorObj1ToObj2 = obj2PosV2 - obj1PosV2;

        /* Object1's facing direction in 2D (it's a vector) */
        Vector2 vectorObj1Forward = new Vector2(obj1Facing.x, obj1Facing.z);

        /* Calculating the cross-product of two vectors to determine sign.
         * The "crossProduct.y" value will be positive if object1's facing direction
         * is on the right-hand-side of the vector "vectorObj1ToObj2" */
        Vector3 crossProduct = Vector3.Cross(new Vector3(vectorObj1ToObj2.x, 0, vectorObj1ToObj2.y),
                                             new Vector3(vectorObj1Forward.x, 0, vectorObj1Forward.y));

        /* "sign" and "offset" variables will be used to modify the "scope" of degree between vectors. 
         * The scope will be changed from "0 - 180" to "0 - 360" degrees */
        float sign = (crossProduct.y > 0) ? -1 : 1;
        float offset = (sign >= 0) ? 0 : 360;

        /* "degreeObj2AtObj1" indicates object2 is at what degree of object1
         * We uses "sign" and "offset" to change the value from range (0 - 180) to (0 - 360) */
        float degreeObj2AtObj1 = Vector2.Angle(vectorObj1Forward, vectorObj1ToObj2) * sign + offset;

        return degreeObj2AtObj1;
    }


    /// <summary>
    /// Function can tell the object2 is at what direction of object1. The direction is in "degree/angle".
    /// The function assumes the "object1.transform.forward" is the facing direction of object1
    /// </summary>
    public static float GetDegreeDirection(GameObject object1, GameObject object2)
    {
        /* Get the position of object1 & object2 */
        Vector3 obj1Position = object1.transform.position;
        Vector3 obj2Position = object2.transform.position;

        /* Get the facing direction of object1 */
        Vector3 obj1Facing = object1.transform.forward;

        return GetDegreeDirection(obj1Position, obj2Position, obj1Facing);
    }


    /// <summary>
    /// Function can tell the obj2Position is at what direction of obj1Position, assuming the object1
    /// is facing the direction "obj1Facing".The direction is specified in the "clock" format.
    /// </summary>
    /// <returns></returns>
    public static float GetClockDirection(Vector3 obj1Position, Vector3 obj2Position, Vector3 obj1Facing)
    {
        float degree = GetDegreeDirection(obj1Position, obj2Position, obj1Facing);
        return DegreeToClockDir(degree);
    }


    /// <summary>
    /// Given two gameObjects, the function can tell the object2 is at what direction of object1.
    /// The direction is specified using "o'clock" format. For example "9.232 o'clock"
    /// ===> It means "object2 is at 9.232 o'clock of object1"
    /// </summary>
    public static float GetClockDirection(GameObject object1, GameObject object2)
    {
        float degree = GetDegreeDirection(object1, object2);
        return DegreeToClockDir(degree);
    }


    /// <summary>
    /// Function can tell the obj2Position is at what direction of obj1Position, assuming the object1
    /// is facing the direction "obj1Facing".The direction is specified in the "clock" format.
    /// </summary>
    /// <returns></returns>
    public static string GetPrettyClockDirection(Vector3 obj1Position, Vector3 obj2Position, Vector3 obj1Facing)
    {
        /* Calculate the exact clock number like 9.232 ===> it means 9.232 o'clock */
        float clockDir = GetClockDirection(obj1Position, obj2Position, obj1Facing);

        /* Convert clock direction to pretty clock direction and return it */
        return ClockToPrettyClock(clockDir);
    }


    /// <summary>
    /// Given two gameObjects, the function can tell the object2 is at what direction of object1.
    /// The direction is specified using "o'clock" format, but prettified compare to result from "GetClockDirection()".
    /// For example "9 and half o'clock" ===> "object2 is at 9 and half o'clock of object1"
    /// </summary>
    public static string GetPrettyClockDirection(GameObject object1, GameObject object2)
    {
        /* Calculate the exact clock direction like 9.232 ===> means 9.232 o'clock */
        float clockDir = GetClockDirection(object1, object2);

        /* Convert clock direction to pretty clock direction and return it */
        return ClockToPrettyClock(clockDir);
    }


    /// <summary>
    /// Function tells obj2Position is at what direction of obj1Position using "simple direction".
    /// Assuming that the object1 is facing the direction of "obj1Facing"
    /// </summary>
    public static string GetSimpleDirection(Vector3 obj1Position, Vector3 obj2Position, Vector3 obj1Facing)
    {
        /* Get the pretty clock direction in string format */
        string prettyClockDir = GetPrettyClockDirection(obj1Position, obj2Position, obj1Facing);

        /* Return the direction in "simple format" */
        return GetSimpleDirection_Private(prettyClockDir);
    }


    /// <summary>
    /// Public function tells object2 is at what direction of object1 using "simple direction"
    /// For example ===> "object2 is at left of object1"
    /// </summary>
    public static string GetSimpleDirection(GameObject object1, GameObject object2)
    {
        /* Get the pretty clock direction in string format */
        string prettyClockDir = GetPrettyClockDirection(object1, object2);

        /* Return the direction in "simple format" */
        return GetSimpleDirection_Private(prettyClockDir);
    }


    /// <summary>
    /// Given positions of two objects and the facing direction of object1 ===> the function
    /// can tell the "obj2Position" is at what direction of "obj1Position".
    /// The direction is specified using "simple direction" + "o'clock" format.
    /// For example "left, 9 and half o'clock" ===> "object2 is at left, 9 and half o'clock of object1"
    /// </summary>
    public static string GetSimpleAndPrettyClockDirection(Vector3 obj1Position, Vector3 obj2Position, Vector3 obj1Facing)
    {
        /* Get the pretty clock direction in string format */
        string prettyClockDir = GetPrettyClockDirection(obj1Position, obj2Position, obj1Facing);

        /* Get the simple direction */
        string simpleDir = GetSimpleDirection_Private(prettyClockDir);

        /* Combine two types of directions */
        return CombineSimpleAndPrettyDir(simpleDir, prettyClockDir);
    }


    /// <summary>
    /// Given two gameObjects, the function can tell the object2 is at what direction of object1.
    /// The direction is specified using "simple direction" + "o'clock" format.
    /// For example "left, 9 and half o'clock" ===> "object2 is at left, 9 and half o'clock of object1"
    /// </summary>
    public static string GetSimpleAndPrettyClockDirection(GameObject object1, GameObject object2)
    {
        /* Get the pretty clock direction in string format */
        string prettyClockDir = GetPrettyClockDirection(object1, object2);

        /* Get the simple direction */
        string simpleDir = GetSimpleDirection_Private(prettyClockDir);

        /* Combine two types of directions */
        return CombineSimpleAndPrettyDir(simpleDir, prettyClockDir);
    }


    /// <summary>
    /// Function returns the normalized 2D distance between two position provided
    /// The distance result by default is "Feet". However, if developer states "useFeet = false", the distance is in "Meters".
    /// </summary>
    public static float GetDistanceNum(Vector3 obj1Position, Vector3 obj2Position, int digitsAfterDecimal = 1, bool useFeet = true)
    {
        /* The measurement multipler for translating meters into feet */
        float measureMulti = 3.28084f;

        /* The position of object 1 & 2 way points */
        Vector2 obj1PosV2 = new Vector2(obj1Position.x, obj1Position.z);
        Vector2 obt2PosV2 = new Vector2(obj2Position.x, obj2Position.z);

        /* Get the distance between object 1 & 2 way points (In feet, NOT meters) */
        float dist = Vector2.Distance(obj1PosV2, obt2PosV2);

        /* Convert distance from "Meters" to "Feet" if needed */
        if (useFeet)
            dist *= measureMulti;

        /* Rounding the number */
        float factor = Mathf.Pow(10f, digitsAfterDecimal);
        dist = Mathf.Round(dist * factor) / factor;

        return dist;
    }


    /// <summary>
    /// Function returns the distance between two objects passed in.
    /// The distance result by default is "Feet". However, if developer states "useFeet = false", the distance is in "Meters".
    /// </summary>
    public static float GetDistanceNum(GameObject object1, GameObject object2, int digitsAfterDecimal = 1, bool useFeet = true)
    {
        /* The position of object 1 & 2 */
        Vector3 position1 = object1.transform.position;
        Vector3 position2 = object2.transform.position;

        /* Calculate normalized distance between two position 
         * Normalized from Vector3 to Vector2 */
        return GetDistanceNum(position1, position2, digitsAfterDecimal, useFeet);
    }


    /// <summary>
    /// Function returns the normalized 2D distance string between two position provided.
    /// The distance result by default is "Feet". However, if developer states "useFeet = false", the distance is in "Meters".
    /// </summary>
    public static string GetDistance(Vector3 obj1Position, Vector3 obj2Position, int digitsAfterDecimal = 1, bool useFeet = true)
    {
        /* Get the numerical distance */
        float dist = GetDistanceNum(obj1Position, obj2Position, digitsAfterDecimal, useFeet);

        /* Conditionally return "xxx feet" or "xxx meters" */
        return distNumToDistAndMeasure(dist, useFeet);
    }


    /// <summary>
    /// Function returns the distance between two objects passed in.
    /// The distance result by default is "Feet". However, if developer states "useFeet = false", the distance is in "Meters".
    /// </summary>
    public static string GetDistance(GameObject object1, GameObject object2, int digitsAfterDecimal = 1, bool useFeet = true)
    {
        /* Get the numerical distance */
        float dist = GetDistanceNum(object1, object2, digitsAfterDecimal, useFeet);

        /* Conditionally return "xxx feet" or "xxx meters" */
        return distNumToDistAndMeasure(dist, useFeet);
    }

    #endregion Public Functions


    #region Private Functions

    /// <summary>
    /// Function converts degree direction (0-360) to clock direction (eg. 9.2, 9.5, etc...)
    /// </summary>
    private static float DegreeToClockDir(float degreeDir)
    {
        return degreeDir / 30f;
    }


    /// <summary>
    /// Function converts "clock direction" to "pretty clock direction".
    /// For example ===> From 9.5 to "9 and half o'clock"
    /// </summary>
    private static string ClockToPrettyClock(float clockDir)
    {
        /* Get the integer part and decimal part */
        int intPart = (int)clockDir;
        float decimalPart = clockDir - intPart;

        /* Based on decimal part, return different kind of value */
        if (decimalPart < 0.333)
            return $"{intPart}";
        else if (decimalPart <= 0.666)
            return $"{intPart} and half";
        else
            return $"{(intPart + 1) % 12}";
    }


    /// <summary>
    /// A private functions transform "prettyClockDir" string into simple direction like "Left", "Right", etc...
    /// This is for inside class usage only!
    /// </summary>
    private static string GetSimpleDirection_Private(string prettyClockDir)
    {
        /* A dictionary maps "pretty clock direction" to "simple direction" */
        Dictionary<string, string> frontAndBehindDict = new()
        {
            { "11 and half", "front" },
            { "0", "front" },
            { "0 and half", "front" },
            { "5 and half", "back" },
            { "6", "back" },
            { "6 and half", "back" }
        };

        /* The number associated with "left" or "right" simple direction */
        List<string> leftNumber = new() { "7", "8", "9", "10", "11" };
        List<string> rightNumber = new() { "1", "2", "3", "4", "5" };

        /* Get the simple direction 
         * Careful ===> Make sure checking "leftNumber" before "rightNumber", b/c number 
         * '1' is in '10' and '11' as well. Reversed checking order will lead to issue */
        string simpleDir = "";
        if (frontAndBehindDict.ContainsKey(prettyClockDir))
            simpleDir = frontAndBehindDict[prettyClockDir];
        else if (leftNumber.Any(num => prettyClockDir.Contains(num)))
            simpleDir = "left";
        else if (rightNumber.Any(num => prettyClockDir.Contains(num)))
            simpleDir = "right";

        return simpleDir;
    }


    /// <summary>
    /// Function combines the simple and prettyClockDir to one string 
    /// </summary>
    private static string CombineSimpleAndPrettyDir(string simpleDir, string prettyClockDir)
    {
        /* Combine two types of directions */
        string combinedDir = prettyClockDir;
        if (simpleDir != "")
            combinedDir = $"'{simpleDir.ToUpper()}', {prettyClockDir}";

        return combinedDir;
    }


    /// <summary>
    /// Function converts distance number to distance of format "distance num + measurement"
    /// For example, "xxx feet" or "xxx meters".
    /// </summary>
    private static string distNumToDistAndMeasure(float dist, bool useFeet)
    {
        /* Conditionally return "xxx feet" or "xxx meters" */
        if (useFeet)
            return $"{dist} feet";
        else
            return $"{dist} meters";
    }

    #endregion Private Functions

}

