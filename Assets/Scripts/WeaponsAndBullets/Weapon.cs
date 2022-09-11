using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField]private WeaponStats weaponStats;
    [SerializeField] private GameObject scope;
    
    public virtual void Fire(Vector3 vector3)
    {
        Instantiate(weaponStats.Bullet, vector3, quaternion.identity);
    }
}
