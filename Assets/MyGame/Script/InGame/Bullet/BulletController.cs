using System;
using System.Collections;
using System.Collections.Generic;
using MyGame.Script.SingletonSystem;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent (typeof (Rigidbody))]
public class BulletController : MonoBehaviourPunCallbacks
{
    [SerializeField] private BulletType _bulletType;
    [SerializeField] int _maxReflectCount = 1;
    [SerializeField] float _bulletSpeed = 1.0f;
    [SerializeField] int _bulletDamage = 30;
    [SerializeField] float _maxLifeTime = 5f;
    [SerializeField] ParticleSystem _trailParticleSystem;
    [SerializeField] Rigidbody _bulletRigidBody;

    public BulletType BulletType => _bulletType;
    int _reflectCount = 1;
    float _myBulletSpeed;
    float _lifeTimer = 0;
    private int _id;
    private bool _isActive;
    public void OnRelease()
    {
        if (_isActive && PhotonNetwork.IsMasterClient)
        {
            BulletsManager.Instance.CallReleaseBullet( _bulletType , _id );
            _isActive = false;
        }
    }
    
    public void Initialize(Vector3 position , Quaternion rotation , int bulletID)
    {
        _myBulletSpeed = _bulletSpeed;
        _reflectCount = _maxReflectCount;
        _lifeTimer = _maxLifeTime;
        _isActive = true;
        _id = bulletID;
        transform.position = position;
        transform.rotation = rotation;
    }

    public void Release()
    {
        _isActive = false;
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (!_isActive) return;
        _lifeTimer -= Time.fixedDeltaTime;
        if( _lifeTimer < 0f)
        {
            OnRelease();
        }
        //ミサイルを飛ばす
        transform.position +=  transform.forward * (_bulletSpeed * Time.fixedDeltaTime);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (!_isActive) return;
        if(collision.transform.CompareTag("Field"))
        {
            if(_reflectCount > 0)
            {
                //反射処理
                AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.reflectBullet);
                _reflectCount -= 1;
                Vector3 dir = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
                transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                OnRelease();
            }
        }
        if (collision.transform.CompareTag("Enemy") || collision.transform.CompareTag("Player"))
        {
            collision.transform.gameObject.GetComponent<Damageable>()?.TakeDamage(_bulletDamage);
            OnRelease();
        }
    }
    /// <summary>
    /// 埋まったとき
    /// </summary>
    void OnCollisionStay(Collision collision)
    {
        if (!_isActive) return;
        if (collision.transform.CompareTag("Field"))
        {
            OnRelease();
        }
    }
}
