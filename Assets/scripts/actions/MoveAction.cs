using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace actions
{
    public class MoveAction : Action
    {
        private readonly Priority _priority;
        private readonly NavMeshAgent agent;

        private readonly Vector3[] contextMap; // Array to store direction weights

        private readonly int id;
        private readonly ActionEnemy parentHandler;

        public float maxRepelStrength = 1000f;
        public float minRepelStrength = 1f;

        private NavMeshPath path;

        private int posIndex;

        public float repelStrength = 2f;

        public int size = 32; // Higher resolution for finer gradient

        private ActionState state;

        public MoveAction(int id, ActionEnemy handler, Priority priority, NavMeshPath pathing)
        {
            this.id = id;
            _priority = priority;
            agent = handler.GetComponent<NavMeshAgent>();

            parentHandler = handler;

            path = pathing;
            Assert.IsTrue(pathing.corners.Length > 0, "was given an invalid path in the initial parts");

            state = ActionState.Waiting;
            contextMap = Movement.MakeContextMap(size);
            Movement.ResetContextMap(contextMap, size);
        }

        public void Tick()
        {
            Assert.IsNotNull(path, "trying to move to undefined path");
            if (posIndex < path.corners.Length)
            {
                var newPos = path.corners[posIndex];
                // agent.SetDestination(newPos);
            }

            Assert.IsNotNull(path, "should path be null?");
            Assert.IsTrue(path.corners.Length > 0, "was given an invalid path");

            Vector3 target = path.corners[posIndex];
            // var force = ComputeContextSteering(target);

            // Debug.DrawLine(parentHandler.transform.position, target, Color.yellow);
            // Debug.DrawLine(parentHandler.transform.position, agent.nextPosition, Color.yellow);

            // var force = ComputeContextSteering(target);

            //todo reduce the force lol
            // if (parentHandler.GetId() == 1044)
            // {
            //     parentHandler.transform.position += force / 40;
            // }
            // else
            // {
            //     parentHandler.transform.position += force / 200;
            // }

            var dist = Vector3.Distance(parentHandler.transform.position, target);

            if (dist < agent.stoppingDistance)
            {
                if (posIndex >= path.corners.Length)
                {
                    Visual.Marker(target, 1f);
                }
                Assert.IsTrue(
                    posIndex < path.corners.Length,
                    $"the distance to the target is completed but pos index is not correct, expected:{path.corners.Length}, got: {posIndex}"
                );

                // Debug.Log($"going to the next distance: {posIndex}, total: {path.corners.Length}");

                posIndex++; // Move to the next waypoint

                // Check if the path is completed
                if (posIndex >= path.corners.Length)
                {
                    Finish();
                }
                else
                {
                    var newPos = path.corners[posIndex];
                    agent.SetDestination(newPos);
                }
            }
        }

        public ActionType GetActionType()
        {
            return ActionType.Move;
        }

        public Priority GetPriority()
        {
            return _priority;
        }

        public ActionState GetState()
        {
            return state;
        }

        public void SetState(ActionState newState)
        {
            state = newState;
        }

        public int GetId()
        {
            return id;
        }

        private Vector3 ComputeContextSteering(Vector3 target)
        {
            Assert.IsNotNull(parentHandler, "parent is null?");
            Movement.ResetContextMap(contextMap, size);
            Vector3 parentPos = parentHandler.transform.position;
            int parentId = parentHandler.GetId();

            Vector3 closestDir = Movement.To(parentPos, target, contextMap);

            foreach (var friend in Hive.enemies)
            {
                var friendPos = friend.transform.position;

                var dist = Vector3.Distance(parentPos, friendPos);
                if (dist > 2f)
                {
                    // Debug.Log($"skipping because of distance {actualDist}");
                    break;
                }

                if (friend.Equals(parentHandler))
                {
                    // Debug.Log($"skippig cus same");
                    continue;
                }
                closestDir += Movement.Repel(
                    dist,
                    parentPos,
                    friendPos,
                    repelStrength,
                    minRepelStrength,
                    maxRepelStrength
                );
            }

            var dir = parentPos - target;

            if (Physics.Raycast(parentPos, dir * 7f, out var hit, 7f, Hive.EnemyMask))
            {
                closestDir += Vector3.right * 10000;
            }

            return closestDir;
        }

        public void Finish()
        {
            agent.ResetPath();
            parentHandler.actions.Finish(id);
        }
    }
}
