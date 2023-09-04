using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTankManager : MonoBehaviour, IStart, IPause , ITankData
{
    public TankData TankData;
    EnemyController _enemyController;
    void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
        //_enemyController.enabled = false;
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
        print("Active CPU");
        _enemyController.enabled = true;
    }
    public void InActive()
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
