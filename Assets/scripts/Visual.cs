using UnityEngine;

public class Visual : MonoBehaviour
{
    public static void Marker(Vector3 pos) => DrawMarker(pos, 1f, Color.blue);

    public static void Marker(Vector3 pos, float radius) => DrawMarker(pos, radius, Color.blue);

    public static void Marker(Vector3 pos, Color color) => DrawMarker(pos, 1f, color);

    private static void DrawMarker(Vector3 pos, float radius, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.transform.position = pos;

        Renderer rend = marker.GetComponent<Renderer>();

        rend.material.color = color;

        marker.transform.localScale = new Vector3(radius, 100 / 2, radius);

        Destroy(marker, 5f);
    }

    private static void DrawSphere(Vector3 pos, Color col, float time)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;

        Renderer rend = sphere.GetComponent<Renderer>();

        rend.material.color = col;

        Destroy(sphere, time);
    }

    public static void Sphere(Vector3 pos) => DrawSphere(pos, Color.blue, 5f);

    public static void Sphere(Vector3 pos, Color color, float time) => DrawSphere(pos, color, time);

    public static void Sphere(Vector3 pos, float time) => DrawSphere(pos, Color.blue, time);

    public static void Sphere(Vector3 pos, Color col) => DrawSphere(pos, col, 5f);

    public static void Line(Vector3 start, Vector3 end, Color color, float time) => DrawLine(start, end, color, time);

    public static void Line(Vector3 start, int size, Color color, float time) =>
        DrawLine(start, start * size, color, time);

    private static void DrawLine(Vector3 start, Vector3 end, Color color, float time)
    {
        GameObject obj = new("Debug Line");

        LineRenderer line = obj.AddComponent<LineRenderer>();

        line.startColor = color;
        line.endColor = color;

        line.SetPosition(0, start);
        line.SetPosition(1, end);
        Destroy(obj, time);
    }

    public static void Circle(Vector3 pos) => DrawCircle(pos, 1f, Color.blue, 5f);

    public static void Circle(Vector3 pos, float radius) => DrawCircle(pos, radius, Color.blue, 5f);

    public static void Circle(Vector3 pos, float radius, Color color) => DrawCircle(pos, radius, color, 5f);

    public static void Circle(Vector3 pos, float radius, float time) => DrawCircle(pos, radius, Color.blue, time);

    public static void Circle(Vector3 pos, float radius, Color color, float time) =>
        DrawCircle(pos, radius, color, time);

    private static void DrawCircle(Vector3 pos, float radius, Color color, float time)
    {
        GameObject circle = new("DebugCircle");
        LineRenderer lineRenderer = circle.AddComponent<LineRenderer>();

        int segments = 32;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = color;
        lineRenderer.positionCount = segments + 1;

        float deltaTheta = 2f * Mathf.PI / segments;
        float theta = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);
            Vector3 currentPosition = new(x, 0f, z);
            lineRenderer.SetPosition(i, currentPosition);
            theta += deltaTheta;
        }

        circle.transform.position = pos;
        Destroy(circle, time);
    }
}
