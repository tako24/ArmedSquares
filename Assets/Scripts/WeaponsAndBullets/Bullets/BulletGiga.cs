using Mirror;
using UnityEngine;

//������ - ������ ������� ���� � ������� 3 �����
public class BulletGiga : Bullet
{
    [SerializeField] private float radius = 3f;
    public override void OnHit(Collider2D col)
    {
        var objects = Physics2D.OverlapCircleAll(transform.position, radius);
        Debug.LogWarning("objects length " + objects.Length);
        foreach(var hitCollider in objects)
        {
            var closestPoint = hitCollider.ClosestPoint(transform.position);
            var distance = Vector2.Distance(closestPoint, transform.position);
            var damagePercent = (int)(Mathf.InverseLerp(radius, 0, distance) * 100f);
            Debug.LogWarning("damagePercent " + damagePercent);
            Damage(hitCollider, damage * damagePercent);
        }
        NetworkServer.Destroy(gameObject);
    }
}
