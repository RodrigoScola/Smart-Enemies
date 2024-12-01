using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PLayer : MonoBehaviour
{
    public List<Vector3> positions;

    public Camera cam;
    public NavMeshAgent agent;
    public bool canWalk = false;
    public Vector3 currentPosition;


    void Update()
    {
        
        if (positions.Count == 0)
        {
            agent.Move(Vector3.zero);
                
        }

        
        if (!canWalk ||!(agent.remainingDistance <= 1) || positions.Count <= 0) return;
        agent.SetDestination(positions[0]);
        currentPosition = positions[0];
        positions.RemoveAt(0);
        Debug.Log($"left: {positions.Count}" );

    }
}