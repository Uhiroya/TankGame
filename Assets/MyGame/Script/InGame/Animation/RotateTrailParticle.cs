using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTrailParticle : MonoBehaviour , IActivatable
{
    [SerializeField] private ParticleSystem _particleSystem;
    private ParticleSystem.MainModule _mainModule;
    private ParticleSystem.EmissionModule _emissionModule;
    void Start()
    {
        _mainModule = _particleSystem.main;
        _emissionModule = _particleSystem.emission;
    }
    
    void Update()
    {
        _mainModule.startRotation = Mathf.Deg2Rad *  transform.rotation.eulerAngles.y ;
    }
    void OnEnable()
    {
        MyServiceLocator.IRegister(this as IActivatable);
    }
    void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IActivatable);
    }

    public void Active()
    {
        _emissionModule.enabled = true;
    }

    public void DeActive()
    {
        _emissionModule.enabled = false;
    }
}
