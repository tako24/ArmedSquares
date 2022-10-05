using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    private readonly List<Weapon> _weapons = new List<Weapon>();
    private Weapon _currentWeapon;
    private int _currentWeaponIndex = 0;
    [SerializeField] float changeWeaponTime = 1.0f;
    private bool _isChangingWeapon = false;

    public uint GetCurrentWeaponID() => _currentWeaponID;
    [SyncVar(hook = nameof(CurrentWeaponChanged))] uint _currentWeaponID = 0;
    [SerializeField] private GameObject[] weaponPrefab;

    private void Start()
    {
        if (!isServer) return;
        foreach(var e in weaponPrefab)
            CreateWeapon(e);
    }

    private void Update()
    {
        Debug.Log("_currentWeaponID " + _currentWeaponID);
        if (!isLocalPlayer) return;

        Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
        float angle = Mathf.Atan2(mouseOnScreen.y - positionOnScreen.y, mouseOnScreen.x - positionOnScreen.x) * Mathf.Rad2Deg; //AngleBetweenTwoPoints
        RotateTurret(angle);

        if(Input.GetMouseButton(0))
            Fire();
        if (Input.GetMouseButton(1))
            ChangeWeapon();
    }

    [Command]
    void Fire()
    {
        _currentWeapon.Fire();
    }

    [Command]
    void RotateTurret(float angle)
    {
        _currentWeapon.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
    }

    [Command]
    public void ChangeWeapon()
    {
        if (_isChangingWeapon) return;
        StartCoroutine(ChengeWeaponCoroutine());
    }

    private IEnumerator ChengeWeaponCoroutine()
    {
        _isChangingWeapon = true;
        _currentWeaponIndex = (_currentWeaponIndex + 1) % _weapons.Count;
        
        _weapons[_currentWeaponIndex].transform.rotation = _currentWeapon.transform.rotation;

        _currentWeapon = _weapons[_currentWeaponIndex];
        _currentWeaponID = _weapons[_currentWeaponIndex].netId;
        yield return new WaitForSeconds(changeWeaponTime);
        _isChangingWeapon = false;
    }

    void CurrentWeaponChanged(uint oldId, uint newId)
    {
        if (isServer)
        {
            if(oldId != 0)
                NetworkServer.spawned[oldId].GetComponent<SpriteRenderer>().enabled = false;
            NetworkServer.spawned[newId].GetComponent<SpriteRenderer>().enabled = true;
        }
        else if (isClient)
        {
            if (oldId != 0)
            {
                NetworkClient.spawned[oldId].GetComponent<SpriteRenderer>().enabled = false;
                NetworkClient.spawned[newId].transform.rotation = NetworkClient.spawned[oldId].transform.rotation;
            }
            NetworkClient.spawned[newId].GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    [Server]
    public void CreateWeapon(GameObject prefab)
    {
        GameObject createdWeapon = Instantiate(prefab, transform.position, Quaternion.identity);

        if (_currentWeapon)
            _currentWeapon.GetComponent<SpriteRenderer>().enabled = false;

        _currentWeapon = createdWeapon.GetComponent<Weapon>();
        _currentWeapon.ownerId = netId;
        _currentWeapon.transform.SetParent(transform);
        NetworkServer.Spawn(createdWeapon);

        _currentWeaponID = _currentWeapon.netId;
        _weapons.Add(_currentWeapon);
        RpcWeaponCreated();
    }

    [ClientRpc]
    void RpcWeaponCreated()
    {
    }
}
