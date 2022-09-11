using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BulletState", menuName = "Items/Create new BulletState")]
public class BulletStats : ScriptableObject
{
    [SerializeField] private int damage;
    [SerializeField] private float speed;
    [SerializeField] private Sprite icon;

    public int Damage => damage;
    public float Speed => speed;
    public Sprite Icon => icon;
}
