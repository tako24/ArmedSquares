using UnityEngine;

public class BulletAccelerator : Bullet
{
    private float startTime = 0.0f;

    public override void OnHit(Collider2D col)
    {
        var lifeTime = Time.time - startTime;
        var timePerBlock = 1.0f / speed;
        var damageMultiplier = lifeTime / timePerBlock;
        damage += damage * (int)damageMultiplier;
        //Debug.LogWarning("multip " + damageMultiplier);
        //Debug.LogWarning("damage " + damage);
        base.OnHit(col);
    }

    private void Start()
    {
        startTime = Time.time;
        base.OnStarting();
    }
}
