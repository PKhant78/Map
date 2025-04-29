using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Linq;
public static class FirebaseHandler
{
    private static DatabaseReference databaseReference;
    private static FirebaseApp app;
    private static bool isInitialized = false;

    public static async Task Initialize()
    {
        if (isInitialized) return;
        
        // Initialize Firebase
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                isInitialized = true;
                Debug.Log("Firebase initialized successfully");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    public static async Task SaveToDatabase<T>(List<T> toSave, string path)
    {
        await EnsureInitialized();
        
        string content = JsonHelper.ToJson<T>(toSave.ToArray(), true);
        await databaseReference.Child(path).SetRawJsonValueAsync(content);
    }

    public static async Task SaveToDatabase<T>(T toSave, string path)
    {
        await EnsureInitialized();
        
        string content = JsonUtility.ToJson(toSave, true);
        await databaseReference.Child(path).SetRawJsonValueAsync(content);
    }

    public static async Task<List<T>> ReadListFromDatabase<T>(string path)
    {
        await EnsureInitialized();
        
        var snapshot = await databaseReference.Child(path).GetValueAsync();
        
        if (!snapshot.Exists)
        {
            return new List<T>();
        }
        
        string content = snapshot.GetRawJsonValue();
        
        if (string.IsNullOrEmpty(content) || content == "{}")
        {
            return new List<T>();
        }
        
        List<T> res = JsonHelper.FromJson<T>(content).ToList();
        return res;
    }

    public static async Task<T> ReadFromDatabase<T>(string path)
    {
        await EnsureInitialized();
        
        var snapshot = await databaseReference.Child(path).GetValueAsync();
        
        if (!snapshot.Exists)
        {
            return default(T);
        }
        
        string content = snapshot.GetRawJsonValue();
        
        if (string.IsNullOrEmpty(content) || content == "{}")
        {
            return default(T);
        }
        
        T res = JsonUtility.FromJson<T>(content);
        return res;
    }
    
    private static async Task EnsureInitialized()
    {
        if (!isInitialized)
        {
            await Initialize();
        }
    }
}

// Keep the JsonHelper class as it was in the original FileHandler.cs
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}