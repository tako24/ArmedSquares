using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
public class MyLog : MonoBehaviour
{
    private string log;
    private const int MAXCHARS = 10000;
    private Queue myLogQueue = new Queue();
    void Start()
    {
        Debug.Log("Screen logger started");
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLogQueue.Enqueue("\n [" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue("\n" + stackTrace);
    }

    void Update()
    {
        log = "";
        myLogQueue.Enqueue("\n\n\n\n\n");
        while (myLogQueue.Count > 0)
            log = myLogQueue.Dequeue() + log;
        if (log.Length > MAXCHARS)
            log = log.Substring(0, MAXCHARS);
    }

    void OnGUI()
    {
        GUILayout.Label(log);
    }
}
#endif
