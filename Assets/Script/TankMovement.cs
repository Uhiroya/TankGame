using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class TankMovement : MonoBehaviour
{
    [SerializeField] private Transform _barrelTranform;
    [SerializeField] float _turnBarrelSpeed = 1f;
    [SerializeField] float _moveSpeed = 1f ;
    [SerializeField] float _turnMoveSpeed = 1f;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
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


    public void Move(float inputVertical)
    {
        Vector3 movement = inputVertical *  transform.forward * _moveSpeed;
        _rb.velocity = movement;
    }
    public void Turn(float inputHorizontal)
    {
        float turn = inputHorizontal * _turnMoveSpeed;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        transform.rotation *= turnRotation;
    }
    public void BarrelTurn(float inputVertical)
    {
        float turn = inputVertical * _turnBarrelSpeed;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        _barrelTranform.rotation *= turnRotation;
    }
}
