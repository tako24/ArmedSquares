using Mirror;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] int _maxHealth = 100;
    [SyncVar] int _health;

    public delegate void DeadHandler(GameObject instigator);
    public event DeadHandler OnDead;
    public delegate void HealthChangedHandler(GameObject instigator, int oldHealth, int newHealth);
    public event HealthChangedHandler OnHealthChanged;

    private void Start()
    {
        _health = _maxHealth;
    }

    [Server]
    public void TakeDamage(GameObject instigator, int damage)
    {
        var oldHealth = _health;
        _health = Mathf.Clamp(_health - damage, 0, _maxHealth);
        OnHealthChanged?.Invoke(instigator, oldHealth, _health);

        if (_health == 0)
        {
            OnDead?.Invoke(instigator);
        }
    }
}
