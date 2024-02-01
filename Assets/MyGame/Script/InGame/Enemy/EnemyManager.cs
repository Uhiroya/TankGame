using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EnemyManager : MonoBehaviourPunCallbacks, IActivatable, IPause
{
    [SerializeField] private GameObject _destroyEffect;
    [SerializeField] private EnemyController _enemyController;
    [SerializeField] private TankController _tankController;

    void Awake()
    {
        _enemyController.enabled = false;
    }
    
    void DeadEvent()
    { 
        if (PhotonNetwork.IsMasterClient) 
            photonView.RPC(nameof(TryDestroy) , RpcTarget.AllViaServer);
    }
    [PunRPC]
    void TryDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            MasterGameManager.Instance.OnDestroyEnemy();
        }
        if (this)
        {
            AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.explotion);
            Instantiate(_destroyEffect, transform.position , _destroyEffect.transform.rotation) ;
            Destroy(gameObject);
        }
    }
    public override void OnEnable()
    {
        base.OnEnable();
        _tankController.DeadEvent += DeadEvent;
        MyServiceLocator.IRegister(this as IPause);
        MyServiceLocator.IRegister(this as IActivatable);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        _tankController.DeadEvent -= DeadEvent;
        MyServiceLocator.IUnRegister(this as IPause);
        MyServiceLocator.IUnRegister(this as IActivatable);
    }
    public void Active()
    {
        if(PhotonNetwork.IsMasterClient)
            _enemyController.enabled = true;
        
    }
    public void DeActive()
    {
        _enemyController.enabled = false;
    }

    public void Pause()
    {
        _enemyController.enabled = false;
    }
    public void Resume()
    {
        _enemyController.enabled = true;
    }
}
