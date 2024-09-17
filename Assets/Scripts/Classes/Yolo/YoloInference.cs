
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEngine;

public class YoloInference
{
    private float confidenceThreshold;
    private float iouThreshold;

    public YoloInference(float confidenceThreshold = 0.5f, float iouThreshold = 0.4f)
    {
        this.confidenceThreshold = confidenceThreshold;
        this.iouThreshold = iouThreshold;
    }

    public List<YoloPrediction> ProcessYoloOutput(Tensor<float> outputTensor, int imageWidth, int imageHeight)
    {
        ComputeShader postProcessingShader = Resources.Load<ComputeShader>("PostProcessOutput");
        if (postProcessingShader == null)
        {
            Debug.LogError("Compute Shader not found!");
            return null;
        }

        ComputeTensorData computeTensorData = ComputeTensorData.Pin(outputTensor);
        if (computeTensorData == null)
        {
            Debug.LogError("Output Tensor not found!");
            return null;
        }
            

        int numDetections = outputTensor.shape[2];
        int numClasses = 80;  // COCO dataset
        // Initialize the valid detection counter to 0
        uint[] zeroArray = { 0 };

        // Create compute buffers for input/output
        ComputeBuffer outputBoxesBuffer = new ComputeBuffer(numDetections, sizeof(float) * 4);  // Bounding boxes buffer
        ComputeBuffer outputClassesBuffer = new ComputeBuffer(numDetections, sizeof(int));  // Class index buffer
        ComputeBuffer outputScoresBuffer = new ComputeBuffer(numDetections, sizeof(float));  // Scores buffer
        ComputeBuffer validDetectionCounterBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Raw);  // Counter for valid detections
        ComputeBuffer outputTensorBuffer = computeTensorData.buffer;
        // Set compute shader parameters
        int kernelHandle = postProcessingShader.FindKernel("CSMain");
        postProcessingShader.SetFloat("confidenceThreshold", confidenceThreshold);
        postProcessingShader.SetInt("numDetections", numDetections);
        postProcessingShader.SetInt("numClasses", numClasses);
        postProcessingShader.SetInt("imageWidth", imageWidth);
        postProcessingShader.SetInt("imageHeight", imageHeight);
        validDetectionCounterBuffer.SetData(zeroArray);

        // Bind buffers to the compute shader
        postProcessingShader.SetBuffer(kernelHandle, "outputTensor", outputTensorBuffer);
        postProcessingShader.SetBuffer(kernelHandle, "outputBoxes", outputBoxesBuffer);
        postProcessingShader.SetBuffer(kernelHandle, "outputClasses", outputClassesBuffer);
        postProcessingShader.SetBuffer(kernelHandle, "outputScores", outputScoresBuffer);
        postProcessingShader.SetBuffer(kernelHandle, "validDetectionCounter", validDetectionCounterBuffer);

        // Dispatch compute shader (numDetections / thread group size)
        int threadGroups = Mathf.CeilToInt((float)numDetections / 256);
        postProcessingShader.Dispatch(kernelHandle, threadGroups, 1, 1);

        // Retrieve the valid detection count from the counter buffer
        uint[] validDetectionCountArray = new uint[1];
        validDetectionCounterBuffer.GetData(validDetectionCountArray);
        int validDetectionCount = (int)validDetectionCountArray[0];

        // Retrieve results from the GPU
        float[] boxes = new float[validDetectionCount * 4];
        int[] classes = new int[validDetectionCount];
        float[] scores = new float[validDetectionCount];

        outputBoxesBuffer.GetData(boxes, 0, 0, validDetectionCount * 4);
        outputClassesBuffer.GetData(classes, 0, 0, validDetectionCount);
        outputScoresBuffer.GetData(scores, 0, 0, validDetectionCount);

        // Convert output to YoloPrediction list
        List<YoloPrediction> predictions = new List<YoloPrediction>();
        for (int i = 0; i < validDetectionCount; i++)
        {
            float xMin = boxes[i * 4 + 0];
            float yMin = boxes[i * 4 + 1];
            float width = boxes[i * 4 + 2];
            float height = boxes[i * 4 + 3];

            YoloPrediction prediction = new YoloPrediction
            {
                ClassIndex = classes[i],
                ClassName = YoloLables.GetClassName(classes[i]),
                ClassColor = YoloLables.GetClassColor(classes[i]),
                Score = scores[i],
                BoundingBox = new Rect(xMin, yMin, width, height)
            };
            predictions.Add(prediction);
        }

