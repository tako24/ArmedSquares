using Mirror;
using UnityEngine;

//Ёффект - —нар€д наносит урон в радиусе 3 блока
public class BulletGiga : Bullet
{
    [SerializeField] private float damageRadius = 10f;
    public override void OnHit(Collider2D col)
    {
        var objects = Physics2D.OverlapCircleAll(transform.position, damageRadius);
        Debug.LogWarning("objects length " + objects.Length);
        foreach(var hitCollider in objects)
        {
            Debug.LogWarning(hitCollider.gameObject.name);
            var healthComponent = hitCollider.GetComponent<HealthComponent>();
            if (!healthComponent) continue;

            var closestPoint = hitCollider.ClosestPoint(transform.position);
            var distance = Vector2.Distance(closestPoint, transform.position);
            var damagePercent = Mathf.InverseLerp(damageRadius, 0, distance);
            Debug.LogWarning("damagePercent " + damagePercent);
            healthComponent.TakeDamage(NetworkServer.spawned[ownerId].gameObject, (int)(damage * damagePercent));
        }
        NetworkServer.Destroy(gameObject);
    }
}
