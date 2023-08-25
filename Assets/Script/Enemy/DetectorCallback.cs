using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
public class DetectorCallback : MonoBehaviour
{
    [SerializeField] string _detectorTag = "";
    [SerializeField] UnityEvent<Collider> _onhit;
    [SerializeField] UnityEvent<Collider> _onStay;

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
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == _detectorTag)
        {
            _onhit?.Invoke(other);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == _detectorTag)
        {
            _onStay?.Invoke(other);
        }
    }
}
