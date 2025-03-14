using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu( fileName = "Movement", menuName = "Scriptable Objects/Movement" )]
public class Movement : ScriptableObject
{
    private static Dictionary<int, Vector3[]> contextSizes = new();

    public static void ResetContextMap( out Vector3[] contextMap, int size )
    {
        if ( contextSizes.ContainsKey( size ) )
        {
            contextSizes.TryGetValue( size, out contextMap );
            return;
        }

        // Assert.IsTrue(contextMap.Length == size, "mismatched size on resetting the context");
        float angleIncrement = 360f / size;

        Vector3[] maps = new Vector3[size];

        for ( int i = 0; i < size; i++ )
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;

            maps[i].x = Mathf.Cos( angle );
            maps[i].y = 0;
            maps[i].z = Mathf.Sin( angle );
        }
        contextSizes.Add( size, maps );
        contextMap = maps;
    }

    public static Vector3[] MakeContextMap( int size )
    {
        return new Vector3[size];
    }

    public static Vector3 To( Vector3 current, Vector3 target, Vector3[] contextMap )
    {
        Vector3 direction = (target - current).normalized;
        Vector3 closestDir = Vector3.forward;
        float closestDot = Vector3.Dot( closestDir, direction );

        // Evaluate each direction
        for ( int i = 0; i < contextMap.Length; i++ )
        {
            Vector3 contextDir = contextMap[i];
            float dot = Vector3.Dot( contextDir, direction );

            if ( dot > closestDot )
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
        float actualDist = dist;

        Assert.IsTrue( actualDist > 0.2f, $"friends are kissing: {actualDist} " );

        actualDist = Mathf.Max( actualDist, 1.0f );

        Vector3 repellingDirection = (start - end).normalized;

        float repellingStrength = repelStrength * Mathf.Exp( -actualDist );
        float dampening = 0.8f * (2.0f - Mathf.Min( actualDist, 2.0f ));
        repellingStrength *= dampening;
        repellingStrength = Mathf.Clamp( repellingStrength, minRepelStrength, maxRepelStrength );

        // Debug.DrawRay(start, repellingDirection * repellingStrength, Color.red);

        return repellingDirection.normalized * Mathf.Max( repellingStrength, 0 );
    }
}
