using UnityEngine;
using UnityEngine.UI;
using Unity.Sentis;
using System.Collections.Generic;
using TMPro;

public class ImageResizer : MonoBehaviour
{
    private const int TARGET_WIDTH = 640;
    private const int TARGET_HEIGHT = 640;

    private float scaleX;
    private float scaleY;

    private int webcamWidth;
    private int webcamHeight;

    // Display
    public GameObject boundingBoxPrefab; // Prefab with a UI Image for bounding boxes
    public RawImage displayImage; // Reference to the RawImage displaying the image
    public RectTransform canvasRectTransform; // The Canvas RectTransform

    // Object Detection
    public ModelAsset modelAsset;
    public float confidenceThreshold = 0.5f;
    public float iouThreshold = 0.4f; // IoU threshold for Non-Max Suppression (NMS)
    private Model runtimeModel;
    private Worker worker;
    private YoloInference yoloInference;
    public ComputeShader postProcessingShader;

    private WebCamTexture webcamTexture;
    private RenderTexture renderTexture;
    private Texture2D cameraTexture2D;
    private List<GameObject> drawnPredictions = new List<GameObject>();

    private void OnEnable()
    {
        yoloInference = new YoloInference();
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
    }

    void Update()
    {
        // Check if the webcam texture has received a new frame
        if (webcamTexture.didUpdateThisFrame)
        {
            DestroyBoundingBoxes(drawnPredictions);

            displayImage.texture = webcamTexture;

            // Prepare the input tensor.
            //Tensor<float> inputTensor = TextureConverter.ToTensor(cameraTexture2D, TARGET_WIDTH, TARGET_HEIGHT);
            Tensor<float> inputTensor = TextureConverter.ToTensor(webcamTexture, TARGET_WIDTH, TARGET_HEIGHT, 3);

            // Run the model
            worker.Schedule(inputTensor);

            // Get output tensor
            Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

            // Process Model Output
            List<YoloPrediction> predictions = yoloInference.ProcessYoloOutput(postProcessingShader, outputTensor, TARGET_WIDTH, TARGET_HEIGHT, confidenceThreshold, iouThreshold);

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
            // Instantiate a bounding box prefab
            GameObject boundingBox = Instantiate(boundingBoxPrefab, canvasRectTransform);
            drawnPredictions.Add(boundingBox);

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

            // Set the outline color instead of filling the box
            Image image = boundingBox.GetComponent<Image>();
            if (image != null)
            {
                image.color = prediction.ClassColor;
            }

            // Optionally set the label (class name) and score on top of the bounding box
            TMP_Text classNameText = boundingBox.GetComponentInChildren<TMP_Text>();
            if (classNameText != null)
            {
                classNameText.text = $"{prediction.ClassName} ({prediction.Score:F2})";
            }
        }
    }

    private void DestroyBoundingBoxes(List<GameObject> predictions)
    {
        if (predictions.Count == 0)
            return;

        foreach (GameObject box in predictions)
        {
            Destroy(box);
        }
    }
    private void OnDisable()
    {
        worker.Dispose();
    }
}


