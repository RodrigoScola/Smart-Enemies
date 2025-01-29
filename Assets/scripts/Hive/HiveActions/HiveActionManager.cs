using System;
using System.Collections.Generic;
using System.Linq;
using actions;
using NUnit.Framework;
using SmartEnemies;
using UnityEngine;

namespace SmartEnemies
{
    public struct BatchEnemies
    {
        private int id;

        public readonly int GetId()
        {
            return id;
        }

        public void SetId( int value )
        {
            Assert.IsTrue( id == 0, "cannot change an id of a batch" );
            id = value;
        }

        public Dictionary<int, ActionEnemy> Enemies;
    }
}

[Serializable]
public class HiveActionManager
{
    [SerializeField]
    private int ticks = 0;

    public int maxPerBatch = 45;

    private readonly Dictionary<int, BatchEnemies> batches = new();

    private readonly Dictionary<int, int> enemyBatches = new();

    private bool ran = false;

    public List<BatchEnemies> Batches()
    {
        Assert.IsTrue( batches.Count > 0, "not batched yet" );
        //do i want this
        return batches.Values.ToList();
    }

    //todo change this to a dictionary datastructure. this will prob do  for now
    public BatchEnemies? GetEnemyBatch( int enemyId )
    {
        Assert.IsTrue( batches.Count > 0, "no batches yet. do i want this" );

        enemyBatches.TryGetValue( enemyId, out int batchId );

        if ( batchId == 0 )
        {
            return null;
        }
        Assert.IsTrue( batches.ContainsKey( batchId ), $"enemy thinks batch {batchId} exists" );

        batches.TryGetValue( batchId, out BatchEnemies batch );
        Assert.NotNull( batch, "batch does not exist" );
        Assert.IsTrue( batch.Enemies.ContainsKey( enemyId ), "batch does not contain enemy" );

        return batch;
    }

    private BatchEnemies NewBatch()
    {
        BatchEnemies currentBatch = new() { Enemies = new() };
        currentBatch.SetId( Hive.GetId() );
        return currentBatch;
    }

    private List<ActionEnemy> SortByDistance( Vector3 basePoint, List<ActionEnemy> enemies )
    {
        Dictionary<float, ActionEnemy> distances = new();

        foreach ( ActionEnemy item in enemies )
        {
            float dist = Vector3.Distance( basePoint, item.transform.position );
            Assert.IsFalse( distances.ContainsKey( dist ), "same distance on batching enemies?" );
            distances.Add( dist, item );
        }
        var result = distances.OrderBy( d => d.Key ).Select( f => f.Value ).ToList();
        Assert.IsTrue(
            result.Count == enemies.Count,
            $"adding or removing more than expected, wanted {enemies.Count}, got {result.Count}"
        );
        return result;
    }

    public void Batch( ActionEnemy[] enemies, Dictionary<int, BatchEnemies> currentBatches )
    {
        Assert.IsTrue( enemies.Length > 0, "batching empty amount of enemies" );
        Assert.GreaterOrEqual( maxPerBatch, 1, "cannot have less than 1 per batch" );

        currentBatches.Clear();
        enemyBatches.Clear();

        BatchEnemies currentBatch = NewBatch();

        List<ActionEnemy> sorted = SortByDistance( enemies[0].transform.position, enemies.ToList() );

        Assert.IsTrue(
            enemies.Length == sorted.Count,
            $"sorted enemies not equal, expected:{enemies.Length} got {sorted.Count}"
        );

        Assert.IsTrue( enemyBatches.Count == 0, "didnt clear the enemy batches good enougth" );
        Assert.IsTrue( currentBatches.Count == 0, "didnt clear the batches good enougth" );

        while ( sorted.Count > 0 )
        {
            ActionEnemy en = sorted[0];
            sorted = SortByDistance( en.transform.position, sorted );

            currentBatch.Enemies.Add( en.GetId(), en );

            enemyBatches.TryGetValue( en.GetId(), out int id );

            Assert.IsFalse(
                enemyBatches.ContainsKey( en.GetId() ),
                $"duplicate enemy ({en.GetId()}) on batching  index: {id}"
            );

            enemyBatches.Add( en.GetId(), currentBatch.GetId() );
            sorted.RemoveAt( 0 );

            if ( currentBatch.Enemies.Count == maxPerBatch )
            {
                batches.Add( currentBatch.GetId(), currentBatch );
                currentBatch = NewBatch();
            }
        }

        batches.Add( currentBatch.GetId(), currentBatch );
    }

