using System;
using System.Collections.Generic;
using Google.MiniJSON;
using Unity.VisualScripting;
using UnityEngine;

public static class FirebaseSave {
    // methods implemented by Daniel
}

public static class JsonHelper {
    public static string Serialize(GameObjectData[] items, bool prettyPrint = false) {
        Environment env = new(items);
        return JsonUtility.ToJson(env, prettyPrint);
    }

    public static Environment Deserialize(string json) {
        return JsonUtility.FromJson<Environment>(json);
    }
}

[Serializable]
public class Environment {
    public GameObjectData[] Items;
    public int HighestID;

    public Environment(GameObjectData[] items) {
        Items = items;
        HighestID = BuildingSystem.CurrentPlaceableID;
    }
}
/*
public static class JsonHelper {
    public static T[] FromJson<T> (string json) {
        Environment<T> env = JsonUtility.FromJson<Environment<T>>(json);
        return env.Items;
    }

    public static string ToJson<T> (T[] array) {
        Environment<T> env = new();
        env.Items = array;
        env.Author = null; // to be implemented
        env.ItemMaxID = BuildingSystem.CurrentPlaceableID;
        return JsonUtility.ToJson(env);
    }

    public static string ToJson<T> (T[] array, bool prettyPrint) {
        Environment<T> env = new();
        env.Items = array;
        
        return JsonUtility.ToJson (env, prettyPrint);
    }

    [Serializable]
    private class Environment<T> {
        public T[] Items;
        public string Author;
        public int ItemMaxID;
    }
}
*/