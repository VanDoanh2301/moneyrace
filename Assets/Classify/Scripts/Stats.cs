using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Profiling;

public class Stats : MonoBehaviour
{
    public TextMeshProUGUI textComp;
    int frames = 0;
    float elapsedTime = 0;
    

    // Update is called once per frame
    void Update()
    {
        frames++;
        elapsedTime += Time.deltaTime;
        if (elapsedTime>=1)
        {
            elapsedTime -= 1;
            //string text = $"{frames} FPS \nGPU memory : {SystemInfo.graphicsMemorySize} \nSys memory : {SystemInfo.systemMemorySize}\nTotalAllocatedMemory : {Profiler.GetTotalAllocatedMemoryLong() / 1048576}mb\nTotalReservedMemory : {Profiler.GetTotalReservedMemoryLong() / 1048576}mb\nTotalUnusedReservedMemory : {Profiler.GetTotalUnusedReservedMemoryLong() / 1048576}mb";
            string text = $"{frames} FPS";
            textComp.text = text;
            frames = 0;
        }
    }
}