    public void Rebatch()
    {
        Batch( Hive.enemies, batches );
        Assert.IsTrue( batches.Count > 0, $"no batches came out, even with {Hive.enemies.Length}" );

        int batchIdx = 0;
        foreach ( BatchEnemies batch in batches.Values )
        {
            Dictionary<int, ActionEnemy> enemies = batch.Enemies;
            Assert.IsTrue(
                enemies.Count <= maxPerBatch,
                $"there should be max {maxPerBatch} per batch, received: {enemies.Count}"
            );

            float hue = (float)batchIdx / batches.Count;
            var bcolor = Color.HSVToRGB( hue, 1f, 1f );

            foreach ( ActionEnemy en in enemies.Values )
            {
                en.actions.Add( new DebugAction( Hive.GetId(), en, Priority.High, bcolor ) );
            }
            batchIdx++;
        }
    }

    public void Tick()
    {
        //should prob replace this with a better alternative, maybe coroutines
        ticks++;

        // if (ticks % 100 == 0)
        // {
        //     Rebatch();
        // }

        DemoPLayer player = Hive.players[0].GetComponent<DemoPLayer>();
        Assert.IsNotNull( player, "player does not have demo player" );

        if ( ticks % 100 == 0 && !ran )
        {
            ran = true;

            foreach ( BatchEnemies batch in Batches() )
            {
                foreach ( ActionEnemy en in batch.Enemies.Values )
                {
                    try
                    {
                        Action running = en.actions.RunningActions().First( ( ac ) => ac.GetActionType() == ActionType.Move );

                        en.actions.Remove( running );
                    }
                    catch ( Exception ) { }

                    en.actions.Add( new ScanAction( Hive.GetId(), en, Priority.Medium ) );

                    float dist = Vector3.Distance( en.transform.position, player.transform.position );

                    if ( dist < en.MinDistance() )
                    {
                        // Visual.Circle(player.transform.position, en.MinDistance());
                        // var path = Hive.GetPath(en.transform.position, Vector3.zero);

                        // foreach (var p in path.corners)
                        // {
                        //     Visual.Sphere(p);
                        // }

                        // en.MinDistance(2);

                        // en.actions.Add(
                        //     new MoveAction(
                        //         Hive.GetId(),
                        //         en,
                        //         Priority.High,
                        //         Hive.GetPath(en.transform.position, Vector3.zero),
                        //         MoveTargetType.Position
                        //     )
                        // );
                        Vector3 direction = en.transform.position - player.transform.position;
                        direction.Normalize();

                        UnityEngine.AI.NavMeshPath path = Hive.GetPath(
                            en.transform.position,
                            player.transform.position + (direction * (en.MinDistance() + 1f))
                        );

                        Vector3[] paths = new[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
                        int ind = 1;
                        do
                        {
                            foreach ( Vector3 pos in paths )
                            {
                                UnityEngine.AI.NavMeshPath left = Hive.GetPath(
                                    en.transform.position,
                                    player.transform.position + (pos * ind) + (direction * (en.MinDistance() + 1f))
                                );

                                if ( left.corners.Length > 0 )
                                {
                                    path = left;
                                    break;
                                }
                            }

                            ind++;
                        } while ( path.corners.Length == 0 );

                        foreach ( Vector3 point in path.corners )
                        {
                            Visual.Sphere( point );
                        }

                        en.MinDistance( 2 );

                        en.actions.Add(
                            new FollowAction( Hive.GetId(), en, Priority.High, path, MoveTargetType.Position )
                        );
                    }

                    en.actions.Add(
                        new FollowAction(
                            Hive.GetId(),
                            en,
                            Priority.High,
                            () =>
                            {
                                return player.Left( 2 );
                            },
                            MoveTargetType.Player
                        )
                    );
                }
            }
        }

        foreach ( BatchEnemies batch in batches.Values )
        {
            foreach ( ActionEnemy en in batch.Enemies.Values )
            {
                en.Tick();
            }
        }
        Assert.IsNotNull( Hive.enemies, "you didnt initialize enemies? how did it get past all the errors" );

        Assert.IsTrue( Hive.enemies.Length > 0, "are you sure you dont want any enemies right now?" );
    }
}
