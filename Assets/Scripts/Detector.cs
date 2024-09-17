using UnityEngine;
using UnityEngine.UI;
using Unity.Sentis;
using System.Collections.Generic;
using TMPro;

public class Detector : MonoBehaviour
{
    private const int TARGET_WIDTH = 640;
    private const int TARGET_HEIGHT = 640;

    private float scaleX;
    private float scaleY;

    private int webcamWidth;
    private int webcamHeight;

    // Display
    [SerializeField] private GameObject boundingBoxPrefab; // Prefab with a UI Image for bounding boxes
    [SerializeField] private RawImage displayImage; // Reference to the RawImage displaying the image

    // Object Detection
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private float confidenceThreshold = 0.5f;
    [SerializeField] private float iouThreshold = 0.4f; // IoU threshold for Non-Max Suppression (NMS)
    private Model runtimeModel;
    private Worker worker;
    private YoloInference yoloInference;

    private WebCamTexture webcamTexture;
    private List<BoundingBox> activeBoundingBoxes = new List<BoundingBox>();

    // Object Pooling
    private ObjectPool<BoundingBox> boundingBoxPool;
    private void OnEnable()
    {
        yoloInference = new YoloInference(confidenceThreshold, iouThreshold); 
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
    }

    void Start()
    {
        // Initialize the webcam texture
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();

        // Assign the webcam texture to the RawImage
        displayImage.texture = webcamTexture;

        // Initialize the object pool
        RectTransform rawImageRectTransform = displayImage.GetComponent<RectTransform>();
        boundingBoxPool = new ObjectPool<BoundingBox>(
            boundingBoxPrefab.GetComponent<BoundingBox>(),
            initialSize: 10,
            parent: rawImageRectTransform
        );
    }

    void Update()
    {
        // Check if the webcam texture has received a new frame
        if (webcamTexture.didUpdateThisFrame)
        {
            ResetBoundingBoxes();

            displayImage.texture = webcamTexture;

            // Prepare the input tensor.
            Tensor<float> inputTensor = TextureConverter.ToTensor(webcamTexture, TARGET_WIDTH, TARGET_HEIGHT, 3);

            // Run the model
            worker.Schedule(inputTensor);

            // Get output tensor
            Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

            // Process Model Output
            List<YoloPrediction> predictions = yoloInference.ProcessYoloOutput(outputTensor, TARGET_WIDTH, TARGET_HEIGHT);

            // Display Result 
            DrawBoundingBoxes(predictions);

            // Dispose tensors
            outputTensor.Dispose();
            inputTensor.Dispose();
        }
    }

    void DrawBoundingBoxes(List<YoloPrediction> yoloPredictions)
    {
        // Get the dimensions of the RawImage (should be 640x640 in this case)
        RectTransform rawImageRectTransform = displayImage.GetComponent<RectTransform>();
        float rawImageWidth = rawImageRectTransform.rect.width;
        float rawImageHeight = rawImageRectTransform.rect.height;

        // Calculate the offset for center-middle anchoring
        float offsetX = rawImageWidth / 2;
        float offsetY = rawImageHeight / 2;

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

    private void ResetBoundingBoxes()
    {
        foreach (BoundingBox box in activeBoundingBoxes)
        {
            boundingBoxPool.ReturnToPool(box);
        }
        activeBoundingBoxes.Clear();
    }

    private void OnDisable()
    {
        worker.Dispose();
    }
}


