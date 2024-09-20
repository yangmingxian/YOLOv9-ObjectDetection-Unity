using UnityEngine;

public class ImageSource : Source
{
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
        return true;
    }
}
