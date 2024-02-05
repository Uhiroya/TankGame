using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    [FormerlySerializedAs(nameof(BulletType))][SerializeField , Header("発射する弾の種類")]
    public BulletType _bulletType;
    [FormerlySerializedAs(nameof(TankHP))][SerializeField , Header("初期HP")]
    public int _tankHp;
    [FormerlySerializedAs(nameof(FireCoolTime))][SerializeField , Header("射撃クールタイム")]
    public float _fireCoolTime = 1.5f;
    [FormerlySerializedAs(nameof(MoveSpeed))][SerializeField , Header("前後移動速度")]
    public float _moveSpeed = 1f;
    [FormerlySerializedAs(nameof(RotateSpeed))][SerializeField , Header("履帯の回転速度")]
    public float _rotateSpeed = 1f;
    [FormerlySerializedAs(nameof(TurnBarrelSpeed))][SerializeField , Header("砲台の回転速度")]
    public float _turnBarrelSpeed = 1f;
    
    public BulletType BulletType => _bulletType;
    public int TankHP => _tankHp;
    public float FireCoolTime =>_fireCoolTime;
    public float MoveSpeed => _moveSpeed;
    public float RotateSpeed => _rotateSpeed;
    public float TurnBarrelSpeed => _turnBarrelSpeed;
}
