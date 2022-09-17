using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public  class Weapon : MonoBehaviour
{
    [SerializeField]private protected WeaponStats weaponStats;
    [SerializeField] private protected Transform bulletPosition;

    public Transform BulletPosition => bulletPosition;
    public WeaponStats WeaponStats => weaponStats; 
    
    
    
    public virtual void Fire()
    {
        Instantiate(weaponStats.Bullet, bulletPosition.position, transform.rotation);

    }
}
