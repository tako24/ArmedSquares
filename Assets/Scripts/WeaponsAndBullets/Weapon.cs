using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public  class Weapon : MonoBehaviour
{
    [SerializeField]private WeaponStats weaponStats;
    [SerializeField] private Transform bulletPosition;

    public Transform BulletPosition => bulletPosition;
    public WeaponStats WeaponStats => weaponStats; 
    
    
    
    public  void Fire()
    {
        Instantiate(weaponStats.Bullet, bulletPosition.position, transform.rotation);

    }
}
