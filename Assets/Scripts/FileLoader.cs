using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class FileLoader : MonoBehaviour
{
    public Action<SourceType, string> OnSourceDetected;

    private List<SourceType> sourceTypes = new() { SourceType.ImageSource, SourceType.CameraSource, SourceType.VideoSource };
    private string path = "";
    private SourceType sourceType = SourceType.ImageSource;
    // Method to open the file browser
    public void OpenFileBrowser()
    {
        // Set filters for images and videos
        FileBrowser.SetFilters(true,
            new FileBrowser.Filter("Images", ".jpg", ".png", ".jpeg"),
            new FileBrowser.Filter("Videos", ".mp4", ".avi", ".mov"));

        // Set default filter
        FileBrowser.SetDefaultFilter(".jpg");

        // Show the file browser
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    // Coroutine to handle the file selection
    IEnumerator ShowLoadDialogCoroutine()
    {
        // Wait for the user to select a file
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Select File", "Load");

        // Check if a file was selected
        if (FileBrowser.Success)
        {
            string path = FileBrowser.Result[0];
            Debug.Log("Selected: " + path);

            // Get the file extension
            string extension = Path.GetExtension(path).ToLower();

            // Check if the file is an image
            if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
            {
                LoadImage(path);
            }
            // Check if the file is a video
            else if (extension == ".mp4" || extension == ".avi" || extension == ".mov")
            {
                LoadVideo(path);
            }
            else
            {
                Debug.LogWarning("Unsupported file type");
            }
        }
    }

    public void SetDefaultFilter(SourceType sourceType)
    {
        this.sourceType = sourceType;
        if (sourceType == SourceType.ImageSource)
        {
            Debug.Log("Setting Default To: Image");
            FileBrowser.SetDefaultFilter(".jpg");
        }
        else if (sourceType == SourceType.VideoSource)
        {
            Debug.Log("Setting Default To: Video");
            FileBrowser.SetDefaultFilter(".mp4");
        } else
        {
            OnSourceDetected?.Invoke(sourceType, "");
        }
    }

    // Method to load and play the video
    void LoadVideo(string path)
    {
        this.path = path;
        OnSourceDetected?.Invoke(sourceType, path);
    }

    // Coroutine to load and display the image
    void LoadImage(string path)
    {
        this.path = path;
        OnSourceDetected.Invoke(sourceType, path);
    }
}
