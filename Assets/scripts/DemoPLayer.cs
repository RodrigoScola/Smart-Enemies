using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class DemoPLayer : MonoBehaviour
{
    private Hive hive;

    private MeshRenderer render;

    void Start()
    {
        var hives = FindObjectsByType<Hive>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Assert.IsTrue(hives.Length == 1, $"invalid hive length, got {hives.Length}");

        var Floor = GameObject.FindWithTag("floor");

        Assert.IsNotNull(Floor, "floor is undefined");

        render = Floor.GetComponent<MeshRenderer>();

        hive = hives[0];
    }

    // Update is called once per frame
    void Update()
    {
        var enemies = hive.Enemies();

        foreach (var enemy in enemies)
        {
            var pos = enemy.transform.position;
            var direction = transform.position - pos;
            var dist = Vector3.Distance(pos, transform.position);

            Vector3 outPos = Vector3.zero;

            if (dist < 10f)
            {
                outPos += direction.normalized * (8f - dist);
            }

            outPos.y = 0;
            transform.position += outPos * Time.deltaTime;

            Visual.Line(transform.position, 10, Color.blue, 0.1f);

            outPos.y = 0;
        }

        var min = render.bounds.min;
        var max = render.bounds.max;

        Vector3 bottomLeft = new Vector3(min.x, min.y, min.z);
        Vector3 bottomRight = new Vector3(max.x, min.y, min.z);
        Vector3 topleft = new Vector3(min.x, min.y, max.z);
        Vector3 topright = new Vector3(min.x, max.y, max.z);

        var p = transform.position;

        var mindist = 20;

        float[] other =
        {
            Mathf.Abs(min.x - p.x),
            Mathf.Abs(min.z - p.z),
            Mathf.Abs(min.y - p.y),
            Mathf.Abs(max.x - p.x),
            Mathf.Abs(max.z - p.z),
            Mathf.Abs(max.y - p.y),
        };

        if (other.Any((p => p < mindist)))
        {
            Visual.Marker(transform.position, 10f);
        }
    }
}
