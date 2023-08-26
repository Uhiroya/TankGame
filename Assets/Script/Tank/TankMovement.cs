using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class TankMovement : MonoBehaviour
{
    public Transform _barrelTranform;

    private Rigidbody _rb;
    TankMoveParam _tankMoveParam;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _tankMoveParam = GetComponent<ITankData>().GetTankData().TankMoveParam;
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
        Vector3 movement = inputVertical *  transform.forward * _tankMoveParam._moveSpeed;
        _rb.velocity = movement;
    }
    public void Turn(float inputHorizontal)
    {
        float turn = inputHorizontal * _tankMoveParam._turnMoveSpeed;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        transform.rotation *= turnRotation;
    }
    public void BarrelTurn(float inputVertical)
    {
        float turn = inputVertical * _tankMoveParam._turnBarrelSpeed;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        _barrelTranform.rotation *= turnRotation;
    }
}
