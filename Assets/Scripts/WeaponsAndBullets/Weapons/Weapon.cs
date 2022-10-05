using Mirror;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected GameObject scope;
    [HideInInspector][SyncVar] public uint ownerId = 0;

    [Header("Weapon stats")]
    [SerializeField] protected float fireRate = 2.0f;
    protected bool _rateDelayReady = true;
    [SerializeField] protected int maxOverheat = 120;
    [SyncVar] protected int _overheat = 0;
    [SerializeField] protected int coolingSpeed = 4;
    protected Coroutine coolingRoutine;

    private void Start()
    {
        var healthComponent = GetComponentInParent<HealthComponent>();
        if(healthComponent)
        {
            healthComponent.OnDead += RpcOwnerDead;
        }
        var playerCharacter = GetComponentInParent<PlayerCharacter>();
        if(playerCharacter)
        {
            playerCharacter.OnRespawn += RpcOwnerRespawned;
        }
    }

    [ClientRpc]
    private void RpcOwnerRespawned()
    {
        var wm = NetworkClient.spawned[ownerId].GetComponent<WeaponManager>();
        var currentWeaponId = wm.GetCurrentWeaponID();
        GetComponent<SpriteRenderer>().enabled = currentWeaponId == netId;
    }

    [ClientRpc]
    private void RpcOwnerDead(GameObject instigator)
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public override void OnStartClient()
    {
        var owner = NetworkClient.spawned[ownerId];
        transform.position = owner.transform.position;
        transform.SetParent(owner.transform);

        var wm = owner.GetComponent<WeaponManager>();
        var currentWeaponId = wm.GetCurrentWeaponID();
        GetComponent<SpriteRenderer>().enabled = currentWeaponId == netId;
    }

    void Update()
    {
        Debug.Log("Weapon overheat " + _overheat);
        Debug.Log("id " + netId);
    }

    [Server]
    public virtual void Fire()
    {
        var overheat = _overheat + bulletPrefab.GetComponent<Bullet>().Overheat;
        if (!_rateDelayReady || _overheat >= maxOverheat || overheat > maxOverheat) return;

        if (coolingRoutine != null)
            StopCoroutine(coolingRoutine);

        Invoke("FireDelay", fireRate);

        GameObject projectile = Instantiate(bulletPrefab, scope.transform.transform.position, quaternion.identity);
        var bullet = projectile.GetComponent<Bullet>();
        bullet.direction = transform.right;
        bullet.ownerId = netId;
        NetworkServer.Spawn(projectile);

        _overheat = Mathf.Min(overheat, maxOverheat);
        if (_overheat != 0)
            coolingRoutine = StartCoroutine(Overheat());
        
        _rateDelayReady = false;
        RpcOnFire();
    }

    void FireDelay()
    {
        _rateDelayReady = true;
    }

    [ClientRpc]
    void RpcOnFire()
    {
    }

    IEnumerator Overheat()
    {
        while (_overheat > 0)
        {
            yield return new WaitForSeconds(1f);
            var overheat = _overheat - coolingSpeed;
            _overheat = Mathf.Max(overheat, 0);
            Debug.Log("Cooling weapon overheat " + _overheat);
        }
        yield return null;
    }
}
