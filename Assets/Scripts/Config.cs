using UnityEngine;

public static class Config
{
    public static bool stepMode = true;
    public static string ip = "0.0.0.0";
    public static int port = 42001;
    public static bool enableManualPlay = true;

    public static int width = 8;
    public static int height = 17;
    public static int virusHeight = 4;
    public static int virusCount = 4;

    public static float cycleDuration = 0.5f;
    public static float fastCycleDuration = 0.1f;

    public static bool onlySingleColorPills = false;
};