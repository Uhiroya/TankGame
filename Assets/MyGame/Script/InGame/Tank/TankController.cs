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
    [SerializeField] Slider _slider;
    [SerializeField] GameObject _nozzle;
    [SerializeField] Transform _burrelTransform;
    [SerializeField] private TankData _tankData;
    const float StartPositionY = 50f;

    public Transform BurrelTransform => _burrelTransform;
    private bool _active;
    private Damageable _damageable;
    
    private CancellationTokenSource _cts = new();
    private ReactiveProperty<float> _nextInputMoveVertical = new();
    private ReactiveProperty<float> _nextInputMoveHorizontal= new();
    private ReactiveProperty<float> _nextInputVertical= new();
    
    private float _inputMoveVertical ;
    private float _inputMoveHorizontal;
    private float _inputVertical;
    
    float _fireCoolTime = 0f;
    private float _fireTimer;
    bool _isReloaded = true;
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
        
        _fireCoolTime = _tankData.FireCoolTime;
        _slider.maxValue = _fireCoolTime;
        
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
        _fireTimer += Time.deltaTime;
        _slider.value = _fireTimer;
        if (_fireTimer > _fireCoolTime -0.5f)
        {
            if (!_isReloaded)
            {
                AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.reload);
                _isReloaded = true;
            }
        }
    }
    #region 移動関係共有メソッド
    void SendInputMove(float input) =>
        photonView.RPC(nameof(GetInputMove) , RpcTarget.AllViaServer, input);
    
    void SendInputTurn(float input) =>
        photonView.RPC(nameof(GetInputTurn) , RpcTarget.AllViaServer, input);
    
    void SendInputBarrelTurn(float input) =>
        photonView.RPC(nameof(GetInputBarrelTurn) , RpcTarget.AllViaServer, input);
    
    public void InputMove(float inputVertical)
    {
        _nextInputMoveVertical.Value = inputVertical;
    }
    public void InputTurn(float inputHorizontal)
    {
        _nextInputMoveHorizontal.Value = inputHorizontal;
    }
    public void InputBarrelTurn(float inputVertical)
    {
        _nextInputVertical.Value = inputVertical;
    }
    
    [PunRPC]
    public void GetInputMove(float inputVertical)
    {
        _inputMoveVertical = inputVertical;
    }
    [PunRPC]
    public void GetInputTurn(float inputHorizontal)
    {
        _inputMoveHorizontal = inputHorizontal;
    }
    [PunRPC]
    public void GetInputBarrelTurn(float inputVertical)
    {
        _inputVertical = inputVertical;
    }
    
    #endregion

    #region アクション関係共有のメソッド
    public void InputFire()
    {
        if (_fireTimer > _fireCoolTime)
        {
            _isReloaded = false;
            _fireTimer = 0f;
            photonView.RPC(nameof(Fire), RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void Fire()
    {
        _isReloaded = false;
        _fireTimer = 0f;
        if(PhotonNetwork.IsMasterClient)
            BulletManager.Instance.CallMadeBullet(BulletType.Normal , _nozzle.transform.position, _burrelTransform.rotation);

        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fire);
    }
    

    #endregion


    public override void OnEnable()
    {
        _damageable.OnDead += OnDead;
        MyServiceLocator.IRegister(this as IActivatable);
        MyServiceLocator.IRegister(this as IAwakeAnim);
        MyServiceLocator.IRegister(this as IPause);
    }

    public override void  OnDisable()
    {
        _damageable.OnDead -= OnDead;
        _cts?.Cancel(); 
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
        _cts = new();
        if(photonView.IsMine)
            Bind();
        _active = true;
    }
    private void Bind()
    {
        _nextInputMoveVertical.DistinctUntilChanged().Subscribe(SendInputMove).AddTo(_cts.Token);
        _nextInputMoveHorizontal.DistinctUntilChanged().Subscribe(SendInputTurn).AddTo(_cts.Token);
        _nextInputVertical.DistinctUntilChanged().Subscribe(SendInputBarrelTurn).AddTo(_cts.Token);
    }
    public void DeActive()
    {
        _cts?.Cancel();   
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