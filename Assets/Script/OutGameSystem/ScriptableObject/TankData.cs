using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TankData 
{
    public TankType TankType;
    public int TankHP;
    public float FireCoolTime = 1.5f;
    public TankMoveParam TankMoveParam;
    public TankEnemyParam TankEnemyParam;
}
[System.Serializable]
public enum TankType 
{ 
    Player,
    EnemyYellow,
    EnemyBlue,
    EnemyRed,
    EnemyGreen,
}
[System.Serializable]
public class TankMoveParam
{
    public float _turnBarrelSpeed = 1f;
    public float _moveSpeed = 1f;
    public float _turnMoveSpeed = 1f;
}
[System.Serializable]
public class TankEnemyParam
{
    public float _targetSpeed = 1f;
    public float _enemyScanRadius = 20f;
    public float _scanMoveRange = 1.0f;
    public float _moveDelay = 1.0f;
}
