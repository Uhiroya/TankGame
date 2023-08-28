using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent (typeof (Rigidbody))]
public class BulletController : MonoBehaviour , IPause
{
    [SerializeField] int _reflectCount = 1;
    [SerializeField] float _bulletSpeed = 1.0f;
    [SerializeField] int _bulletDamege = 30;
    Rigidbody _bulletRigidbody;
    float _myBulletSpeed;
    void Awake()
    {
        _bulletRigidbody = GetComponent<Rigidbody> ();
    }

    void OnEnable()
    {
        _myBulletSpeed = _bulletSpeed;
        MyServiceLocator.Register<IPause>(this);
    }

    void OnDisable()
    {
        MyServiceLocator.UnRegister<IPause>(this);
    }

    void Start()
    {
        Destroy(gameObject , 5f);
    }

    void Update()
    {
        
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
                Debug.Log("‚Ô‚Â‚©‚Á‚½");
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
    public void Pause()
    {
        _bulletSpeed = 0;
        _bulletRigidbody.Sleep();
    }
    public void Resume()
    {
        _bulletRigidbody.WakeUp(); 
        _bulletSpeed = _myBulletSpeed;
    }
}
