using UnityEngine;
using UnityEngine.Video;

public class VideoSource : Source
{
    private VideoPlayer videoPlayer;
    private long lastProcessedFrame = -1; // Track the last processed frame

    public override Texture GetTexture()
    {
        throw new System.NotImplementedException();
    }

    public override bool IsFrameReady()
    {
        throw new System.NotImplementedException();
    }

    public override bool IsProcessedOnce()
    {
        return false;
    }
}
