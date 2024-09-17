using System.Collections.Generic;
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
        ComputeBuffer outputClassesBuffer = new ComputeBuffer(numDetections, sizeof(int));      // Class index buffer
        ComputeBuffer outputScoresBuffer = new ComputeBuffer(numDetections, sizeof(float));     // Scores buffer
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

        // Dispatch compute shader
        int threadGroups = Mathf.CeilToInt((float)numDetections / 256);
        postProcessingShader.Dispatch(kernelHandle, threadGroups, 1, 1);

        // Retrieve the valid detection count from the counter buffer
        uint[] validDetectionCountArray = new uint[1];
        validDetectionCounterBuffer.GetData(validDetectionCountArray);
        int validDetectionCount = (int)validDetectionCountArray[0];

        if (validDetectionCount == 0)
        {
            // Release resources
            outputBoxesBuffer.Release();
            outputClassesBuffer.Release();
            outputScoresBuffer.Release();
            validDetectionCounterBuffer.Release();
            computeTensorData.Dispose();
            outputTensorBuffer.Release();

            return new List<YoloPrediction>();
        }

        // Pass the buffers directly to the NMS shader
        List<YoloPrediction> predictions = ApplyNonMaxSuppression(
            outputBoxesBuffer, outputClassesBuffer, outputScoresBuffer, validDetectionCount);

        // Release resources
        outputBoxesBuffer.Release();
        outputClassesBuffer.Release();
        outputScoresBuffer.Release();
        validDetectionCounterBuffer.Release();
        computeTensorData.Dispose();
        outputTensorBuffer.Release();

        return predictions;
    }

    public List<YoloPrediction> ApplyNonMaxSuppression(
        ComputeBuffer boxesBuffer, ComputeBuffer classesBuffer, ComputeBuffer scoresBuffer, int numDetections)
    {
        ComputeShader nmsShader = Resources.Load<ComputeShader>("NonMaxSuppression");
        if (nmsShader == null)
        {
            Debug.LogError("NMS Compute Shader not found!");
            return null;
        }

        if (numDetections == 0)
            return new List<YoloPrediction>();

        // Create output buffers for the NMS shader
        ComputeBuffer nmsBoxesBuffer = new ComputeBuffer(numDetections, sizeof(float) * 4);
        ComputeBuffer nmsClassesBuffer = new ComputeBuffer(numDetections, sizeof(int));
        ComputeBuffer nmsScoresBuffer = new ComputeBuffer(numDetections, sizeof(float));

        // Create an atomic counter for the number of detections after NMS
        ComputeBuffer nmsDetectionCounterBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Raw);
        uint[] zeroArray = { 0 };
        nmsDetectionCounterBuffer.SetData(zeroArray);

        // Create a buffer for selected indices
        ComputeBuffer selectedIndicesBuffer = new ComputeBuffer(numDetections, sizeof(int));
        // Initialize selectedIndices with zeros
        int[] selectedIndicesInit = new int[numDetections];
        selectedIndicesInit[0] = 1; // Initialize first index to 1
        selectedIndicesBuffer.SetData(selectedIndicesInit);

        // Set shader parameters
        int kernelHandle = nmsShader.FindKernel("CSMain");
        nmsShader.SetInt("numDetections", numDetections);
        nmsShader.SetFloat("iouThreshold", iouThreshold);

        // Bind buffers
        nmsShader.SetBuffer(kernelHandle, "inputBoxes", boxesBuffer);
        nmsShader.SetBuffer(kernelHandle, "inputScores", scoresBuffer);
        nmsShader.SetBuffer(kernelHandle, "inputClasses", classesBuffer);
        nmsShader.SetBuffer(kernelHandle, "outputBoxes", nmsBoxesBuffer);
        nmsShader.SetBuffer(kernelHandle, "outputScores", nmsScoresBuffer);
        nmsShader.SetBuffer(kernelHandle, "outputClasses", nmsClassesBuffer);
        nmsShader.SetBuffer(kernelHandle, "nmsDetectionCounter", nmsDetectionCounterBuffer);
        nmsShader.SetBuffer(kernelHandle, "selectedIndices", selectedIndicesBuffer);

        // Dispatch shader
        int threadGroups = Mathf.CeilToInt((float)numDetections / 256);
        nmsShader.Dispatch(kernelHandle, threadGroups, 1, 1);

        // Get the number of detections after NMS
        uint[] nmsDetectionCountArray = new uint[1];
        nmsDetectionCounterBuffer.GetData(nmsDetectionCountArray);
        int nmsDetectionCount = (int)nmsDetectionCountArray[0];

        if (nmsDetectionCount == 0)
        {
            // Release buffers
            nmsBoxesBuffer.Release();
            nmsScoresBuffer.Release();
            nmsClassesBuffer.Release();
            nmsDetectionCounterBuffer.Release();
            selectedIndicesBuffer.Release();

            return new List<YoloPrediction>();
        }

        // Retrieve results from the GPU
        float[] boxes = new float[nmsDetectionCount * 4];
        int[] classes = new int[nmsDetectionCount];
        float[] scores = new float[nmsDetectionCount];

        nmsBoxesBuffer.GetData(boxes, 0, 0, nmsDetectionCount * 4);
        nmsClassesBuffer.GetData(classes, 0, 0, nmsDetectionCount);
        nmsScoresBuffer.GetData(scores, 0, 0, nmsDetectionCount);

        // Convert output to YoloPrediction list
        List<YoloPrediction> result = new List<YoloPrediction>();
        for (int i = 0; i < nmsDetectionCount; i++)
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
            result.Add(prediction);
        }

        // Release buffers
        nmsBoxesBuffer.Release();
        nmsScoresBuffer.Release();
        nmsClassesBuffer.Release();
        nmsDetectionCounterBuffer.Release();
        selectedIndicesBuffer.Release();

        return result;
    }
}
