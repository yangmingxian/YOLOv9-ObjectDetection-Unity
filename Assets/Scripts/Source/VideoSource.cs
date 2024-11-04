using UnityEngine;
using UnityEngine.Video;

public class VideoSource : Source
{
    private VideoPlayer videoPlayer;
    // 使用long来记录处理的上一帧，(2^63-1)足够处理然和时长的视频了
    private long lastProcessedFrame = -1;

    public VideoSource(string path)
    {
        videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>();
        videoPlayer.url = path;

        videoPlayer.Prepare();

        // 等待视频准备好，获取宽高
        videoPlayer.prepareCompleted += (source) =>
        {
            originalSize = new Vector2Int((int)videoPlayer.width, (int)videoPlayer.height);
            Debug.Log($"Video loaded with size: {originalSize.x} x {originalSize.y}");
        };
    }

    public override Texture GetTexture()
    {
        return videoPlayer.texture;
    }

    public override bool IsFrameReady()
    {
        // 判断是否第一帧是就绪的
        if (videoPlayer.isPlaying && videoPlayer.frame > 0)
        {
            // 只处理新的帧
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
