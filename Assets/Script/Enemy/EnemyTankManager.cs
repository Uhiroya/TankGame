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
        MyServiceLocator.Register(this as IPause);
        MyServiceLocator.Register(this as IStart);
    }

    void OnDisable()
    {
        GameManager.Instance?.DestroyEnemy();
        MyServiceLocator.UnRegister(this as IPause);
        MyServiceLocator.UnRegister(this as IStart);
    }

    void Start()
    {

    }

    void Update()
    {

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
