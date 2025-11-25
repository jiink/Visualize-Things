using UnityEngine;

using System.Collections.Generic;
using System.Text;
using TMPro;

public class RLogger : MonoBehaviour
{
    public static RLogger Instance { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private TMP_Text _logText;

    [Header("Settings")]
    [Tooltip("If true, also prints to the Unity Editor Console")]
    [SerializeField] private bool _mirrorToConsole = true;

    [Tooltip("Maximum number of lines to keep before deleting old ones")]
    [SerializeField] private int _maxLogLines = 20;

    // Internal storage
    // We use a Queue to easily keep the 'Last N' messages
    private readonly Queue<string> _logQueue = new Queue<string>();
    private readonly StringBuilder _displayBuilder = new StringBuilder();
    private int _totalLogCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void Log(object message, LogType type = LogType.Log)
    {
        if ($"{message}".Contains("XrEventDataSpaceQueryCompleteFB"))
        {
            return;
        }
        // 1. Create the individual formatted line
        _totalLogCount++;
        string colorTag = null;

        switch (type)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                colorTag = "<color=red>";
                break;
            case LogType.Warning:
                colorTag = "<color=yellow>";
                break;
        }

        // Format: "15 | <color=red>Some Error</color>"
        string finalLine = $"{_totalLogCount} | {(colorTag != null ? colorTag : "")}{message}{(colorTag != null ? "</color>" : "")}";

        // 2. Add to Queue and Enforce Limit
        _logQueue.Enqueue(finalLine);

        while (_logQueue.Count > _maxLogLines)
        {
            _logQueue.Dequeue(); // Throw away the oldest line
        }

        // 3. Rebuild the text for UI
        UpdateUI();

        // 4. Mirror to Console
        if (_mirrorToConsole)
        {
            // Check if the message originated from internal Unity debug to avoid double logging
            if (!(message is string msg && msg.StartsWith("[InGameLog]")))
            {
                Debug.LogFormat(type, LogOption.None, this, "[InGameLog]: {0}", message);
            }
        }
    }

    private void UpdateUI()
    {
        if (_logText == null) return;

        _displayBuilder.Clear();

        foreach (string line in _logQueue)
        {
            _displayBuilder.AppendLine(line);
        }

        _logText.SetText(_displayBuilder);
        // Force scroll to bottom
        _logText.pageToDisplay = _logText.textInfo?.pageCount ?? 1;
    }

    public void Clear()
    {
        _logQueue.Clear();
        _displayBuilder.Clear();
        _totalLogCount = 0;
        if (_logText != null) _logText.text = "";
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleUnityLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        // Prevent infinite loop if we are mirroring to console
        if (_mirrorToConsole && logString.StartsWith("[InGameLog]")) return;

        Log(logString, type);
    }
}