using UnityEngine;
using UnityEngine.UI;

public class DebugPlayerUI : MonoBehaviour
{
    [SerializeField] private Text health;
    [SerializeField] private Text overheat;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private WeaponManager weaponManager;

    private void Start()
    {
        healthComponent.OnHealthChanged += UpdateHealthText;
        health.text = healthComponent.Health + "/" + healthComponent.MaxHealth;
    }

    void UpdateHealthText(GameObject instigator, int oldHealth, int newHealth)
    {
        health.text = newHealth + "/" + healthComponent.MaxHealth;
    }
}
