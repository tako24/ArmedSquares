using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Bullet : MonoBehaviour
{
    [SerializeField] private BulletStats bulletStats;

    private void OnCollisionEnter2D(Collision2D col)
    {
        
        OnHit(col);
    }

    public virtual void OnHit(Collision2D col)
    {
        
    }
}
