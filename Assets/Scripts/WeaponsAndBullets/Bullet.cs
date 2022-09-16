using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public  class Bullet : MonoBehaviour
{
    [SerializeField] private BulletStats bulletStats;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask layerMask;
    private float _distance = 0.05f;
    private void Start()
    {
        
    }

    private void Update()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, transform.right, _distance, layerMask);
        if (hitInfo.collider != null)
        {
            if (hitInfo.collider.CompareTag("Enemy"))
            {
                hitInfo.collider.GetComponent<HealthComponent>().TakeDamage(gameObject,bulletStats.Damage);
                Debug.Log(hitInfo.collider.GetComponent<HealthComponent>().GetCurrentHP);
            }
            Destroy(gameObject);   
        }
        transform.Translate(Vector2.right * (bulletStats.Speed * Time.deltaTime));
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
    }

    public virtual void OnHit(Collision2D col)
    {
        
    }
}
