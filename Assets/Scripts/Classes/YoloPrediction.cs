using UnityEngine;

public class YoloPrediction
{
    public int ClassIndex { get; set; }
    public string ClassName { get; set; }
    public Color ClassColor { get; set; }
    public float Score { get; set; }
    public Rect BoundingBox { get; set; }
}

