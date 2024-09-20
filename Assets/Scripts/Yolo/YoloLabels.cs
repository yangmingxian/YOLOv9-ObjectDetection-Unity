using System.Collections.Generic;
using UnityEngine;

public class YoloLables
{
    private static float alpha = 0.25f;

    private static readonly List<string>  ClassNames = new List<string>
    {
        "person",
        "bicycle",
        "car",
        "motorcycle",
        "airplane",
        "bus",
        "train",
        "truck",
        "boat",
        "traffic light",
        "fire hydrant",
        "stop sign",
        "parking meter",
        "bench",
        "bird",
        "cat",
        "dog",
        "horse",
        "sheep",
        "cow",
        "elephant",
        "bear",
        "zebra",
        "giraffe",
        "backpack",
        "umbrella",
        "handbag",
        "tie",
        "suitcase",
        "frisbee",
        "skis",
        "snowboard",
        "sports ball",
        "kite",
        "baseball bat",
        "baseball glove",
        "skateboard",
        "surfboard",
        "tennis racket",
        "bottle",
        "wine glass",
        "cup",
        "fork",
        "knife",
        "spoon",
        "bowl",
        "banana",
        "apple",
        "sandwich",
        "orange",
        "broccoli",
        "carrot",
        "hot dog",
        "pizza",
        "donut",
        "cake",
        "chair",
        "couch",
        "potted plant",
        "bed",
        "dining table",
        "toilet",
        "tv",
        "laptop",
        "mouse",
        "remote",
        "keyboard",
        "cell phone",
        "microwave",
        "oven",
        "toaster",
        "sink",
        "refrigerator",
        "book",
        "clock",
        "vase",
        "scissors",
        "teddy bear",
        "hair drier",
        "toothbrush"
    };

    private static List<Color> ClassColors = new List<Color>
    {
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 1
        new Color(0.850f, 0.325f, 0.098f, alpha),  // Class 2
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 3
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 4
        new Color(0.635f, 0.078f, 0.184f, alpha),  // Class 5
        new Color(0.000f, 0.447f, 0.741f, alpha),  // Class 6
        new Color(0.850f, 0.125f, 0.098f, alpha),  // Class 7
        new Color(0.929f, 0.694f, 0.125f, alpha),  // Class 8
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 9
        new Color(0.635f, 0.078f, 0.184f, alpha),  // Class 10
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 11
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 12
        new Color(0.635f, 0.078f, 0.184f, alpha),  // Class 13
        new Color(0.000f, 0.447f, 0.741f, alpha),  // Class 14
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 15
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 16
        new Color(0.850f, 0.125f, 0.098f, alpha),  // Class 17
        new Color(0.929f, 0.694f, 0.125f, alpha),  // Class 18
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 19
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 20
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 21
        new Color(0.635f, 0.078f, 0.184f, alpha),  // Class 22
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 23
        new Color(0.000f, 0.447f, 0.741f, alpha),  // Class 24
        new Color(0.850f, 0.125f, 0.098f, alpha),  // Class 25
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 26
        new Color(0.929f, 0.694f, 0.125f, alpha),  // Class 27
        new Color(0.635f, 0.078f, 0.184f, alpha),  // Class 28
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 29
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 30
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 31
        new Color(0.000f, 0.447f, 0.741f, alpha),  // Class 32
        new Color(0.850f, 0.325f, 0.098f, alpha),  // Class 33
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 34
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 35
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 36
        new Color(0.850f, 0.125f, 0.098f, alpha),  // Class 37
        new Color(0.635f, 0.078f, 0.184f, alpha),  // Class 38
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 39
        new Color(0.000f, 0.447f, 0.741f, alpha),  // Class 40
        new Color(0.850f, 0.325f, 0.098f, alpha),  // Class 41
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 42
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 43
        new Color(0.929f, 0.694f, 0.125f, alpha),  // Class 44
        new Color(0.850f, 0.125f, 0.098f, alpha),  // Class 45
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 46
        new Color(0.635f, 0.078f, 0.184f, alpha),  // Class 47
        new Color(0.000f, 0.447f, 0.741f, alpha),  // Class 48
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 49
        new Color(0.850f, 0.325f, 0.098f, alpha),  // Class 50
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 51
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 52
        new Color(0.635f, 0.078f, 0.184f, alpha),  // Class 53
        new Color(0.850f, 0.125f, 0.098f, alpha),  // Class 54
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 55
        new Color(0.000f, 0.447f, 0.741f, alpha),  // Class 56
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 57
        new Color(0.850f, 0.125f, 0.098f, alpha),  // Class 58
        new Color(0.929f, 0.694f, 0.125f, alpha),  // Class 59
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 60
        new Color(0.850f, 0.325f, 0.098f, alpha),  // Class 61
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 62
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 63
        new Color(0.635f, 0.078f, 0.184f, alpha),  // Class 64
        new Color(0.000f, 0.447f, 0.741f, alpha),  // Class 65
        new Color(0.850f, 0.125f, 0.098f, alpha),  // Class 66
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 67
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 68
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 69
        new Color(0.850f, 0.325f, 0.098f, alpha),  // Class 70
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 71
        new Color(0.850f, 0.125f, 0.098f, alpha),  // Class 72
        new Color(0.000f, 0.447f, 0.741f, alpha),  // Class 73
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 74
        new Color(0.635f, 0.078f, 0.184f, alpha),  // Class 75
        new Color(0.929f, 0.490f, 0.192f, alpha),  // Class 76
        new Color(0.850f, 0.125f, 0.098f, alpha),  // Class 77
        new Color(0.466f, 0.674f, 0.188f, alpha),  // Class 78
        new Color(0.301f, 0.745f, 0.933f, alpha),  // Class 79
        new Color(0.929f, 0.694f, 0.125f, alpha),  // Class 80
    };
    public static string GetClassName(int classIndex) { return ClassNames[classIndex]; }
    public static Color GetClassColor(int classIndex) { return ClassColors[classIndex]; }
}
