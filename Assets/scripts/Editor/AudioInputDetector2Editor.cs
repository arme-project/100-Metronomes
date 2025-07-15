using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioInputDetector2))]
public class AudioInputDetector2Editor : Editor
{
    private AudioInputDetector2 audioDetector;
    private float updateInterval = 0.03f; // Update visualization 30+ times per second
    private double lastUpdateTime = 0;

    // Cached GUI styles
    private GUIStyle waveformBoxStyle;
    private GUIStyle labelStyle;
    private bool stylesInitialized = false;

    private void OnEnable()
    {
        audioDetector = (AudioInputDetector2)target;
        EditorApplication.update += ForceInspectorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= ForceInspectorUpdate;
    }

    private void ForceInspectorUpdate()
    {
        // Force inspector to repaint at regular intervals for smooth visualization
        if (EditorApplication.timeSinceStartup - lastUpdateTime > updateInterval)
        {
            lastUpdateTime = EditorApplication.timeSinceStartup;
            Repaint();
        }
    }

    private void InitializeStyles()
    {
        if (stylesInitialized) return;

        waveformBoxStyle = new GUIStyle("box");
        waveformBoxStyle.padding = new RectOffset(5, 5, 5, 5);

        labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.fontSize = 10;

        stylesInitialized = true;
    }

    public override void OnInspectorGUI()
    {
        InitializeStyles();

        // Draw default inspector first
        DrawDefaultInspector();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Audio visualization is only available during Play Mode", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(20);

        // Get visualization settings
        var vizSettings = audioDetector.GetVisualizationSettings();

        // Audio Visualizations Section
        EditorGUILayout.LabelField("Audio Visualization", EditorStyles.boldLabel);

        // Draw Audio Bar
        if (vizSettings.showAudioBar)
        {
            DrawAudioBar();
        }

        EditorGUILayout.Space(10);

        // Draw Waveform
        if (vizSettings.showWaveform)
        {
            DrawWaveform();
        }
    }

    private void DrawAudioBar()
    {
        var vizSettings = audioDetector.GetVisualizationSettings();

        EditorGUILayout.LabelField("Volume Level", EditorStyles.miniLabel);

        Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,
            GUILayout.Height(30), GUILayout.ExpandWidth(true));

        // Draw background
        EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f, 1f));

        // Draw border
        Handles.color = Color.gray;
        Handles.DrawSolidRectangleWithOutline(barRect, Color.clear, Color.gray);

        // Get current audio values
        float audioValue = audioDetector.GetAudioBarValue();
        float audioPeak = audioDetector.GetAudioBarPeak();
        float threshold = audioDetector.GetDetectionThreshold();

        // Draw the audio level bar
        if (audioValue > 0.001f)
        {
            Rect valueRect = new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(audioValue), barRect.height);

            // Change color based on whether we're above threshold
            Color barColor = audioValue > threshold ? Color.yellow : vizSettings.audioBarColor;
            EditorGUI.DrawRect(valueRect, barColor);
        }

        // Draw peak indicator
        if (audioPeak > 0.001f)
        {
            float peakX = barRect.x + (barRect.width * Mathf.Clamp01(audioPeak));
            Rect peakRect = new Rect(peakX - 2, barRect.y, 4, barRect.height);
            EditorGUI.DrawRect(peakRect, vizSettings.audioBarPeakColor);
        }

        // Draw threshold line
        if (threshold > 0)
        {
            float thresholdX = barRect.x + (barRect.width * Mathf.Clamp01(threshold));
            Handles.color = Color.red;
            Handles.DrawLine(
                new Vector3(thresholdX, barRect.y, 0),
                new Vector3(thresholdX, barRect.y + barRect.height, 0)
            );

            // Draw threshold label
            GUI.Label(new Rect(thresholdX + 5, barRect.y, 100, 20), "Threshold", labelStyle);
        }

        // Draw value labels
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Level: {audioValue:F3}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Peak: {audioPeak:F3}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Threshold: {threshold:F3}", EditorStyles.miniLabel);

        // Show if currently detecting
        if (audioValue > threshold)
        {
            GUIStyle detectedStyle = new GUIStyle(EditorStyles.miniLabel);
            detectedStyle.normal.textColor = Color.yellow;
            EditorGUILayout.LabelField("ABOVE THRESHOLD!", detectedStyle);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawWaveform()
    {
        var vizSettings = audioDetector.GetVisualizationSettings();
        float[] waveformData = audioDetector.GetWaveformData();

        if (waveformData == null || waveformData.Length == 0)
            return;

        EditorGUILayout.LabelField("Waveform", EditorStyles.miniLabel);

        // Create waveform display area
        Rect waveformRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,
            GUILayout.Height(vizSettings.waveformHeight), GUILayout.ExpandWidth(true));

        // Draw background
        EditorGUI.DrawRect(waveformRect, vizSettings.waveformBackgroundColor);

        // Draw border
        Handles.color = Color.gray;
        Handles.DrawSolidRectangleWithOutline(waveformRect, Color.clear, Color.gray);

        // Draw center line
        float centerY = waveformRect.y + waveformRect.height * 0.5f;
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Handles.DrawLine(
            new Vector3(waveformRect.x, centerY, 0),
            new Vector3(waveformRect.x + waveformRect.width, centerY, 0)
        );

        // Draw waveform
        if (waveformData.Length > 1)
        {
            Handles.color = vizSettings.waveformColor;

            float xStep = waveformRect.width / (waveformData.Length - 1);

            // Create points for the waveform line
            Vector3[] points = new Vector3[waveformData.Length];

            for (int i = 0; i < waveformData.Length; i++)
            {
                float x = waveformRect.x + i * xStep;
                float normalizedValue = Mathf.Clamp01(waveformData[i] * 5f); // Scale for visibility
                float y = centerY - (normalizedValue * waveformRect.height * 0.4f);
                points[i] = new Vector3(x, y, 0);
            }

            // Draw the waveform as a polyline
            Handles.DrawPolyLine(points);

            // Also draw a mirrored version below the center line for a more traditional waveform look
            for (int i = 0; i < points.Length; i++)
            {
                points[i].y = centerY + (centerY - points[i].y);
            }
            Handles.DrawPolyLine(points);
        }

        // Draw threshold line on waveform
        float threshold = audioDetector.GetDetectionThreshold();
        if (threshold > 0)
        {
            float thresholdHeight = threshold * 5f * waveformRect.height * 0.4f; // Same scaling as waveform

            Handles.color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red

            // Upper threshold line
            float upperY = centerY - thresholdHeight;
            Handles.DrawLine(
                new Vector3(waveformRect.x, upperY, 0),
                new Vector3(waveformRect.x + waveformRect.width, upperY, 0)
            );

            // Lower threshold line
            float lowerY = centerY + thresholdHeight;
            Handles.DrawLine(
                new Vector3(waveformRect.x, lowerY, 0),
                new Vector3(waveformRect.x + waveformRect.width, lowerY, 0)
            );
        }
    }
}