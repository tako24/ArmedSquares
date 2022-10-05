using Mirror;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] int _maxHealth = 100;
    [SyncVar] int _health;

    public int MaxHealth => _maxHealth;
    public int Health => _health;

    public delegate void DeadHandler(GameObject instigator);
    public event DeadHandler OnDead;
    public delegate void HealthChangedHandler(GameObject instigator, int oldHealth, int newHealth);
    public event HealthChangedHandler OnHealthChanged;

    public void Respawn()
    {
        _health = _maxHealth;
    }

    private void Start()
    {
        Respawn();
    }

    private void Update()
    {
        Debug.Log("health " + _health + "/" + _maxHealth);
    }

    [Server]
    public void TakeDamage(GameObject instigator, int damage)
    {
        var oldHealth = _health;
        _health = Mathf.Clamp(_health - damage, 0, _maxHealth);
        OnHealthChanged?.Invoke(instigator, oldHealth, _health);
        HealthChanged(_health);
        if (_health == 0)
        {
            OnDead?.Invoke(instigator);
        }
    }

    [ClientRpc]
    void HealthChanged(int health)
    {
        OnHealthChanged?.Invoke(null, 0, health);
    }
}
