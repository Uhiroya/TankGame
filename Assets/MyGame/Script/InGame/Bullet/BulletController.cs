﻿using System;
using System.Collections;
using System.Collections.Generic;
using MyGame.Script.SingletonSystem;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent (typeof (Rigidbody))]
public class BulletController : MonoBehaviourPunCallbacks , IPause , IActivatable
{
    [SerializeField] private BulletType _bulletType;
    [SerializeField] int _maxReflectCount = 1;
    [SerializeField] float _bulletSpeed = 1.0f;
    [SerializeField] int _bulletDamage = 30;
    [SerializeField] float _lifeTime = 5f;
    [SerializeField] ParticleSystem _trailParticleSystem;
    [SerializeField] Rigidbody _bulletRigidBody;
    int _reflectCount = 1;
    float _myBulletSpeed;
    float _timer = 0;
    private int _ID;
    private static int _nextID;
    public void OnHit()
    {
        if(PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(BulletManager.Instance.ReleaseBullet) , RpcTarget.All , _bulletType , _ID );
    }
    
    public int Initialize(Vector3 position , Quaternion rotation)
    {
        _timer = 0f;
        _reflectCount = _maxReflectCount;
        _ID = _nextID;
        _nextID++;
        var transform1 = this.transform;
        transform1.position = position;
        transform1.rotation = rotation;
        return _ID;
    }
    void OnEnable()  
    {
        _myBulletSpeed = _bulletSpeed;
        MyServiceLocator.IRegister<IPause>(this);
        MyServiceLocator.IRegister<IActivatable>(this);
    }

    void OnDisable()
    {
        MyServiceLocator.IUnRegister<IPause>(this);
        MyServiceLocator.IUnRegister<IActivatable>(this);
    }
    void Update()
    {
        if (!PauseManager.IsPause)
        {
            _timer += Time.deltaTime;
            if( _timer > _lifeTime)
            {
                OnHit();
            }
        }
    }
    private void FixedUpdate()
    {
        _bulletRigidBody.MovePosition(_bulletRigidBody.position + transform.forward * _bulletSpeed * Time.deltaTime);
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Field")
        {
            if(_reflectCount > 0)
            {
                //Debug.Log("ぶつかった");
                AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.reflectBullet);
                _reflectCount -= 1;
                Vector3 dir = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
                transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                OnHit();
            }
        }
        if (collision.transform.tag == "Enemy" || collision.transform.tag == "Player")
        {
            collision.transform.gameObject.GetComponent<TankModel>()?.TakeDamage(_bulletDamage);
            OnHit();
        }
    }
    void OnCollisionStay(Collision collision)
    {
        if (collision.transform.tag == "Field")
        {
            OnHit();
        }
    }
    public void Pause()
    {
        _bulletSpeed = 0;
        _bulletRigidBody.Sleep();
        _trailParticleSystem.Pause();
    }
    public void Resume()
    {
        _bulletRigidBody.WakeUp(); 
        _bulletSpeed = _myBulletSpeed;
        _trailParticleSystem.Play();
    }
    public void Active()
    {
    }
    public void DeActive()
    {
        OnHit();
    }
}
