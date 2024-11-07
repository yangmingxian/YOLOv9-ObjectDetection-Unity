using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private Button startDetectionButton;
    [SerializeField] private Button stopDetectionButton;
    [SerializeField] private Button openFileSelectonButton;
    [SerializeField] private TMP_InputField confidenceThreshold;
    [SerializeField] private TMP_InputField iouThreshold;
    [SerializeField] private TMP_Dropdown sourceTypeSelector;
    [SerializeField] private Detector detector;
    [SerializeField] private FileLoader fileLoader;
    [SerializeField] private GameObject userInterface;
    [SerializeField] private GameObject display;
    [SerializeField] private GameObject fileSelectorPrefab;
    [SerializeField] private GameObject watermark;




    SourceType curSourceType;

    public TMP_Text pathText;
    private void OnEnable()
    {
        fileLoader.OnFileSelected += FreshPathInfo;
    }
    private void OnDisable()
    {
        fileLoader.OnFileSelected -= FreshPathInfo;
    }
    void FreshPathInfo(SourceType sourceType, string path)
    {
        curSourceType = sourceType;
        string fileName = Path.GetFileName(path);
        if (pathText != null)
        {
            switch (sourceType)
            {
                case SourceType.ImageSource:
                    pathText.text = $"数据源类型: 图片\n数据源名称: {fileName}";
                    break;
                case SourceType.VideoSource:
                    pathText.text = $"数据源类型: 视频\n数据源名称: {fileName}";
                    break;
                default:
                    pathText.text = "";
                    break;
            }
        }
    }

    private List<SourceType> sourceTypes = new() { SourceType.ImageSource, SourceType.CameraSource, SourceType.VideoSource };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure the button is assigned
        if (startDetectionButton != null)
        {
            // Add a listener to the button
            startDetectionButton.onClick.AddListener(OnStartDetectionButtonClick);
        }

        if (stopDetectionButton != null)
        {
            // Add a listener to the button
            stopDetectionButton.onClick.AddListener(OnStopDetectionButtonClick);
        }

        // Ensure the button is assigned
        if (openFileSelectonButton != null)
        {
            // Add a listener to the button
            openFileSelectonButton.onClick.AddListener(OnOpenFileSelectonButton);
        }

        if (sourceTypeSelector != null)
        {
            sourceTypeSelector.onValueChanged.AddListener(OnSourceTypeChanged);
        }
    }
    /// <summary>
    /// 通用场景：confidenceThreshold = 0.5，iouThreshold = 0.5
    // 检测准确性优先（减少误检）：confidenceThreshold = 0.6，iouThreshold = 0.5
    // 检测灵敏度优先（减少漏检）：confidenceThreshold = 0.3，iouThreshold = 0.4
    /// </summary>
    private void OnStartDetectionButtonClick()
    {
        float cTh = 0.5f, iouTh = 0.5f;
        if (!string.IsNullOrEmpty(confidenceThreshold.text))
            cTh = float.Parse(confidenceThreshold.text);
        if (!string.IsNullOrEmpty(iouThreshold.text))
            iouTh = float.Parse(iouThreshold.text);

        // 没有Path的时候就不会进入识别
        if (curSourceType != SourceType.CameraSource && string.IsNullOrEmpty(pathText.text))
            return;

        userInterface.SetActive(false);
        display.SetActive(true);
        detector.StartDetection(cTh, iouTh);
    }

    private void OnStopDetectionButtonClick()
    {
        pathText.text = "";
        detector.StopDetection();
        userInterface.SetActive(true);
        display.SetActive(false);
    }

    private void OnSourceTypeChanged(int value)
    {
        var sourceType = sourceTypes[value];
        if (sourceType == SourceType.CameraSource && openFileSelectonButton.gameObject.activeSelf)
        {
            openFileSelectonButton.gameObject.SetActive(false);
        }
        else if (((sourceType == SourceType.VideoSource) || (sourceType == SourceType.ImageSource)) && (!openFileSelectonButton.gameObject.activeSelf))
        {
            openFileSelectonButton.gameObject.SetActive(true);
        }
        fileLoader.SetDefaultFilter(sourceType);
    }

    private void OnOpenFileSelectonButton()
    {
        fileSelectorPrefab.SetActive(true);
        fileLoader.OpenFileBrowser();
    }

}
