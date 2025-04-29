using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseFileSelector : MonoBehaviour
{
    private string currentPath;
    public Text filePathText;
    
    // This will store all available files in Firebase
    private List<string> availableFiles = new List<string>();
    
    // Reference to UI elements that will display and select files
    public Dropdown filesDropdown;
    public InputField newFileNameInput;
    
    async void Start()
    {
        await FirebaseHandler.Initialize();
        await RefreshFileList();
    }
    
    public async Task RefreshFileList()
    {
        // Get the list of files from the "_file_index" location in Firebase
        availableFiles = await FirebaseHandler.ReadFromDatabase<List<string>>("_file_index") ?? new List<string>();
        
        // Update dropdown with available files
        UpdateFilesDropdown();
    }
    
    private void UpdateFilesDropdown()
    {
        if (filesDropdown != null)
        {
            filesDropdown.ClearOptions();
            
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach (string file in availableFiles)
            {
                options.Add(new Dropdown.OptionData(file));
            }
            
            filesDropdown.AddOptions(options);
        }
    }
    
    public void OnSelectFile()
    {
        if (filesDropdown != null && filesDropdown.options.Count > 0 && filesDropdown.value >= 0)
        {
            currentPath = filesDropdown.options[filesDropdown.value].text;
            
            if (filePathText != null)
            {
                filePathText.text = "Selected File: " + currentPath;
            }
            
            Debug.Log("Selected file: " + currentPath);
        }
    }
    
    public void OnNewFile()
    {
        if (newFileNameInput != null && !string.IsNullOrEmpty(newFileNameInput.text))
        {
            currentPath = newFileNameInput.text;
            
            if (filePathText != null)
            {
                filePathText.text = "New File: " + currentPath;
            }
            
            Debug.Log("New file: " + currentPath);
            
            // Add to available files if it's not already there
            if (!availableFiles.Contains(currentPath))
            {
                availableFiles.Add(currentPath);
                UpdateFilesDropdown();
                _ = SaveFileIndex(); // Fire and forget
            }
        }
    }
    
    private async Task SaveFileIndex()
    {
        // Save the updated file list to Firebase
        await FirebaseHandler.SaveToDatabase(availableFiles, "_file_index");
    }
    
    public string GetCurrentPath()
    {
        return currentPath;
    }
}