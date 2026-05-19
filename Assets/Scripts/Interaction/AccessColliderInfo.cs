using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccessColliderInfo : MonoBehaviour
{
    // A list of potential "part name" which are not useful, will filter them out later
    static List<string> uselessPartNameList = new List<string> { "_Holder_", "Outside", "Inside", "" };


    /// <summary>
    /// Function for checking if a triggered "object part" is the outside or inside of an object
    /// </summary>
    public static string WhichSide(Transform other)
    {
        string side = "";

        if ((other.name == "Outside") || (GetParentName(other) == "Outside"))
        {
            side = "Outside";  // For an touched object, it's an "Outside" object if itself named "Outside", or it's below an "Outside" object
        }
        else if ((other.name == "Inside") || (GetParentName(other) == "Inside"))
        {
            side = "Inside";  // For an touched object, it's an "Inside" object if itself named "Inside", or it's below an "Inside" object
        }
        return (side);
    }


    /// <summary>
    /// Function for get the hierarchy level of the triggered "object part"
    /// </summary>
    public static int WhichLevel(Transform other)
    {
        /* If the object's parent is NULL, it means the cane collided with a single layer object ===> thus return "1" */
        if (other.transform.parent == null)
            return 1;

        /* If it's not a single layer object 
         * ===> Root object (level=1) must be empty object, so if cane collide on something, it at least be at level=2 (level=2 is usually empty as well) */
        int level = 2;

        Transform rootTrans = other.transform.root;    // Transform of the root game object
        Transform tempTrans = other.transform;         // Initialize the temporary transform holder with current object's transform

        /* If the object is not directly under the root, then it can't be level 2, thus increase 1 level, until reach an object which is direct child of the root */
        while (tempTrans.parent != rootTrans)
        {
            level += 1;
            tempTrans = tempTrans.parent;
        }

        return level;
    }


    /// <summary>
    /// Function for checking what material the triggered "object part" is made of (eg. wood, metal, glass, etc...)
    /// </summary>
    public static string WhatMaterial(Transform other)
    {
        string material = "";
        int level = WhichLevel(other);  // get the hierarchy level of current triggered object
        Transform tempTrans = other.transform;

        /* From current level to root, try to find matrial level-by-level. The material from the deepest level will be the final choice */
        for (int i = 0; i < level; i++)
        {
            material = tempTrans.tag;
            if (material != "Untagged") break;
            tempTrans = tempTrans.parent;
        }

        return (material);
    }


    /// <summary>
    /// Function traverse from the hitted object's hierarchy tree to get the name of the root object
    /// (eg. "Table", "Navigation Road", "Floor", etc...)
    /// </summary>
    public static string GetRootName(Transform other)
    {
        string name = other.transform.root.name;
        return name;
    }


    /// <summary>
    /// Function traverse from the hitted object's hierarchy tree to get the name of the object's parent
    /// </summary>
    public static string GetParentName(Transform other)
    {
        /* If it's a single layer object, the parent is NULL */
        if (other.transform.parent == null)
            return "<--- NULL --->";

        /* If the parent is not NULL, return its name */
        return other.transform.parent.name;
    }


    /// <summary>
    /// Function for getting "object part name" (eg. table leg) based on the thing cane triggered
    /// </summary>
    public static string GetPartName(Transform other)
    {
        /* The transform of the 2nd level object in the hierarchy */
        Transform SecondLevelTrans = GetNthLevelTrans(other, 2);

        /* Get the name */
        if (SecondLevelTrans == other)
            return "";
        else
            return SecondLevelTrans.name;
    }


    /// <summary>
    /// Function gets the transform of the Nth level object under the hierarchy which "other" belongs to
    /// For Example:
    /// [Room] -> [Wall1] -> [LargeWindow]. If "other = LargeWindow" and "targetLevel = 2".
    /// Function will return the transform of "Wall1".
    /// </summary>
    public static Transform GetNthLevelTrans(Transform other, int targetLevel)
    {
        int level = WhichLevel(other);

        /* Try to get the transform of the Nth level object */
        Transform tempTrans = other.transform;
        for (int i = 0; i < level - targetLevel; ++i)
        {
            tempTrans = tempTrans.parent;
        }

        return tempTrans;
    }


    /// <summary>
    /// Function for getting both "object part name" and "object name" based on the thing cane triggered
    /// </summary>
    public static string[] GetObjNames(Transform other)
    {
        string name = "";
        string partName = "";

        /* The full object name, like "Table", is always at the root level */
        name = GetRootName(other);

        /* Try to get the part object name, like "leg1" (of "Table") */
        partName = GetPartName(other);

        /* Load "name" and "part name" into an array to pass it out */
        string[] nameArray = new string[] { name, partName };
        return (nameArray);
    }


    /// <summary>
    /// Function to judge whether the part name of current collider object useful (eg. "Outside" or "_Holder_" are not useful. "Table Leg" is useful)
    /// </summary>
    private static bool IsPartNameUseful(Transform other)
    {
        string partName = GetPartName(other);
        bool judgeUsefulness = (uselessPartNameList.Contains(partName)) ? false : true;
        return judgeUsefulness;
    }


    /// <summary>
    /// Function for generating string to describe hitted object
    /// </summary>
    public static string DescribeHittedObject(Transform other)
    {
        string[] nameArray = GetObjNames(other); // Get "object part" and "object" name
        string name = nameArray[0];
        string partName = nameArray[1];

        /* If the partName is useful, we will describe the hitted object using its 2nd-level "partName" + " of " + root level "Name", otherwise use the root level "Name" only */
        string description = IsPartNameUseful(other) ? (UsefulTools.ArrayToString(UsefulTools.SplitAtCapital(partName), " ") + " of " + UsefulTools.ArrayToString(UsefulTools.SplitAtCapital(name), " ")) : (UsefulTools.ArrayToString(UsefulTools.SplitAtCapital(name), " "));

        return description;
    }


    /// <summary>
    /// Function gets the ObjId of the level which we actually refers to when describe the hitted object name
    /// (Will be used in VerbalManager to control the frequency of calling out name when hitting objects)
    /// </summary>
    public static int GetActualReferObjId(Transform other)
    {
        /* If the partName is useful, the object we actually refers to is the 2nd-level "part", otherwise we refers to the root level */
        int resultId = IsPartNameUseful(other) ? GetObjIdByLevel(other, 2) : GetObjIdByLevel(other, 1);
        return resultId;
    }


    /// <summary>
    /// For collider object "other", get the ID of the object which is at levelN of its hierarchy tree
    /// (Will be used to get Actual Refer Object ID)
    /// </summary>
    private static int GetObjIdByLevel(Transform other, int levelN)
    {
        int resultId;
        int currLevel = WhichLevel(other);

        Transform tempTrans = other.transform;
        for (int i = 0; i < currLevel - levelN; ++i)
        {
            tempTrans = tempTrans.parent;
        }

        resultId = tempTrans.GetInstanceID();
        return resultId;
    }


    /// <summary>
    /// Function for print the name of object which the cane hitted
    /// </summary>
    public static void PrintHittedObject(Transform other)
    {
        string description = DescribeHittedObject(other);
        Debug.Log(description);
    }


    /// <summary>
    /// Function check whether the object is a floor.
    /// How to check:
    /// 1. The rootName might be "xxxFloor" like "BrickFloor" or "ConcreteFloor"
    /// 2. The material of this object would be "xxxFloor" like "WoodFloor"
    /// Either one of these 2 clues should indicate the object is a floor object
    /// </summary>
    public static bool IsFloor(Transform other)
    {
        // Find clue from root name
        string rootName = GetRootName(other);
        string[] namePartArr = UsefulTools.SplitAtCapital(rootName);
        int rSize = namePartArr.Length;

        // Find clue from material name
        string material = WhatMaterial(other);
        string[] matPartArr = UsefulTools.SplitAtCapital(material);
        int mSize = matPartArr.Length;

        // Check if this is an floor object
        bool judge = false;
        if (namePartArr[rSize - 1] == "Floor" || matPartArr[mSize - 1] == "Floor")
            judge = true;

        return judge;
    }


    /// <summary>
    /// Function check whether the object is a navigation road
    /// How to check:
    /// 1. The rootName might be "NavigationRoad" (maybe it will be changed in the future)
    /// 2. The material of this object should be "xxxTactilePaving" like "PlasticTactilePaving"
    /// Either one of these 2 clues should indicate the object is a navigation road object
    /// </summary>
    public static bool IsNavigationRoad(Transform other)
    {
        List<string> potentialNavName = new List<string>() { "NavigationRoad", "TactileRoad" };

        // Find clue from root name
        string rootName = GetRootName(other);

        // Find clue from material name
        string material = WhatMaterial(other);

        // Check if this is an NavigationRoad object
        bool judge = false;
        if (potentialNavName.Contains(rootName) || material.Contains("TactilePaving"))
        {
            judge = true;
        }

        return judge;
    }


}

