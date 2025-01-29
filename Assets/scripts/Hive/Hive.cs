using System;
using System.Collections.Generic;
using System.Linq;
using actions;
using SmartEnemies;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class Hive : MonoBehaviour
{
    public static ActionEnemy[] enemies;

    private static int ids;

    [SerializeField]
    public HiveActionManager manager = new();

    public static HiveActionManager Manager = new();

    public List<GameObject> gamePoints;
    public static List<GameObject> players;

    public static LayerMask EnemyMask;

    private void Start()
    {
        Hive.EnemyMask = LayerMask.GetMask( "Enemy" );
        Hive.enemies = FindObjectsByType<ActionEnemy>( FindObjectsInactive.Exclude, FindObjectsSortMode.None )
            .ToList()
            .GroupBy( f => f.GetId() )
            .Select( f => f.First() )
            .ToArray();

        Hive.Manager = manager;

        Hive.players = GameObject.FindGameObjectsWithTag( "Player" ).ToList();

        Hive.Manager.Rebatch();

        Assert.IsTrue( Hive.enemies.Length > 0, "you forgot to init enemies" );
        Debug.Log( $"hive initialized, enemies: {Hive.enemies.Length}" );

        List<BatchEnemies> batches = Hive.Manager.Batches();

        float angleIncrement = 360f / batches.Count;

        Dictionary<int, Vector3> positions = new();

        for ( int i = 0; i < batches.Count; i++ )
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;

            var pos = new Vector3( Mathf.Cos( angle ), 0, Mathf.Sin( angle ) );
            positions.Add( batches[i].GetId(), pos * 20f );
        }

        foreach ( ActionEnemy enemy in enemies )
        {
            Follow( enemy, players[0] );
            // positions.TryGetValue(enemy.GetBatch()!.GetId(), out var pos);

            // MoveToPosition(enemy, pos);
        }
    }

    public static int GetId()
    {
        return ++ids;
    }

    public static NavMeshPath GetPath( Vector3 start, Vector3 end )
    {
        NavMeshPath path = new();
        NavMesh.CalculatePath( start, end, NavMesh.AllAreas, path );

        return path;
    }

    private void Update()
    {
        //todo: remove this once were not just testing anymore

        Hive.enemies ??= FindObjectsByType<ActionEnemy>( FindObjectsInactive.Exclude, FindObjectsSortMode.None )
            .GroupBy( f => f.GetId() )
            .Select( f => f.First() )
            .ToArray();
        List<BatchEnemies> b = Hive.Manager.Batches();
        Hive.Manager.Rebatch();
        Assert.IsTrue( b.Count > 0, "no batch found" );

        manager.Tick();
    }

    private static void MoveToPosition( ActionEnemy handler, Vector3 position )
    {
        handler.actions.Add( new DebugAction( GetId(), handler, Priority.Medium, Color.green ) );

        BatchEnemies? batch = Manager.GetEnemyBatch( handler.GetId() );

        Assert.IsTrue( batch.HasValue, "didnt batch the enemy yet" );

        NavMeshPath path = Hive.GetAlternatePath( handler.transform.position, position, batch.Value.GetId() );

        Assert.IsTrue( path.corners.Length > 0, "invalid path on batching" );

        handler.actions.Add( new MoveAction( Hive.GetId(), handler, Priority.High, path, MoveTargetType.Position ) );

        Assert.IsTrue( handler.actions.Actions().Count > 0, "no actions were actually initialized" );
    }

    public static void Follow( ActionEnemy enemy, GameObject player )
    {
        // enemy.actions.Add(
        //     new FollowAction(GetId(), enemy, Priority.High, () => player.transform.position, MoveTargetType.Player)
        // );
    }

    private static void MovePoints( ActionEnemy handler, List<GameObject> p )
    {
        Assert.IsTrue( p.Count > 0, "there are no points to be initted" );

        // handler.actions.Add(new ScanAction(GetId(), handler, Priority.Low));
        handler.actions.Add( new DebugAction( GetId(), handler, Priority.Medium, Color.green ) );

        for ( int i = 0; i < p.Count; i++ )
        {
            GameObject point = p[i];
            BatchEnemies? batch = Manager.GetEnemyBatch( handler.GetId() );

            Assert.IsTrue( batch.HasValue, "didnt batch the enemy yet" );

            NavMeshPath path = Hive.GetAlternatePath(
                handler.transform.position,
                point.transform.position,
                batch.Value.GetId()
            );

            Assert.IsTrue( path.corners.Length > 0, "invalid path on batching" );

            handler.actions.Add( new MoveAction( Hive.GetId(), handler, Priority.High, path, MoveTargetType.Position ) );
        }

        Assert.IsTrue( handler.actions.Actions().Count > 0, "no actions were actually initialized" );
    }

    public static NavMeshPath GetAlternatePath( Vector3 start, Vector3 end, int batchIndex )
    {
        NavMeshPath path = new NavMeshPath();
        Vector3 midPoint;

        int times = 0;
        do
        {
            float offset = (batchIndex % 2 == 0) ? 10f + times : -10f + times;
            midPoint = Vector3.Lerp( start, end, 0.5f ) + new Vector3( offset, 0, offset );

            NavMesh.CalculatePath( start, midPoint, NavMesh.AllAreas, path );
            NavMesh.CalculatePath( midPoint, end, NavMesh.AllAreas, path );

            if ( path.corners.Length > 0 )
            {
                return path;
            }

            NavMesh.CalculatePath( start, end, NavMesh.AllAreas, path );
            times++;
        } while ( path.corners.Length == 0 && times < 10 );
        return path;
    }

    public ActionEnemy[] Enemies()
    {
        return Hive.enemies;
    }
}
