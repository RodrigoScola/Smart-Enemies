using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace actions
{
    public class Hive : MonoBehaviour
    {
        public static ActionHandler[] enemies;


        private static int ids;

        public static LayerMask obstacles;

        public List<GameObject> gamePoints;
        public static List<GameObject> points = new();

        private void Start()
        {
            Hive.enemies =
                FindObjectsByType<ActionHandler>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            Assert.IsTrue(Hive.enemies.Length > 0, "you forgot to init enemies");
            Debug.Log($"hive initialized, enemies: {Hive.enemies.Length}");



            foreach (var enemy in Hive.enemies) Hive.FourDirections(enemy, obstacles, gamePoints);



        }



        public static int GetId()
        {
            return ++ids;
        }

        public static NavMeshPath GetPath(Vector3 start, Vector3 end)
        {

            NavMeshPath path = new();
            NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);

            return path;
        }


        private static void FourDirections(ActionHandler handler, LayerMask obstacles, List<GameObject> p)
        {

            Assert.IsTrue(p.Count > 0, "there are no points to be initted");


            foreach (var point in p)
            {
                var id = GetId();
                Debug.Log($"initting an action with id of {id}");
                handler.Add(new AgentAction(
                    GetId(),
                    Priority.High,
                    point.transform.position,
                    obstacles,
                    handler

                ));

            }


            handler.Add(
                new DebugAction(
                    GetId(),
                    handler,
                    Priority.Medium,
                    Color.green
                )
            );
            Assert.IsTrue(handler.actions.Count > 0, "no actions were actually initialized");
        }
    }
}