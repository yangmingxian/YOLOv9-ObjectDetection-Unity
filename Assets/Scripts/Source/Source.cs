using UnityEngine;

/// <summary>
/// 输入源的基类 用来派生 图片源/视频源/摄像头源
/// </summary>
public abstract class Source
{
    // 获取数据
    public abstract Texture GetTexture();

    // 输入源是否就绪 非单帧数据的帧更新是否就绪
    public abstract bool IsFrameReady();

    // 是否单帧处理数据
    public abstract bool IsProcessedOnce();

    // 非单帧数据的播放
    public abstract void Play();
}

/// <summary>
/// 数据源类型
/// </summary>
public enum SourceType
{
    ImageSource,
    CameraSource,
    VideoSource
}
