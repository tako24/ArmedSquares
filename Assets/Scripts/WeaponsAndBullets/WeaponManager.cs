using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private List<Weapon> weapons;
    [SerializeField] private FixedJoystick joystick;
    private Weapon _currentWeapon;
    //private int _maxWeaponsCount;
    private int _currentWeaponIndex;

    private float _timeBtwShot;
    

    private void Start()
    {
        //_maxWeaponsCount = 3;
        _currentWeapon = weapons[_currentWeaponIndex];
    }

    public void ChangeWeapon()
    {
        var oldWeapon = _currentWeapon;
        _currentWeapon.gameObject.SetActive(false);
        _currentWeaponIndex = (_currentWeaponIndex + 1) % weapons.Count;
        _currentWeapon = weapons[_currentWeaponIndex];
        _currentWeapon.transform.rotation = oldWeapon.transform.rotation;
        _currentWeapon.gameObject.SetActive(true);
        _timeBtwShot = 0; //исправить
    }
    
    public void Fire()
    {
        _currentWeapon.Fire();
    }

    private void RotateWeapon()
    {
        var x = joystick.Horizontal;
        var y= joystick.Vertical;
        var vec = new Vector2(x, y).normalized;
        
        float rotation_z = Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
        _currentWeapon.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, rotation_z);   

    }

    private void Update()
    { 
        if (joystick.Horizontal != 0 && joystick.Vertical != 0) 
        {
            RotateWeapon();


            if (_timeBtwShot <= 0)
            {
                Fire();
                Debug.Log("shot");
                _timeBtwShot = _currentWeapon.WeaponStats.ShotCooldown;
            }
            else
            {
                _timeBtwShot -= Time.deltaTime;
            }
        }
    }
}
