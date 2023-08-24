using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAction : MonoBehaviour
{
    [SerializeField] GameObject _bullet;
    [SerializeField] GameObject _nozzle;
    [SerializeField] Transform _burrelTransform;
    void Awake()
    {
        
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
    public void Fire()
    {
        Instantiate(_bullet, _nozzle.transform.position, _burrelTransform.rotation);
    }
}
