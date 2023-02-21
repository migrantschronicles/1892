using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
#if DEBUG && !UNITY_EDITOR
    static string myLog = "";
    private string output;
    private string stack;

    private void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    private void Log(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        myLog = output + "\n" + stack + "\n" + myLog;
        if (myLog.Length > 5000)
        {
            myLog = myLog.Substring(0, 4000);
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, Screen.width - 10, Screen.height - 10), myLog);
    }
#endif
}
