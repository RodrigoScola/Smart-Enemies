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
        outpos.y = 0;

        Visual.Line(transform.position + outpos, transform.position, Color.blue, 0.1f);

        transform.position += outpos * Time.deltaTime;
    }
}
