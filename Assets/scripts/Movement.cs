        using System.Collections;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.AI.Navigation;
using UnityEngine;


public class Mov : MonoBehaviour {
    public int directions = 8; // Higher resolution for finer gradient
    public float radius = 20f; // Radius for evaluating context
    public GameObject target; // Target to navigate toward
    public LayerMask obstacles; // Layer to check for obstacles

    public Transform currentTarget;


    private Vector3[] contextMap; // Array to store direction weights


    public NavMeshLink[] links;


    void Start() {
        // Initialize the context map
        contextMap = new Vector3[directions];


        links = FindObjectsByType<NavMeshLink>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var link = FindClosestLink(transform.position);
        currentTarget = target.transform;

        Debug.Log($"{link} THE NLI");
        Assert.IsNotNull(link,"there is no link");

            currentTarget = link.transform;

        Debug.Log($"links {links.Length}");
    }

    void FixedUpdate() {
        
        for (int i = 0; i < directions; i++) {
            contextMap[i] = Vector3.zero;
        }
        Vector3 bestDirection = ComputeContextSteering();

        transform.position += bestDirection * (Time.deltaTime * 5f); // Adjust speed as needed
    }
    

    [CanBeNull]
    public NavMeshLink FindClosestLink(Vector3 position) {
        Assert.IsTrue(links.Length > 0, "no link");

        var closest = links[0];
        var closestDist = Vector3.Distance(closest.startPoint, position);


        foreach (var link in links) {
            var d = Vector3.Distance(link.startPoint, position);
            if (d < closestDist) {
                closestDist = d;
                closest = link;
            }
        }

        return closest;
    }

    Vector3 ComputeContextSteering() {

        // float angleIncrement = 360f / directions;
        Vector3 resultantVector = Vector3.zero;
        
        if(!currentTarget) {
            NavMeshLink link = FindClosestLink(transform.position);
            Assert.IsNotNull(link, "no link found");

            TraverseLink(link);



        }
        
        
        //
        // for (int i = 0; i < directions; i++) {
        //     float angle = i * angleIncrement * Mathf.Deg2Rad;
        //     Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        //
        //     Ray ray = new Ray(transform.position, direction);
        //     bool hit = Physics.Raycast(ray, radius, obstacles);
        //     if (hit) {
        //         contextMap[i] = Vector3.zero; // Penalize this direction
        //         continue;
        //     }
        //
        //     float Dist = Vector3.Distance((currentTarget.transform.position - transform.position), direction);
        //     float weight = Vector3.Dot((currentTarget.transform.position - transform.position).normalized, direction);
        //
        //     Assert.NotNull(currentTarget, "there is no current Target?");
        //     NavMeshLink link = FindClosestLink(transform.position);
        //
        //     if (link) {
        //         Assert.NotNull(link, "there is no link");
        //
        //         if (Vector3.Distance(link.transform.position, currentTarget.position) < Dist) {
        //             currentTarget = link.transform;
        //         }
        //
        //         if (Vector3.Distance(link.transform.position, transform.position) <= 1.5) {
        //             TraverseLink(link);
        //         }
        //     }
        //
        //     weight = Mathf.Max(0, weight); // Ensure no negative weights
        //     contextMap[i] = direction * weight;
        // }
        //
        // // Compute resultant vector as the weighted sum of all directions
        foreach (Vector3 dir in contextMap) {
            resultantVector += dir;
        }
        //
        // Debug.DrawRay(transform.position, resultantVector * 10, Color.blue);
        //
        return resultantVector.normalized; // Normalize to ensure consistent movement
    }


    public float traversalSpeed = 2.0f;

    void TraverseLink(NavMeshLink link) {
        StartCoroutine(Traverse(link.startPoint, link.endPoint));
    }

    private IEnumerator Traverse(Vector3 start, Vector3 end) {
        float journey = 0.0f;
        

        while (journey < 1.0f) {
            journey += Time.deltaTime * traversalSpeed;
            transform.position = Vector3.Lerp(start, end, journey);
            yield return null;
        }
    }
}