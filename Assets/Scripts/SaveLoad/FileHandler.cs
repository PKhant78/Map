using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class FileHandler {

    public static void SaveToJSON(List<GameObjectData> toSave, string filename, string customPath = null)
    {
        //Debug.Log(GetPath(filename, customPath));
        string content = JsonHelper.Serialize(toSave.ToArray(), true);
        WriteFile(GetPath(filename, customPath), content);
    }

    public static List<GameObjectData> ReadListFromJSON(string filename, string customPath = null)
    {
        string content = ReadFile(GetPath(filename, customPath));

        if (string.IsNullOrEmpty (content) || content == "{}") {
            return new List<GameObjectData> ();
        }

        return JsonHelper.Deserialize(content).Items.ToList();
    }

    public static int ReadIDFromJSON(string filename, string customPath = null)
    {
        string content = ReadFile(GetPath(filename, customPath));

        if (string.IsNullOrEmpty (content) || content == "{}") {
            return 0;
        }

        return JsonHelper.Deserialize(content).HighestID;
    }

    public static T ReadFromJSON<T>(string filename, string customPath = null)
    {
        string content = ReadFile(GetPath(filename, customPath));

        if (string.IsNullOrEmpty (content) || content == "{}") {
            return default (T);
        }

        T res = JsonUtility.FromJson<T> (content);

        return res;

    }

    private static string GetPath(string filename, string customPath = null)
    {
        string basePath = string.IsNullOrEmpty(customPath) ? Application.persistentDataPath : customPath;
        if (Path.GetFileName(basePath) == filename)
        {
            return basePath;
        }

        return Path.Combine(basePath, filename);
    }

        private static void WriteFile (string path, string content) {
        FileStream fileStream = new FileStream (path, FileMode.Create);

        using (StreamWriter writer = new StreamWriter (fileStream)) {
            writer.Write (content);
        }
    }

    private static string ReadFile (string path) {
        if (File.Exists (path)) {
            using (StreamReader reader = new StreamReader (path)) {
                string content = reader.ReadToEnd ();
                return content;
            }
        }
        return "";
    }
}