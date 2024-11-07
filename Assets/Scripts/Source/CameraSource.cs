using System.Collections;
using UnityEngine;

/// <summary>
/// 摄像头源
/// </summary>
public class CameraSource : Source
{
    WebCamTexture webcamTexture;

    public CameraSource()
    {
        // 获取所有可用摄像头
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            // 显示所有摄像头的名称
            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log("摄像头 " + i + ": " + devices[i].name);
            }

            // 使用第一个摄像头
            webcamTexture = new WebCamTexture(devices[0].name);
        }
        else
        {
            Debug.LogWarning("未找到可用的摄像头！");
        }

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

    public override void Play()
    {
        webcamTexture.Play();
    }
}
