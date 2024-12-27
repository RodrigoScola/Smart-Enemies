using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "HealthSystem", menuName = "Scriptable Objects/HealthSystem")]
public class HealthSystem : ScriptableObject
{
    private GameObject healthBarObject;
    private Image healthBarFill;
    private ActionEnemy _self;
    public bool barCreated = false;

    public float baseHealth = 100;
    public float maxHealth = 100;

    public float currentHealth;

    private Transform mainCamera;

    public void Restore(float amount)
    {
        Assert.GreaterOrEqual(currentHealth, 0, "this guy should be dead");
        currentHealth += amount;
    }

    public void Damage(float amount)
    {
        Assert.GreaterOrEqual(currentHealth, 0, "this guy should be dead");
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Assert.IsNotNull(_self, "trying to kill yourself huh pissboy? well you cant kill yourself");
        Destroy(_self);
    }

    public void SetParent(ActionEnemy owner)
    {
        _self = owner;
    }

    public void Setup()
    {
        mainCamera = Camera.main.transform;
        CreateHealthBar();
    }

    public void CreateHealthBar() { }

    public void Update() { }

    public void Display() { }
}
