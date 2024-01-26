using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class TankMovement : MonoBehaviourPunCallbacks, IAnimAwake ,IActivatable
{
    [FormerlySerializedAs("startPositionY")] [SerializeField]
    private float _startPositionY = 50f;

    public Transform BrrelTransform;
    private Rigidbody _rb;
    private TankMoveParam _tankMoveParam;
    private bool _active;
    
    private ReactiveProperty<float> _inputMoveVertical = new();
    private ReactiveProperty<float> _inputMoveHorizontal= new();
    private ReactiveProperty<float> _inputVertical= new();
    private ReactiveProperty<float> _inputHorizontal= new();

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _tankMoveParam = GetComponent<ITankData>().GetTankData().TankMoveParam;
        transform.position += Vector3.up * _startPositionY;
    }

    private void FixedUpdate()
    {
        if (!_active) return;
        //MOVE
        _rb.velocity = transform.forward * (_inputMoveVertical.Value * _tankMoveParam._moveSpeed);
        //ROTATE
        transform.rotation *= Quaternion.Euler(0f, _inputMoveHorizontal.Value * _tankMoveParam._turnMoveSpeed, 0f);
        //TURN
        BrrelTransform.rotation *= Quaternion.Euler(0f, _inputVertical.Value * _tankMoveParam._turnBarrelSpeed, 0f);
    }

    private CancellationTokenSource _cts = new();
    private void Bind()
    {
        _inputMoveVertical.DistinctUntilChanged().Subscribe(SendInputMove).AddTo(_cts.Token);
        _inputMoveHorizontal.DistinctUntilChanged().Subscribe(SendInputTurn).AddTo(_cts.Token);
        _inputVertical.DistinctUntilChanged().Subscribe(SendInputBarrelTurn).AddTo(_cts.Token);
    }
    public override void OnEnable()
    {
        MyServiceLocator.IRegister(this as IActivatable);
        MyServiceLocator.IRegister(this as IAnimAwake);
    }

    public override void  OnDisable()
    {
        _cts?.Cancel(); 
        MyServiceLocator.IUnRegister(this as IActivatable);
        MyServiceLocator.IUnRegister(this as IAnimAwake);
    }

    public async UniTask AnimAwake(float time)
    {
        print("Start");
        var pos = transform.position;
        _ = transform.DOMove(new Vector3(pos.x, 0, pos.z), time);
        await transform.DOLocalRotate(new Vector3(0, 360, 0), time, RotateMode.FastBeyond360);
    }

    void SendInputMove(float input) =>
        photonView.RPC(nameof(InputMove) , RpcTarget.Others, input);
    
    void SendInputTurn(float input) =>
        photonView.RPC(nameof(InputTurn) , RpcTarget.Others, input);
    
    void SendInputBarrelTurn(float input) =>
        photonView.RPC(nameof(InputBarrelTurn) , RpcTarget.Others, input);
    
    [PunRPC]
    public void InputMove(float inputVertical)
    {
        _inputMoveVertical.Value = inputVertical;
    }
    [PunRPC]
    public void InputTurn(float inputHorizontal)
    {
        _inputMoveHorizontal.Value = inputHorizontal;
    }
    [PunRPC]
    public void InputBarrelTurn(float inputVertical)
    {
        _inputVertical.Value = inputVertical;
    }

    [PunRPC]
    public void PunBarrelTurn(float inputVertical)
    {
        var turn = inputVertical * _tankMoveParam._turnBarrelSpeed;
        var turnRotation = Quaternion.Euler(0f, turn, 0f);
        BrrelTransform.rotation *= turnRotation;
    }

    public void Active()
    {
        _cts = new();
        if(photonView.IsMine)
            Bind();
        _active = true;
    }

    public void DeActive()
    {
        _cts?.Cancel();   
        _active = false;
    }
}