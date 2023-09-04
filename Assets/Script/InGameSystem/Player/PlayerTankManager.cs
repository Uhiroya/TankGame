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
