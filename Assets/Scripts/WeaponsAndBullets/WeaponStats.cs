using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponState", menuName = "Items/Create new WeaponState")]
public class WeaponStats : ScriptableObject
{
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject bullet;
    
    public Sprite Icon => icon;
    public GameObject Bullet => bullet;
}
