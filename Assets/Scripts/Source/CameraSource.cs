using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;

public class CameraSource : Source
{
    WebCamTexture webcamTexture;

    public CameraSource()
    {
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
    }

    public override Texture GetTexture()
    {
        return webcamTexture;
    }

    public override bool IsFrameReady()
    {
        return webcamTexture.didUpdateThisFrame;
    }

    public override bool IsProcessedOnce()
    {
        return false;
    }
}
