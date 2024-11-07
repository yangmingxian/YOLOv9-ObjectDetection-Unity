using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drawable
{
    private ObjectPool<BoundingBox> boundingBoxPool;
    public RawImage screen;
    private RectTransform screenRectTransform;
    private List<BoundingBox> activeBoundingBoxes;
    private GameObject boundingBoxPrefab;
    private float screenWidth;
    private float screenHeight;

    public Drawable(Source source)
    {
        screen = GameObject.Find("Display").GetComponent<RawImage>();
        ScaleOutputDisplay(source.originalSize);
        PrepareBoundingBoxPool();
    }

    public void SetTexture(Texture texture)
    {
        if (texture == null)
        {
            Debug.LogError("The given texture is null");
            return;
        }

        screen.texture = texture;
    }

    public void ScaleOutputDisplay(Vector2Int originalSize)
    {
        // 计算宽高缩放比例
        // Debug.Log("Screen.width" + Screen.width + " Screen.height" + Screen.height);

        float widthScale = 1920f / Detector.TARGET_WIDTH;
        float hightScale = 1080f / Detector.TARGET_HEIGHT;

        var remainRatioRate = (float)originalSize.x / originalSize.y;

        var scale = Mathf.Min(widthScale, hightScale);
        // Debug.Log("widthScale" + widthScale + " hightScale" + hightScale);
        // Debug.Log("scale" + scale);

        if (scale == widthScale)
        {
            var temp = widthScale / remainRatioRate;
            screen.transform.localScale = new Vector3(widthScale, temp, 1);
        }
        else
        {
            var temp = hightScale * remainRatioRate;
            screen.transform.localScale = new Vector3(temp, hightScale, 1);
        }
    }

    public void DrawBoundingBoxes(List<YoloPrediction> yoloPredictions)
    {
        // Calculate the offset for center-middle anchoring
        float offsetX = screenWidth / 2;
        float offsetY = screenHeight / 2;

        foreach (YoloPrediction prediction in yoloPredictions)
        {
            // Get a bounding box from the pool
            BoundingBox boundingBox = boundingBoxPool.Get();
            activeBoundingBoxes.Add(boundingBox);

            // Get the RectTransform of the bounding box
            RectTransform boxRectTransform = boundingBox.GetComponent<RectTransform>();

            var predBoundingBox = prediction.BoundingBox;

            // Calculate the box's position relative to the center of the image (center-middle anchoring)
            float xMin = predBoundingBox.xMin - offsetX;
            float yMin = predBoundingBox.yMin - offsetY;
            float width = predBoundingBox.width;
            float height = predBoundingBox.height;

            // Set the size and position of the bounding box
            boxRectTransform.anchoredPosition = new Vector2(xMin + width / 2, yMin + height / 2); // Center the box
            boxRectTransform.sizeDelta = new Vector2(width, height);

            // Update the bounding box appearance
            boundingBox.SetColor(prediction.ClassColor);
            boundingBox.SetLabel($"{prediction.ClassName} ({prediction.Score:F2})");
        }
    }

    public void ResetBoundingBoxes()
    {
        foreach (BoundingBox box in activeBoundingBoxes)
        {
            boundingBoxPool.ReturnToPool(box);
        }
        activeBoundingBoxes.Clear();
    }

    private void PrepareBoundingBoxPool()
    {
        activeBoundingBoxes = new List<BoundingBox>();
        screenRectTransform = screen.GetComponent<RectTransform>();
        // TODO: Try folder path 
        boundingBoxPrefab = Resources.Load<GameObject>("Prefabs/BBox");

        if (boundingBoxPrefab != null)
        {
            screenWidth = screenRectTransform.rect.width;
            screenHeight = screenRectTransform.rect.height;
            boundingBoxPool = new ObjectPool<BoundingBox>(
                boundingBoxPrefab.GetComponent<BoundingBox>(),
                initialSize: 10,
                parent: screenRectTransform
            );
        }
    }
}
