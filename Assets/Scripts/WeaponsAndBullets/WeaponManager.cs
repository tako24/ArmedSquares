using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private List<Weapon> weapons;
    [SerializeField] private GameObject scope;
    private Weapon _currentWeapon;
    //private int _maxWeaponsCount;
    private int _currentWeaponIndex;
    

    private void Start()
    {
        //_maxWeaponsCount = 3;
        _currentWeapon = weapons[_currentWeaponIndex];
    }

    public void ChangeWeapon()
    {
        _currentWeaponIndex = (_currentWeaponIndex + 1) % weapons.Count;
        _currentWeapon = weapons[_currentWeaponIndex];
    }
    public void Fire()
    {
        _currentWeapon.Fire(scope.transform.position - transform.position);
    }
    
}
