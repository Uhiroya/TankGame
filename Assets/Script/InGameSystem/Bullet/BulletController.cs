using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent (typeof (Rigidbody))]
public class BulletController : MonoBehaviour , IPause , IStart
{
    [SerializeField] int _reflectCount = 1;
    [SerializeField] float _bulletSpeed = 1.0f;
    [SerializeField] int _bulletDamege = 30;
    [SerializeField] float _lifeTime = 5f;
    [SerializeField] ParticleSystem _trailParticleSystem;
    Rigidbody _bulletRigidbody;
    float _myBulletSpeed;
    float _timer = 0;
    void Awake()
    {
        _bulletRigidbody = GetComponent<Rigidbody> ();
    }

    void OnEnable()
    {
        _myBulletSpeed = _bulletSpeed;
        MyServiceLocator.IRegister<IPause>(this);
        MyServiceLocator.IRegister<IStart>(this);
    }

    void OnDisable()
    {
        MyServiceLocator.IUnRegister<IPause>(this);
        MyServiceLocator.IUnRegister<IStart>(this);
    }
    void Update()
    {
        if (!PauseManager.IsPause)
        {
            _timer += Time.time;
            if( _timer > _lifeTime)
            {
                Destroy(gameObject);
            }
        }
    }
    private void FixedUpdate()
    {
        _bulletRigidbody.MovePosition(_bulletRigidbody.position + transform.forward * _bulletSpeed * Time.deltaTime);
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Field")
        {
            if(_reflectCount > 0)
            {
                //Debug.Log("‚Ô‚Â‚©‚Á‚½");
                AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.reflectBullet);
                _reflectCount -= 1;
                Vector3 dir = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
                transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        if (collision.transform.tag == "Enemy" || collision.transform.tag == "Player")
        {
            collision.transform.gameObject.GetComponent<TankHelth>()?.TakeDamege(_bulletDamege);
            Destroy(gameObject);
        }
    }
    void OnCollisionStay(Collision collision)
    {
        if (collision.transform.tag == "Field")
        {
            Destroy(gameObject);
        }
    }
    public void Pause()
    {
        _bulletSpeed = 0;
        _bulletRigidbody.Sleep();
        _trailParticleSystem.Pause();
    }
    public void Resume()
    {
        _bulletRigidbody.WakeUp(); 
        _bulletSpeed = _myBulletSpeed;
        _trailParticleSystem.Play();
    }
    public void Active()
    {
    }
    public void InActive()
    {
        Destroy(gameObject);
    }
}