        // Release compute buffers
        outputTensorBuffer.Release();
        outputBoxesBuffer.Release();
        outputClassesBuffer.Release();
        outputScoresBuffer.Release();
        validDetectionCounterBuffer.Release();
        computeTensorData.Dispose();

        return ApplyNonMaxSuppression(predictions, iouThreshold);
    }

    public List<YoloPrediction> ApplyNonMaxSuppression(List<YoloPrediction> predictions, float iouThreshold)
    {
        ComputeShader nmsShader = Resources.Load<ComputeShader>("NonMaxSuppression");
        if (nmsShader == null)
        {
            Debug.LogError("NMS Compute Shader not found!");
            return null;
        }

        int numDetections = predictions.Count;
        if (numDetections == 0)
            return new List<YoloPrediction>();

        // Prepare data arrays
        float[] boxes = new float[numDetections * 4];
        float[] scores = new float[numDetections];
        int[] classes = new int[numDetections];
        int[] selectedIndices = new int[numDetections];

        for (int i = 0; i < numDetections; i++)
        {
            var bbox = predictions[i].BoundingBox;
            boxes[i * 4 + 0] = bbox.xMin;
            boxes[i * 4 + 1] = bbox.yMin;
            boxes[i * 4 + 2] = bbox.width;
            boxes[i * 4 + 3] = bbox.height;
            scores[i] = predictions[i].Score;
            classes[i] = predictions[i].ClassIndex;
            selectedIndices[i] = 1; // Initialize all to selected
        }

        // Create compute buffers
        ComputeBuffer boxesBuffer = new ComputeBuffer(numDetections, sizeof(float) * 4);
        ComputeBuffer scoresBuffer = new ComputeBuffer(numDetections, sizeof(float));
        ComputeBuffer classesBuffer = new ComputeBuffer(numDetections, sizeof(int));
        ComputeBuffer selectedBuffer = new ComputeBuffer(numDetections, sizeof(int));

        // Set data
        boxesBuffer.SetData(boxes);
        scoresBuffer.SetData(scores);
        classesBuffer.SetData(classes);
        selectedBuffer.SetData(selectedIndices);

        // Set shader parameters
        int kernelHandle = nmsShader.FindKernel("CSMain");
        nmsShader.SetInt("numDetections", numDetections);
        nmsShader.SetFloat("iouThreshold", iouThreshold);

        // Bind buffers
        nmsShader.SetBuffer(kernelHandle, "inputBoxes", boxesBuffer);
        nmsShader.SetBuffer(kernelHandle, "inputScores", scoresBuffer);
        nmsShader.SetBuffer(kernelHandle, "inputClasses", classesBuffer);
        nmsShader.SetBuffer(kernelHandle, "selectedIndices", selectedBuffer);

        // Dispatch shader
        int threadGroups = Mathf.CeilToInt((float)numDetections / 256);
        nmsShader.Dispatch(kernelHandle, threadGroups, 1, 1);

        // Get results
        selectedBuffer.GetData(selectedIndices);

        // Collect selected predictions
        List<YoloPrediction> result = new List<YoloPrediction>();
        for (int i = 0; i < numDetections; i++)
        {
            if (selectedIndices[i] == 1)
            {
                result.Add(predictions[i]);
            }
        }

        // Release buffers
        boxesBuffer.Release();
        scoresBuffer.Release();
        classesBuffer.Release();
        selectedBuffer.Release();

        return result;
    }

    private float CalculateIoU(Rect boxA, Rect boxB)
    {
        float intersectionWidth = Mathf.Max(0, Mathf.Min(boxA.xMax, boxB.xMax) - Mathf.Max(boxA.xMin, boxB.xMin));
        float intersectionHeight = Mathf.Max(0, Mathf.Min(boxA.yMax, boxB.yMax) - Mathf.Max(boxA.yMin, boxB.yMin));
        float intersectionArea = intersectionWidth * intersectionHeight;

        float unionArea = boxA.width * boxA.height + boxB.width * boxB.height - intersectionArea;
        return intersectionArea / unionArea;
    }
}
