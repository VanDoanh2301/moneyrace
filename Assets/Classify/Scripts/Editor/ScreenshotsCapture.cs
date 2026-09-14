using UnityEditor;
using UnityEngine;

public class ScreenshotsCapture 
{
    [MenuItem("Tools/CaptureSceenshot")]
    static void CaptureSceenshot()
    {
        ScreenCapture.CaptureScreenshot(System.DateTime.Now.ToString("ddMMyyHHmmss") + ".png");
    }

    [MenuItem("Tools/Delete All Data")]
    static void DeleteData()
    {
        PlayerPrefs.DeleteAll();
    }
}
