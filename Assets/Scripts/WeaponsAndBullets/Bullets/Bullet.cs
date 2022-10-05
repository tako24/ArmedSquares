using Mirror;
using Telepathy;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [Header("Bullet stats")]
    [SerializeField] protected int damage;
    [Tooltip("Speed in blocks per second")][SerializeField] protected float speed;
    [SerializeField] protected int overheat;

    public int Overheat => overheat;

    [HideInInspector][SyncVar] public Vector2 direction;
    [HideInInspector][SyncVar] public uint ownerId;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!isServer 
            || col.gameObject.GetComponent<Bullet>()                                // ignore other bullets
            || col.gameObject == this                                               // ignore self
            || col.gameObject == NetworkClient.spawned[ownerId].gameObject) return; // ignore owner
        OnHit(col);
    }

    [Server]
    public virtual void OnHit(Collider2D col)
    {
        //Debug.LogWarning(Time.time - time);
        var healthComponent = col.GetComponent<HealthComponent>();
        if (healthComponent)
            healthComponent.TakeDamage(NetworkServer.spawned[ownerId].gameObject, damage);
        NetworkServer.Destroy(gameObject);
    }

    //float time;
    private void Start()
    {
        OnStarting();
    }

    protected virtual void OnStarting()
    {
        //time = Time.time;
        var blockToSpeed = Time.fixedDeltaTime * 2500f;//50 - 0.02        x - 0.005
        var rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.AddForce(direction * speed * blockToSpeed);
    }
}
