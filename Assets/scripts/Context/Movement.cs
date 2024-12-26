using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "Movement", menuName = "Scriptable Objects/Movement")]
public class Movement : ScriptableObject
{
    public static void ResetContextMap(Vector3[] contextMap, int size)
    {
        Assert.IsTrue(contextMap.Length == size, "mismatched size on resetting the context");
        var angleIncrement = 360f / size;

        for (var i = 0; i < size; i++)
        {
            var angle = i * angleIncrement * Mathf.Deg2Rad;

            contextMap[i].x = Mathf.Cos(angle);
            contextMap[i].y = 0;
            contextMap[i].z = Mathf.Sin(angle);
        }
    }

    public static Vector3[] MakeContextMap(int size)
    {
        return new Vector3[size];
    }

    public static Vector3 To(Vector3 current, Vector3 target, Vector3[] contextMap)
    {
        Vector3 direction = (target - current).normalized;
        Vector3 closestDir = Vector3.forward;
        var closestDot = Vector3.Dot(closestDir, direction);

        // Evaluate each direction
        for (var i = 0; i < contextMap.Length; i++)
        {
            Vector3 contextDir = contextMap[i];
            var dot = Vector3.Dot(contextDir, direction);

            if (dot > closestDot)
            {
                closestDot = dot;
                closestDir = contextDir;
            }
        }
        // Debug.DrawRay(current, closestDir * 10, Color.yellow);

        return closestDir;
    }

    public static Vector3 Repel(
        float dist,
        Vector3 start,
        Vector3 end,
        float repelStrength,
        float minRepelStrength,
        float maxRepelStrength
    )
    {
        var actualDist = dist;

        Assert.IsTrue(actualDist > 0.2f, $"friends are kissing: {actualDist} ");

        actualDist = Mathf.Max(actualDist, 1.0f);

        var repellingDirection = (start - end).normalized;

        float repellingStrength = repelStrength * Mathf.Exp(-actualDist);
        float dampening = 0.8f * (2.0f - Mathf.Min(actualDist, 2.0f));
        repellingStrength *= dampening;
        repellingStrength = Mathf.Clamp(repellingStrength, minRepelStrength, maxRepelStrength);

        // Debug.DrawRay(start, repellingDirection * repellingStrength, Color.red);

        return repellingDirection * Mathf.Max(repellingStrength, 0);
    }
}
