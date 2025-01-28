using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class DemoPLayer : MonoBehaviour
{
    private Hive hive;

    [SerializeField]
    [UnityEngine.Range(0, 1000)]
    private float bufferDistance;

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

        Vector3 outpos = Vector3.zero;

        foreach (var enemy in enemies)
        {
            var pos = enemy.transform.position;
            var direction = transform.position - pos;
            var dist = Vector3.Distance(pos, transform.position);

            if (dist < 10f)
            {
                outpos += direction.normalized * (8f - dist);
            }
        }

        outpos *= Time.deltaTime;

        var min = render.bounds.min;
        var max = render.bounds.max;

        var top = new Vector3(min.x, 0, min.z);
        var left = new Vector3(min.x, 0, min.z);
        var right = new Vector3(max.x, 0, max.z);
        var bottom = new Vector3(max.x, 0, max.z);

        if (
            outpos.x + transform.position.x > bottom.x - bufferDistance
            || outpos.x + transform.position.x < top.x + bufferDistance
        )
        {
            outpos.x *= -1;
        }

        if (
            outpos.z + transform.position.z < left.z + bufferDistance
            || outpos.z + transform.position.z > right.z - bufferDistance
        )
        {
            outpos.z *= -1;
        }

        outpos.y = 0;
        transform.position += outpos;
        transform.rotation = Quaternion.LookRotation(transform.position);
    }

    public Vector3 Right(float dist)
    {
        Assert.IsTrue(dist > 0, $"distance cannot be less than 0, got: ({dist})");
        return transform.position + dist * Vector3.right;
    }

    public Vector3 Left(float dist)
    {
        Assert.IsTrue(dist > 0, $"distance cannot be less than 0, got: ({dist})");
        return transform.position + (Vector3.left * dist);
    }
}
