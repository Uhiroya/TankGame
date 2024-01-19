using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IStart, IPause , ITankData
{
    [SerializeField] private GameObject _destroyEffect;
    public TankData TankData;
    EnemyController _enemyController;
    private TankModel _model;

    void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
        _model = gameObject.AddComponent<TankModel>().Initialize(_destroyEffect, TankData.TankHP);
        _model.OnDead += RegisterEvent;
        _enemyController.enabled = false;
    }
    
    void RegisterEvent()
    {
        GameManager.Instance?.DestroyEnemy();
        Destroy(this.gameObject);
    }
    void OnEnable()
    {
        MyServiceLocator.IRegister(this as IPause);
        MyServiceLocator.IRegister(this as IStart);
    }

    void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IPause);
        MyServiceLocator.IUnRegister(this as IStart);
    }
    public void Active()
    {
        if(PhotonNetwork.IsMasterClient)
            _enemyController.enabled = true;
        print("Active CPU");
        
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
    public TankData GetTankData()
    {
        return TankData;
    }
}
