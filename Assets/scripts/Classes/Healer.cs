using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "Healer", menuName = "Scriptable Objects/Healer")]
public class Healer : HiveClass
{
    private ActionEnemy _parent;

    public override void Setup(ActionEnemy enemy)
    {
        _parent = enemy;
    }

    public override void Tick()
    {
        var enemies = _parent.GetBatch().enemies;
        Assert.IsTrue(enemies.Count > 0, "there are no enemies in batch?");

        foreach (var en in enemies.Values)
        {
            if (en.GetId() == _parent.GetId())
                continue;

            // foreach (var ac in en.actions.Actions().Values)
            // {
            //     en.actions.Remove(ac);
            // }
        }
    }
}
