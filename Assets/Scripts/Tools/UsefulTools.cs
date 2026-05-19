using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


/// <brief>
/// A class which contains lots of general purposes, useful functions
/// </brief>

public static class UsefulTools
{

    /// <summary>
    /// A helper function to split the string at the capital letter
    /// </summary>
    public static string[] SplitAtCapital(string str)
    {
        string[] result = Regex.Split(str, @"(?<!^)(?=[A-Z])");
        return result;
    }


    /// <summary>
    /// A helper function for join a list of strings 
    /// </summary>
    public static string ArrayToString(string[] strArr, string joinBy)
    {
        string result = "";
        for (int i = 0; i < strArr.Length; ++i)
        {
            result += strArr[i];
            if (i != strArr.Length - 1)
                result += joinBy;
        }
        return result;
    }


    /// <summary>
    /// Function splits a string into a list of strings. Split at the "splitAt" string provided
    /// </summary>
    public static string[] SplitText(string text, string splitAt)
    {
        return text.Split(splitAt);
    }


    /// <summary>
    /// Generic function which generate a deep copy of a List
    /// </summary>
    public static List<T> ListDeepCopy<T>(List<T> originList)
    {
        List<T> newList = new();
        foreach (T x in originList)
            newList.Add(x);

        return newList;
    }


    /// <summary>
    /// Function generates a random number between minimum and maximum
    /// Include "minimum" but not include "maximum"
    /// </summary>
    public static double GetRandomDouble(double minimum, double maximum)
    {
        var random = new System.Random();
        return random.NextDouble() * (maximum - minimum) + minimum;
    }


    /// <summary>
    /// Read the content from a TextAsset and transfer it into string variable
    /// </summary>
    public static string ParseTextAsset(TextAsset textFile)
    {
        string contentText = "";
        if (textFile)
        {
            string[] lines = textFile.text.Split("\n"[0]);
            contentText = string.Join("", lines);
        }
        return contentText;
    }
}

