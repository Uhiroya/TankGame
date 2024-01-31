using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum BulletType
{
    Normal = 0,
    Speed = 1,
    SlowBullet = 2,
    
}
[System.Serializable]
public class TankData
{
    [SerializeField , Header("発射する弾の種類")]
    public BulletType BulletType;
    [SerializeField , Header("初期HP")]
    public int TankHP;
    [SerializeField , Header("射撃クールタイム")]
    public float FireCoolTime = 1.5f;
    [SerializeField , Header("前後移動速度")]
    
    public float MoveSpeed = 1f;
    [SerializeField , Header("履帯の回転速度")]
    public float RotateSpeed = 1f;
    
    [SerializeField , Header("砲台の回転速度")]
    public float TurnBarrelSpeed = 1f;
}
[System.Serializable]
public class TankEnemyParam
{
    public float TargetSpeed = 1f;
    public float EnemyScanRadius = 20f;
    public float ScanMoveRange = 1.0f;
    public float MoveDelay = 1.0f;
}
