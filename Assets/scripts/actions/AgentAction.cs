using System;
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
        private readonly SortedList<float, Friend> friends = new();


        private readonly int id;
        private readonly ActionHandler parentHandler;

        private Vector3 pos;
        private readonly float radius = 10f;
        private ActionType _actionType;
        private ActionHandler[] enemies;

        public float maxDistToCalculate = 2f;

        public float maxRepelStrength = 1000f;
        public float minRepelStrength = 1f;

        public LayerMask obstacles; // Layer to check for obstacles

        private NavMeshPath path;

        private int posIndex;

        public float repelStrength = 2f;


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
            path = Hive.GetPath(parentHandler.transform.position, pos);
            Debug.Log($"total paths {path.corners.Length}");

            foreach (var corn in path.corners)
            {
                Debug.Log($"corder {corn}");

            }

            ResetContext(contextMap, size);
        }


        public void Tick()
        {

            Assert.IsNotNull(path, "should path be null?");
            if (path.corners.Length == 0)
            {
                Visual.Sphere(pos, 1);
                Assert.IsTrue(path.corners.Length > 0, $"was given an invalid path to: {pos}");


            }









            Vector3 target = path.corners[posIndex];
            var force = ComputeContextSteering(target);

            Debug.DrawLine(parentHandler.transform.position, target, Color.yellow);

            parentHandler.transform.position += force;


            var dist = Vector3.Distance(parentHandler.transform.position, target);
            // Debug.Log($"current distance {dist}");

            if (dist < 2f)
            {
                Debug.Log("going to the next distance");

                posIndex++; // Move to the next waypoint


                // Check if the path is completed
                if (posIndex >= path.corners.Length)
                {
                    Debug.Log("this is finishing");
                    parentHandler.Finish(id);
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

        private Vector3? NextPath()
        {
            posIndex++;
            return GetPath();
        }

        private Vector3? GetPath()
        {
            Assert.IsNotNull(path, "path is not initted yet");
            if (posIndex >= path.corners.Length) return null;

            var node = path.corners[posIndex];
            return node;
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
                if (f.Equals(parentHandler))
                {
                    Debug.Log("skipping same on resetting");
                    continue;
                }
                //todo: handle whenever the distance is the same...
                // right now im not going to because i want to develop this
                var dist = Vector3.Distance(f.transform.position, parentHandler.transform.position);
                Assert.IsTrue(dist > 0, $"distances should never be 0, distance: {dist} friend: {f.transform.position}, current: {parentHandler.transform.position}");

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

        private Vector3 ComputeContextSteering(Vector3 target)
        {
            resetEnemies(Hive.enemies);
            Assert.IsNotNull(parentHandler, "parent is null?");
            ResetContext(contextMap, size);

            var resultantVector = target;

            // Evaluate each direction
            for (var i = 0; i < size; i++)
            {

                int ran = 0;


                foreach (var (dist, friend) in friends)
                {
                    var actualDist = dist;
                    if (actualDist > 2f)
                    {
                        Debug.Log("skipping because of distance");
                        continue;
                    }

                    if (friend.Equals(parentHandler))
                    {
                        Debug.Log($"skippig cus same");
                        continue;
                    }

                    ran++;

                    //maybe todo? would be worth to get the collision width and height?
                    Assert.IsTrue(actualDist > 0.2f, $"friends are kissing with: {actualDist}, {friends.Count}");

                    // Increase minimum separation distance
                    actualDist = Mathf.Max(actualDist, 1.0f);

                    var repellingDirection = (parentHandler.transform.position - friend.handler.transform.position).normalized;

                    // Exponential repulsion for stronger close-range forces
                    float repellingStrength = repelStrength * Mathf.Exp(-actualDist);
                    // Add distance-based dampening
                    float dampening = 0.8f * (2.0f - Mathf.Min(actualDist, 2.0f));
                    repellingStrength *= dampening;
                    repellingStrength = Mathf.Clamp(repellingStrength, minRepelStrength, maxRepelStrength * 2.0f);



                    Debug.Log($"str: {repellingStrength}");



                    Debug.DrawRay(parentHandler.transform.position, repellingDirection * repellingStrength, Color.red);



                    resultantVector += repellingDirection * Mathf.Max(repellingStrength, 0);

                    // Visual.Marker(resultantVector, 0.5f);

                }


                if (ran > 0)
                {
                    Debug.Log($"this tick, ran: {ran} friends");
                }

            }

            foreach (var dir in contextMap) resultantVector += dir;

            // return resultantVector.normalized; // Normalize for consistent movement
            return resultantVector;
        }
    }
}