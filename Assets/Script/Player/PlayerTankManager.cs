using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTankManager : MonoBehaviour , IStart , IPause , ITankData
{
    public TankData TankData;
    PlayerTankInputManager _playerInputManager;
    void Awake()
    {
        _playerInputManager = GetComponent<PlayerTankInputManager>();
        //_playerInputManager.enabled = false;
    }

    void OnEnable()
    {
        MyServiceLocator.Register(this as IPause);
        MyServiceLocator.Register(this as IStart);
    }

    void OnDisable()
    {
        MyServiceLocator.UnRegister(this as IPause);
        MyServiceLocator.UnRegister(this as IStart);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
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
        _playerInputManager.enabled = false;
    }

    public void Resume()
    {
        _playerInputManager.enabled = true;
    }
}
