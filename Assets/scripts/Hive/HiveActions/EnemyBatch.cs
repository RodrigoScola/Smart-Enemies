using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBatch", menuName = "Scriptable Objects/EnemyBatch")]
public class EnemyBatch : ScriptableObject
{
    private int id;

    private BatchObjective _objective;

    public BatchObjective Objective()
    {
        return _objective;
    }

    public void Objective(BatchObjective obj)
    {
        _objective = obj;
    }

    public int GetId()
    {
        return id;
    }

    public void Id(int value)
    {
        Assert.IsTrue(id == 0, "cannot change an id of a batch");
        id = value;
    }

    public Dictionary<int, ActionEnemy> _enemies;

    public Dictionary<int, ActionEnemy> Enemies()
    {
        return _enemies;
    }

    public void Enemies(Dictionary<int, ActionEnemy> newEnemies)
    {
        _enemies = newEnemies;
    }
}
