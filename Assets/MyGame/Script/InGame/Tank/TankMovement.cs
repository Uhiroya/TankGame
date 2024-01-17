using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
[RequireComponent(typeof(Rigidbody))]
public class TankMovement : MonoBehaviour, IAnimAwake
{
    public Transform BrrelTransform;
    private Rigidbody _rb;
    TankMoveParam _tankMoveParam;

    void OnEnable()
    {
        MyServiceLocator.IRegister(this as IAnimAwake);
    }
    void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IAnimAwake);
    }
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        
        _tankMoveParam = GetComponent<ITankData>().GetTankData().TankMoveParam;
    }

    public void Move(float inputVertical)
    {
        Vector3 movement = inputVertical * transform.forward * _tankMoveParam._moveSpeed;
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
        BrrelTransform.rotation *= turnRotation;
    }

    public async UniTask AnimAwake(float time)
    {
        var defPos = transform.position;
        transform.position = defPos + Vector3.up * 35;
        _ = transform.DOMove(defPos, time);
        await transform.DOLocalRotate(new Vector3(0, 360, 0), time, RotateMode.FastBeyond360);
    }
}