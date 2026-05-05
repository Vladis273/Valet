using UnityEngine;

public class Tracer : MonoBehaviour
{
    public float lifetime = 0.2f;
    public float fadeSpeed = 0.5f;
    public float width = 0.05f;
    public Color startColor = Color.yellow;
    public Color endColor = Color.clear;

    private LineRenderer lineRenderer;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float creationTime;
    private float animationDuration;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            
            Shader tracerShader = Shader.Find("Unlit/Color");
            if (tracerShader == null)
            {
                tracerShader = Shader.Find("Sprites/Default");
                if (tracerShader == null)
                    Debug.LogWarning("[Tracer] Could not find a suitable shader. Please assign one manually.");
            }
            
            lineRenderer.material = new Material(tracerShader);
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }
    }

    public void Initialize(Vector3 start, Vector3 end, Color color, float width = 0.05f)
    {
        startPoint = start;
        endPoint = end;
        this.width = width;
        startColor = color;

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;

        creationTime = Time.time;
        animationDuration = lifetime;

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, startPoint);
    }

    void Update()
    {
        float elapsed = Time.time - creationTime;
        float progress = Mathf.Clamp01(elapsed / animationDuration);

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        float t = Mathf.SmoothStep(0f, 1f, progress);
        Vector3 currentStart = Vector3.Lerp(startPoint, endPoint, t);
        Vector3 currentEnd = endPoint;

        lineRenderer.SetPosition(0, currentStart);
        lineRenderer.SetPosition(1, currentEnd);

        float pulse = Mathf.Sin(Time.time * 20f) * 0.1f + 1f;
        lineRenderer.startWidth = width * pulse;
        lineRenderer.endWidth = width * pulse * 0.5f;

        float fade = 1f - Mathf.Pow(progress, 1f / fadeSpeed);
        lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, startColor.a * fade);
        lineRenderer.endColor = new Color(endColor.r, endColor.g, endColor.b, endColor.a * fade);
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}