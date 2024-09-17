
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEngine;

public class YoloInference
{
    private ComputeBuffer outputTensorBuffer, outputBoxesBuffer, outputClassesBuffer, outputScoresBuffer, validDetectionCounterBuffer;
    
    public List<YoloPrediction> ProcessYoloOutput(ComputeShader postProcessingShader, Tensor<float> outputTensor, int imageWidth, int imageHeight, float confidenceThreshold, float iouThreshold)
    {
        int numDetections = outputTensor.shape[2];
        int numClasses = 80;  // COCO dataset

        // Create compute buffers for input/output
        outputBoxesBuffer = new ComputeBuffer(numDetections, sizeof(float) * 4);  // Bounding boxes buffer
        outputClassesBuffer = new ComputeBuffer(numDetections, sizeof(int));  // Class index buffer
        outputScoresBuffer = new ComputeBuffer(numDetections, sizeof(float));  // Scores buffer
        validDetectionCounterBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Raw);  // Counter for valid detections


        // Initialize the valid detection counter to 0
        uint[] zeroArray = { 0 };
        validDetectionCounterBuffer.SetData(zeroArray);

        ComputeTensorData computeTensorData = ComputeTensorData.Pin(outputTensor);
        if (computeTensorData != null)
        {
            outputTensorBuffer = computeTensorData.buffer; // YOLO output tensor buffer
        }

        // Set compute shader parameters
        int kernelHandle = postProcessingShader.FindKernel("CSMain");
        postProcessingShader.SetFloat("confidenceThreshold", confidenceThreshold);
        postProcessingShader.SetInt("numDetections", numDetections);
        postProcessingShader.SetInt("numClasses", numClasses);
        postProcessingShader.SetInt("imageWidth", imageWidth);
        postProcessingShader.SetInt("imageHeight", imageHeight);

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
        List<YoloPrediction> result = new List<YoloPrediction>();

        foreach (var prediction in predictions.OrderByDescending(p => p.Score))
        {
            bool shouldSelect = true;

            foreach (var selectedPrediction in result)
            {
                float iou = CalculateIoU(prediction.BoundingBox, selectedPrediction.BoundingBox);
                if (iou > iouThreshold)
                {
                    shouldSelect = false;
                    break;
                }
            }

            if (shouldSelect)
            {
                result.Add(prediction);
            }
        }

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
