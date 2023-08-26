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

    }

    void OnDisable()
    {

    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void Active()
    {
        _enemyController.enabled = true;
    }

    public void Pause()
    {
        throw new System.NotImplementedException();
    }

    public void Resume()
    {
        throw new System.NotImplementedException();
    }

    public TankData GetTankData()
    {
        return TankData;
    }
}
