using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShutgun  : Weapon
{

    public override void Fire()
    {
        var vec = bulletPosition.position.normalized;
        
        float rotation_z = Mathf.Atan2(vec.y+3, vec.x+3) * Mathf.Rad2Deg;
        var rotation1 = Quaternion.Euler(0f, 0f, rotation_z);
        var rotation2 = Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z + 10);
        Instantiate(weaponStats.Bullet, bulletPosition.position , rotation1);
        Instantiate(weaponStats.Bullet, bulletPosition.position, transform.rotation);
        Instantiate(weaponStats.Bullet, bulletPosition.position, rotation2 );
    }
}
