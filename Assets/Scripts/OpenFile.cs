using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;
using UnityEngine.UI;

public class OpenFile : MonoBehaviour
{
    private string path;
    public Text filePathText;

    void Start()
    {
        
    }

    public void LoadFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Load JSON File", "", "json", false);

        
        if (paths.Length > 0)
        {
            string selectedPath = paths[0];
            setPath(selectedPath);
            //filePathText.text = "Selected File: " + paths[0];
        }
    }
    public void SaveFile()
    {
        string path = StandaloneFileBrowser.SaveFilePanel("Save JSON File", "", "SaveFile", "json");

        if (!string.IsNullOrEmpty(path))
        {
            setPath(path);
            Debug.Log( "Save File: " + path);
        }
    }

    private void setPath(string p) 
    {
        path = p;
    }

    public string loadPath()
    {
        Debug.Log(path);
        return path;
        
    }


}
