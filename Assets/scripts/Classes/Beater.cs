using UnityEngine;

public abstract class HiveClass : ScriptableObject
{
    public abstract void Setup(ActionEnemy enemy);
    public abstract void Tick();
}

[CreateAssetMenu(fileName = "Beater", menuName = "Scriptable Objects/Beater")]
public class Beater : HiveClass
{
    private ActionEnemy _parent;

    public override void Setup(ActionEnemy enemy)
    {
        _parent = enemy;
    }

    public override void Tick() { }
}
