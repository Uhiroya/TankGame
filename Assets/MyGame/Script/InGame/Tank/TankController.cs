using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyGame.Script.SingletonSystem;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviourPunCallbacks, IAwakeAnim ,IActivatable ,IPause
{
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] Transform _burrelTransform;
    [SerializeField] private TankData _tankData;
    const float StartPositionY = 50f;

    public Transform BurrelTransform => _burrelTransform;
    public TankData TankData => _tankData;

    private bool _active;
    private Damageable _damageable;
    
    
    private float _inputMoveVertical ;
    private float _inputMoveHorizontal;
    private float _inputVertical;
    

    private readonly TankInputSync _tankInputSync;
    
    public event Action DeadEvent;

    public void OnDead() => DeadEvent?.Invoke();

    public void ChangeImmortal(bool flag)
    {
        _damageable.IsImmortal = flag;
    }
    
    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        transform.position += Vector3.up * StartPositionY;
        
        
        
         _damageable = gameObject.AddComponent<Damageable>().Initialize(_tankData.TankHP);
    }
    

    private void FixedUpdate()
    {
        if (!_active) return;
        //MOVE
        _rigidBody.velocity = transform.forward * (_inputMoveVertical* _tankData.MoveSpeed);
        //ROTATE
        transform.rotation *= Quaternion.Euler(0f, _inputMoveHorizontal * _tankData.RotateSpeed, 0f);
        //TURN
        _burrelTransform.rotation *= Quaternion.Euler(0f, _inputVertical * _tankData.TurnBarrelSpeed, 0f);
        
        //Fire準備

    }
    [PunRPC]
    public void GetInputMove(float inputVertical, Vector3 position)
    {
        //transform.position = position;
        _inputMoveVertical = inputVertical;
    }

    [PunRPC]
    public void GetInputTurn(float inputHorizontal, Quaternion rotation)
    {
        //transform.rotation = rotation;
        _inputMoveHorizontal = inputHorizontal;
    }

    [PunRPC]
    public void GetInputBarrelTurn(float inputVertical, Quaternion rotation)
    {
        //_burrelTransform.rotation = rotation;
        _inputVertical = inputVertical;
    }

    public override void OnEnable()
    {
        _damageable.OnDead += OnDead;
        base.OnEnable();
        MyServiceLocator.IRegister(this as IActivatable);
        MyServiceLocator.IRegister(this as IAwakeAnim);
        MyServiceLocator.IRegister(this as IPause);
    }

    public override void  OnDisable()
    {
        base.OnDisable();
        _damageable.OnDead -= OnDead;
        MyServiceLocator.IUnRegister(this as IActivatable);
        MyServiceLocator.IUnRegister(this as IAwakeAnim);
        MyServiceLocator.IUnRegister(this as IPause);
    }



    #region インターフェース実装
    public async UniTask AnimAwake(float time)
    {
        var pos = transform.position;
        _ = transform.DOMove(new Vector3(pos.x, 0, pos.z), time);
        await transform.DOLocalRotate(new Vector3(0, 360, 0), time, RotateMode.FastBeyond360);
    }
    public void Active()
    {
        _active = true;
    }

    public void DeActive()
    {
        _active = false;
    }
    public void Pause()
    {
        _active = true;
    }

    public void Resume()
    {
        _active = false;
    }
    

    #endregion



}