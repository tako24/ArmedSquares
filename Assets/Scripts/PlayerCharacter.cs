using Mirror;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacter : NetworkBehaviour
{
    bool bIsDead = false;
    public Action OnRespawn;
    [SerializeField] private GameObject[] objectsDisableOnDeath;
    [SerializeField] private Behaviour[] componentsDisableOnDeath;
    [SerializeField] private GameObject playerCanvas;
    private void Start()
    {
        if (!isLocalPlayer)
        {
            playerCanvas.SetActive(false);
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
        }

        var healthComponent = GetComponent<HealthComponent>();
        OnRespawn += healthComponent.Respawn;
        healthComponent.OnDead += Die;
    }

    [ClientRpc]
    void RpcCharacterEnable(bool enable)
    {
        for (int i = 0; i < componentsDisableOnDeath.Length; i++)
            componentsDisableOnDeath[i].enabled = enable;
        for (int i = 0; i < objectsDisableOnDeath.Length; i++)
            objectsDisableOnDeath[i].SetActive(enable);

        GetComponent<SpriteRenderer>().enabled = enable;
    }

    private void Die(GameObject instigator)
    {
        bIsDead = true;
        RpcCharacterEnable(false);
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1.0f);// respawn time
        
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        yield return new WaitForSeconds(0.1f);
        bIsDead = false;
        RpcCharacterEnable(true);
        OnRespawn.Invoke();
    }
}
