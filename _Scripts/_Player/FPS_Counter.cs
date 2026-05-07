using UnityEngine;
using LightSide;

public class FpsCounter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private UniText fpsText;

    private float lastUpdate;
    private int frameCount;
    private float currentFps;

    void Start()
    {
        if (fpsText == null)
            enabled = false;
    }

    void Update()
    {
        frameCount++;

        if (Time.time - lastUpdate >= updateInterval)
        {
            currentFps = frameCount / (Time.time - lastUpdate);
            fpsText.Text = $"FPS: {Mathf.Round(currentFps)}";

            frameCount = 0;
            lastUpdate = Time.time;
        }
    }
}