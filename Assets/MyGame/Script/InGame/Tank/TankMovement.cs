using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using Photon.Pun;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class TankMovement : MonoBehaviourPunCallbacks, IAnimAwake
{
    [FormerlySerializedAs("startPositionY")] [SerializeField] private float _startPositionY = 50f;
    public Transform BrrelTransform;
    private Rigidbody _rb;
    TankMoveParam _tankMoveParam;

    void OnEnable()
    {
        print("読み込まれました");
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
        transform.position += Vector3.up * _startPositionY;
    }
    public async UniTask AnimAwake(float time)
    {
        var pos = transform.position;
        _ = transform.DOMove(new Vector3(pos.x ,0 , pos.z), time);
        await transform.DOLocalRotate(new Vector3(0, 360, 0), time, RotateMode.FastBeyond360);
    }
    public void Move(float inputVertical)
    {
        photonView.RPC(nameof(PunMove) , RpcTarget.All , inputVertical);
    }
    [PunRPC]
    public void PunMove(float inputVertical)
    {
        Vector3 movement = transform.forward * (inputVertical * _tankMoveParam._moveSpeed);
        _rb.velocity = movement;
    }
    public void Turn(float inputHorizontal)
    {
        photonView.RPC(nameof(PunTurn) , RpcTarget.All , inputHorizontal);
    }
    [PunRPC]
    public void PunTurn(float inputHorizontal)
    {
        float turn = inputHorizontal * _tankMoveParam._turnMoveSpeed;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        transform.rotation *= turnRotation;
    }
    public void BarrelTurn(float inputVertical)
    {
        photonView.RPC(nameof(PunBarrelTurn) , RpcTarget.All , inputVertical);
    }
    [PunRPC]
    public void PunBarrelTurn(float inputVertical)
    {
        float turn = inputVertical * _tankMoveParam._turnBarrelSpeed;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        BrrelTransform.rotation *= turnRotation;
    }

}