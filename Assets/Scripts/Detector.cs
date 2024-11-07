using UnityEngine;
using Unity.Sentis;
using System.Collections.Generic;

public class Detector : MonoBehaviour
{
    public const int TARGET_WIDTH = 640;
    public const int TARGET_HEIGHT = 640;

    public FileLoader fileLoader;

    // Object Detection
    [SerializeField] private ModelAsset modelAsset;
    private Drawable screen;
    [SerializeField] private Model runtimeModel;
    private Worker worker;
    private Yolo yolo;
    private Source source = null;

    private void OnEnable()
    {
        fileLoader.OnFileSelected += OnSourceChanged;
    }
    private void OnDisable()
    {
        fileLoader.OnFileSelected -= OnSourceChanged;
        worker.Dispose();
    }

    void Start()
    {
        // Initialise Classes
        yolo = new Yolo();
        modelAsset = Resources.Load<ModelAsset>("Models/yolov9-c");
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
    }

    void FixedUpdate()
    {
        if (source == null || source.IsProcessedOnce())
            return;

        if (source.IsFrameReady())
        {
            DetectFrame();
        }
    }

    public void StartDetection(float cTh, float iouTh)
    {
        yolo.IouThreshold = iouTh;
        yolo.ConfidenceThreshold = cTh;

        screen = new Drawable(source);

        if (source.IsProcessedOnce())
        {
            DetectFrame();
        }
        else
        {
            source.Play();
        }
    }
    public void StopDetection()
    {
        source = null;
    }

    void OnSourceChanged(SourceType sourceType, string path)
    {
        if (sourceType == SourceType.ImageSource)
        {
            source = new ImageSource(path);
        }
        else if (sourceType == SourceType.VideoSource)
        {
            source = new VideoSource(path);
        }
        else
        {
            source = new CameraSource();
        }
    }
    private void DetectFrame()
    {
        // Get the newly generated texture
        Texture texture = source.GetTexture();

        // Remove the old bounding boxes
        screen.ResetBoundingBoxes();

        // Display the texture
        screen.SetTexture(texture);

        // Prepare the input tensor
        Tensor<float> inputTensor = TextureConverter.ToTensor(texture, TARGET_WIDTH, TARGET_HEIGHT, 3);

        // Run the model on the input
        worker.Schedule(inputTensor);

        // Get output tensor
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

        // Process Model Output
        List<YoloPrediction> predictions = yolo.Predict(outputTensor, TARGET_WIDTH, TARGET_HEIGHT);

        // Draw the new bounding boxes 
        screen.DrawBoundingBoxes(predictions);

        // Dispose tensors
        outputTensor.Dispose();
        inputTensor.Dispose();
    }
}


