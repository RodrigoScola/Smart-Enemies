using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace actions
{
    public class Hive : MonoBehaviour
    {
        public static ActionHandler[] enemies;


        private static int ids;

        public LayerMask obstacles;

        private void Start()
        {
            Hive.enemies =
                FindObjectsByType<ActionHandler>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            Assert.IsTrue(Hive.enemies.Length > 0, "you forgot to init enemies");
            Debug.Log($"hive initialized, enemies: {Hive.enemies.Length}");
            foreach (var enemy in Hive.enemies) FourDirections(enemy, obstacles);
        }



        public static int GetId()
        {
            return ++ids;
        }


        private static void FourDirections(ActionHandler handler, LayerMask obstacles)
        {
            handler.Add(new AgentAction(
                GetId(),
                Priority.Low,
                Vector3.forward * 10,
                obstacles,
                handler
            ));
            handler.Add(new AgentAction(
                GetId(),
                Priority.High,
                Vector3.forward * 20,
                obstacles,
                handler
            ));

            handler.Add(new AgentAction(
                GetId(),
                Priority.High,
                Vector3.back * 40,
                obstacles,
                handler
            ));
            handler.Add(
                new DebugAction(
                    GetId(),
                    handler,
                    Priority.High,
                    Color.blue
                )
            );
            handler.Add(new AgentAction(
                GetId(),
                Priority.High,
                Vector3.left * 10,
                obstacles,
                handler
            ));

            handler.Add(new AgentAction(
                GetId(),
                Priority.High,
                Vector3.right * 10,
                obstacles,
                handler
            ));

            handler.Add(new AgentAction(
                GetId(),
                Priority.High,
                Vector3.zero,
                obstacles,
                handler
            ));

            handler.Add(
                new DebugAction(
                    GetId(),
                    handler,
                    Priority.Medium,
                    Color.green
                )
            );
            Assert.IsTrue(handler.actions.Count >  0 ,"no actions were actually initialized");
        }
    }
}