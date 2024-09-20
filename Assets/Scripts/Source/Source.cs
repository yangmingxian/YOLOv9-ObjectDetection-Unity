using UnityEngine;

public abstract class Source
{
    public abstract Texture GetTexture();

    public abstract bool IsFrameReady();

    public abstract bool IsProcessedOnce();
}
