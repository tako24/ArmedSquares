using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
     private int _health;

     public int GetCurrentHP => _health;

    
    // instigator
    public event Action<GameObject> OnDead;

    // instigator, oldHealth, newHealth
    public event Action<GameObject, int, int> OnHealthChanged;

    private void Start()
    {
        _health = maxHealth;
    }

    
    public void TakeDamage(GameObject instigator, int damage)
    {
        var oldHealth = _health;
        _health = Mathf.Clamp(_health - damage, 0, maxHealth);
        
        OnHealthChanged?.Invoke(instigator, oldHealth, _health);

        if (_health == 0)
        {
            OnDead?.Invoke(instigator);
        }
    }
}
