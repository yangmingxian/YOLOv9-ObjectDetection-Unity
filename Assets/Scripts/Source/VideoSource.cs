using UnityEngine;
using UnityEngine.Video;

public class VideoSource : Source
{
    private VideoPlayer videoPlayer;
    private long lastProcessedFrame = -1; // Track the last processed frame
    private bool frameReady = false;

    public VideoSource(string path)
    {
        videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>();
        videoPlayer.url = path;
    }

    public override Texture GetTexture()
    {
        return videoPlayer.texture;
    }

    public override bool IsFrameReady()
    {
        // Check if a new frame is available from the VideoPlayer
        if (videoPlayer.isPlaying && videoPlayer.frame > 0)
        {
            // Only process the frame if it's new
            if (videoPlayer.frame != lastProcessedFrame)
            {
                lastProcessedFrame = videoPlayer.frame;
                return true;
            }
        }
        return false;
    }

    public override bool IsProcessedOnce()
    {
        return false;
    }

    public override void Play()
    {
        videoPlayer.Play();
    }
}
