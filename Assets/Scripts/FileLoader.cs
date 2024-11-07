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
    private string filePath = "";
    private SourceType sourceType = SourceType.ImageSource;

    public Action<SourceType, string> OnFileSelected;

    public void OpenFileBrowser()
    {
        // Set filters for images and videos
        FileBrowser.SetFilters(true,
            new FileBrowser.Filter("Images", ".jpg", ".png", ".jpeg"),
            new FileBrowser.Filter("Videos", ".mp4", ".avi", ".mov"));

        // Set default filter
        // FileBrowser.SetDefaultFilter(".jpg");

        // Show the file browser
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    // 协程-处理文件选择逻辑
    IEnumerator ShowLoadDialogCoroutine()
    {
        // 等待用户选择文件
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Select File", "Load");

        if (FileBrowser.Success)
        {
            string path = FileBrowser.Result[0];
            Debug.Log("Selected: " + path);

            string extension = Path.GetExtension(path).ToLower();

            if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
            {
                LoadImage(path);
            }
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
        }
        else
        {
            OnFileSelected?.Invoke(sourceType, "");
        }
    }

    void LoadVideo(string path)
    {
        this.filePath = path;
        OnFileSelected?.Invoke(sourceType, path);
    }

    void LoadImage(string path)
    {
        this.filePath = path;
        OnFileSelected.Invoke(sourceType, path);
    }
}
