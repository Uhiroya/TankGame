using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PlayerManager : MonoBehaviour , IStart , IPause , ITankData 
{
    [SerializeField] GameObject _destroyEffect;
    [SerializeField] bool _immortal = false;
    public TankData TankData;
    private TankModel _model;
    PlayerInputManager _playerInputManager;
    
    void Awake()
    {
        _model = gameObject.AddComponent<TankModel>().Initialize(_destroyEffect, TankData.TankHP, _immortal);
        _playerInputManager = gameObject.AddComponent<PlayerInputManager>();

        _model.OnDead += RegisterEvent;
        //_playerInputManager.enabled = false;
    }

    void RegisterEvent()
    {
        GameManager.Instance?.GameOver();
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


    public TankData GetTankData()
    {
        return TankData;
    }
    public void Active()
    {
        _playerInputManager.enabled = true;
    }
    public void InActive()
    {
        _playerInputManager.enabled = false;
    }
    public void Pause()
    {
        _playerInputManager.StopTankAudio();
        _playerInputManager.enabled = false;
    }

    public void Resume()
    {
        _playerInputManager.enabled = true;
    }
}
