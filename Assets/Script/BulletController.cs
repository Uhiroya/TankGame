using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent (typeof (Rigidbody))]
public class BulletController : MonoBehaviour
{
    [SerializeField] int _reflectCount = 1;
    [SerializeField] float _bulletSpeed = 1.0f;
    Rigidbody _bulletRigidbody;
    void Awake()
    {
        _bulletRigidbody = GetComponent<Rigidbody> ();
    }

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        
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
        if (collision.transform.tag == "Tank")
        {
             Destroy(gameObject);
        }
    }
}
