using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private Button startDetectionButton;
    [SerializeField] private TMP_InputField confidenceThreshold;
    [SerializeField] private TMP_InputField iouThreshold;
    [SerializeField] private Detector detector;
    [SerializeField] private GameObject userInterface;
    [SerializeField] private GameObject display;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure the button is assigned
        if (startDetectionButton != null)
        {
            // Add a listener to the button
            startDetectionButton.onClick.AddListener(OnStartDetectionButtonClick);
        }
    }

    private void OnStartDetectionButtonClick()
    {
        float cTh = float.Parse(confidenceThreshold.text);
        float iouTh = float.Parse(iouThreshold.text);
        detector.StartDetection(cTh, iouTh);
        userInterface.SetActive(false);
        display.SetActive(true);
    }

}
