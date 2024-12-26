using UnityEngine;

public class Visual : MonoBehaviour
{
    public static void Marker(Vector3 pos, float radius)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.transform.position = pos;

        marker.transform.localScale = new Vector3(radius, 100 / 2, radius);

        Destroy(marker, 5f);
    }

    public static void Sphere(Vector3 pos, float radius)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;

        Destroy(sphere, 5f);
    }

    public static void Circle(Vector3 pos, float radius)
    {
        GameObject circle = new GameObject("DebugCircle");
        LineRenderer lineRenderer = circle.AddComponent<LineRenderer>();

        int segments = 32;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = segments + 1;

        float deltaTheta = (2f * Mathf.PI) / segments;
        float theta = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);
            Vector3 currentPosition = new Vector3(x, 0f, z);
            lineRenderer.SetPosition(i, currentPosition);
            theta += deltaTheta;
        }

        circle.transform.position = pos;
        Object.Destroy(circle, 5f);
    }
}
