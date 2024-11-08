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
    private Model runtimeModel;
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
        if (modelAsset == null)
            modelAsset = Resources.Load<ModelAsset>("Models/yolov8n");
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
        screen.ResetBoundingBoxes();
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
        // 获取数据源输入
        Texture texture = source.GetTexture();

        // 清除识别框
        screen.ResetBoundingBoxes();

        // 数据源显示
        screen.SetTexture(texture);

        // 输入张量
        Tensor<float> inputTensor = TextureConverter.ToTensor(texture, TARGET_WIDTH, TARGET_HEIGHT, 3);

        // 运行模型
        worker.Schedule(inputTensor);

        // 获取输出张量
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

        // 处理模型的输出
        List<YoloPrediction> predictions = yolo.Predict(outputTensor, TARGET_WIDTH, TARGET_HEIGHT);

        // 绘制识别框
        screen.DrawBoundingBoxes(predictions);

        // 释放非托管的张量资源
        outputTensor.Dispose();
        inputTensor.Dispose();
    }
}


