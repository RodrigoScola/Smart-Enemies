using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent _agent;
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        Assert.IsNotNull(_agent,"need to have an agent on enemy");

    }
    
    static Vector3 RandomPointInPlayer()
    {
        Vector3 pos = Hive.GetPlayerPosition();
        
        // Generate a random angle in radians
        float angle = Random.Range(0f, Mathf.PI * 2f);

        // Generate a random radius with uniform distribution over the area
        float r = Mathf.Sqrt(Random.Range(0f, 1f)) * 6;

        // Convert polar coordinates to Cartesian coordinates
        float x = r * Mathf.Cos(angle);
        float y = r * Mathf.Sin(angle);
        
        
        return new Vector3( pos.x + x, pos.y + y, pos.z );
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if( !_agent.hasPath)
        {
            _agent.SetDestination(RandomPointInPlayer());
        }
        
    } 
    
    
}
