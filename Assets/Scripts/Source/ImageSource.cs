using System.IO;
using UnityEngine;

public class ImageSource : Source
{
    private Texture2D texture = new(2, 2);
    private bool isLoaded = false;
    public ImageSource(string path)
    {
        LoadTextureFromFile(path);
    }

    public override Texture GetTexture()
    {
        if (isLoaded)
        {
            return texture;
        }
        else
        {
            return null;
        }
    }

    public override bool IsFrameReady()
    {
        return isLoaded;
    }

    public override bool IsProcessedOnce()
    {
        return true;
    }

    public override void Play()
    {
        throw new System.NotImplementedException();
    }

    void LoadTextureFromFile(string path)
    {
        if (File.Exists(path))
        {
            byte[] imageData = File.ReadAllBytes(path);

            isLoaded = texture.LoadImage(imageData);
            // Load the image data into the texture
            if (!isLoaded)
            {
                Debug.LogError("Failed to load image data into texture.");
            }  
        }
        else
        {
            Debug.LogError("File not found at path: " + path);
        }
    }
}
