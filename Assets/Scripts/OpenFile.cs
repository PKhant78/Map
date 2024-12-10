using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;
using UnityEngine.UI;

public class OpenFile : MonoBehaviour
{
    public Button openFileButton;
    private string path;
    public Text filePathText;

    void Start()
    {
        openFileButton.onClick.AddListener(OpenFileW);
    }

    public void OpenFileW()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Load JSON File", "", "json", false);

        
        if (paths.Length > 0)
        {
            string selectedPath = paths.Length > 0 ? paths[0] : null;
            setPath(selectedPath);
            filePathText.text = "Selected File: " + paths[0];
        }
    }

    private void setPath(string p) 
    {
        path = p;
    }

    public string loadPath()
    {
        return path;
    }


}
