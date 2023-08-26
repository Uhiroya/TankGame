using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAction : MonoBehaviour
{
    [SerializeField] GameObject _bullet;
    [SerializeField] GameObject _nozzle;
    [SerializeField] Transform _burrelTransform;
    float _fireCoolTime;
    private float _fireTimer = 0f;
    void Awake()
    {
        _fireCoolTime = GetComponent<ITankData>().GetTankData().FireCoolTime;
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
        _fireTimer += Time.deltaTime;
    }
    public void OnFire(bool targeting)
    {
        if (!targeting)
        {
            _fireTimer = 0f;
        }
        if (_fireTimer > _fireCoolTime)
        {
            Instantiate(_bullet, _nozzle.transform.position, _burrelTransform.rotation);
            _fireTimer = 0f;
        }

    }
}
