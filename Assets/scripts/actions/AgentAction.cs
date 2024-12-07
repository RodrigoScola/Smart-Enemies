using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace actions
{
    internal struct Friend
    {
        public float dist;
        public ActionHandler handler;
    }


    public class AgentAction : Action
    {
        private readonly Priority _priority;
        private readonly NavMeshAgent agent;

        private readonly Vector3[] contextMap; // Array to store direction weights


        private readonly int id;
        private readonly ActionHandler parentHandler;

        public float maxRepelStrength = 1000f;

        public float maxDistToCalculate = 2f;
        
        public float repelStrength = 25f;
        public float minRepelStrength = 0f;

        private readonly Vector3 pos;
        private readonly float radius = 10f;
        private ActionType _actionType;
        private ActionHandler[] enemies;
        private readonly SortedList<float, Friend> friends = new SortedList<float, Friend>();

        public LayerMask obstacles; // Layer to check for obstacles


        public int size = 32; // Higher resolution for finer gradient

        private ActionState state;


        public AgentAction(int id, Priority priority, Vector3 destination, LayerMask _obstacles,
            ActionHandler handler
        )
        {
            this.id = id;
            _priority = priority;
            agent = handler.GetComponent<NavMeshAgent>();
            pos = destination;
            obstacles = _obstacles;

            parentHandler = handler;

            contextMap = new Vector3[size];

            state = ActionState.Waiting;

            ResetContext(contextMap, size);
        }


        public void Tick()
        {
            Assert.IsTrue(state == ActionState.Running,
                $"cannot be running with any other action other than running, received: {state}");
            Assert.IsNotNull(parentHandler, "Parent Is null??");
            Assert.IsNotNull(agent, "agent is null");

            Debug.Log("TICKING");


            var bestDirection = ComputeContextSteering();

            Debug.Log($"best direction {bestDirection}");

            agent.transform.position += bestDirection * (Time.deltaTime * 5f); // Adjust speed as needed


            var dist = Vector3.Distance(agent.transform.position, pos);


            //todo: this is trash make it in the done function
            if (dist < 3)
            {
                Debug.Log($"finishing moving action to {pos}");
                parentHandler.Finish(id);
            }
        }

        public ActionType GetActionType()
        {
            return _actionType;
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

        private static void ResetContext(Vector3[] contextMap, int size)
        {
            var angleIncrement = 360f / size;

            for (var i = 0; i < size; i++)
            {
                var angle = i * angleIncrement * Mathf.Deg2Rad;
                contextMap[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            }
        }

        private void resetEnemies(ActionHandler[] current)
        {
            friends.Clear();

            foreach (var f in current)
            {
                //todo: handle whenever the distance is the same... 
                // right now im not going to because i want to develop this
                var dist = Vector3.Distance(f.transform.position, parentHandler.transform.position);
                friends.Add(
                    dist,
                    new Friend
                    {
                        handler = f,
                        dist = dist
                    }
                );
            }
        }


        private Vector3 ComputeContextSteering()
        {
            resetEnemies(Hive.enemies);
            Assert.IsNotNull(parentHandler, "parent is null?");
            ResetContext(contextMap, size);

            var resultantVector = Vector3.zero;

            // Evaluate each direction
            for (var i = 0; i < size; i++)
            {
                var direction = contextMap[i];
                var ray = new Ray(parentHandler.transform.position, direction);

                if (Physics.Raycast(ray, radius, obstacles))
                {
                    contextMap[i] = Vector3.zero; // Penalize this direction
                }
                else
                {
                    foreach (var (dist, friend) in friends)
                    {
                        if (dist > maxDistToCalculate) continue;

                        var repellingDirection = (parentHandler.transform.position - friend.handler.transform.position).normalized;
                        var repellingStrength = Mathf.Clamp(repelStrength / (dist * dist), minRepelStrength, repelStrength);

                        if (dist < 2.0f) // Minimum separation
                        {
                            repellingStrength = maxRepelStrength;
                        }

                        resultantVector += repellingDirection * repellingStrength;

                        Debug.DrawLine(parentHandler.transform.position, 
                            parentHandler.transform.position + repellingDirection * repellingStrength, 
                            Color.red);
                    }

                    var weight = Vector3.Dot((pos - parentHandler.transform.position).normalized, direction);
                    weight = Mathf.Max(0, weight);
                    contextMap[i] = direction * weight;
                }
            }

            foreach (var dir in contextMap) resultantVector += dir;

            return resultantVector.normalized; // Normalize for consistent movement
        }
    }
}