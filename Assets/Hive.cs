using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hive : MonoBehaviour
{
    public int enemies;
    public int rad;
    public Enemy[] enemy;

    private static PLayer _player;

    public List<Transform> targets;

    public Collider[] obstacles;
    
    public static Vector3 GetPlayerPosition()
    {
        if (!_player)
        {
            _player = FindFirstObjectByType<PLayer>();
        }
        return _player.transform.position;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var t in enemy)
        {
            for(int j = 0; j < enemies; j++)
            {
                
                float angle = Random.Range(0f, Mathf.PI * 2f);

                // Generate a random radius with uniform distribution over the area
                float r = Mathf.Sqrt(Random.Range(0f, 1f)) * rad;

                // Convert polar coordinates to Cartesian coordinates
                float x = r * Mathf.Cos(angle);
                float y = r * Mathf.Sin(angle);

                var obj = Instantiate(t, gameObject.transform).gameObject;

                obj.transform.position = new Vector3(
                    obj.transform.position.x + x,
                    obj.transform.position.y + y,
                    obj.transform.position.z
                );

            }
        }
    }

}
