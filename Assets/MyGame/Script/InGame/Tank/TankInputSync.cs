using System.Threading;
using Cysharp.Threading.Tasks;
using MyGame.Script.SingletonSystem;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TankInputSync : MonoBehaviourPunCallbacks, IActivatable
{
    [SerializeField] private Slider _slider;
    [SerializeField] private GameObject _nozzle;
    private readonly ReactiveProperty<float> _nextInputMoveHorizontal = new();
    private readonly ReactiveProperty<float> _nextInputMoveVertical = new();
    private readonly ReactiveProperty<float> _nextInputVertical = new();
    private float _fireTimer;
    private bool _isReloaded = true;
    private TankController _tankController;
    public TankData TankData => _tankController.TankData;
    public Transform BurrelTransform => _tankController.BurrelTransform;

    private void Awake()
    {
        _tankController = GetComponent<TankController>();
        _slider.maxValue = TankData.FireCoolTime;
    }
    #region 共通呼び出し
    private void FixedUpdate()
    {
        _fireTimer += Time.deltaTime;
        _slider.value = _fireTimer;
        if (_fireTimer > TankData.FireCoolTime - 0.5f)
            if (!_isReloaded)
            {
                AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.reload);
                _isReloaded = true;
            }
    }
    public void InputFire()
    {
        if (_fireTimer > TankData.FireCoolTime)
        {
            _isReloaded = false;
            _fireTimer = 0f;
            photonView.RPC(nameof(SendInputFire) , RpcTarget.MasterClient);
        }
    }
    [PunRPC]
    public void GetInputMove(float inputVertical, Vector3 position)
    {
        //transform.position = position;
        _tankController.InputMove(inputVertical, position);
    }

    [PunRPC]
    public void GetInputTurn(float inputHorizontal, Quaternion rotation)
    {
        //transform.rotation = rotation;
        _tankController.InputTurn(inputHorizontal,  rotation);
    }

    [PunRPC]
    public void GetInputBarrelTurn(float inputVertical, Quaternion rotation)
    {
        //_burrelTransform.rotation = rotation;
        _tankController.InputBarrelTurn(inputVertical,  rotation);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        MyServiceLocator.IRegister(this as IActivatable);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        _cts?.Cancel();
        MyServiceLocator.IUnRegister(this as IActivatable);
    }
    #endregion
    
    #region MasterClassのみでの呼び出し
    
    
    [PunRPC]
    public void SendInputFire()
    {
        BulletsFireSync.Instance.CallMadeBullet(TankData.BulletType, _nozzle.transform.position, _tankController.BurrelTransform.rotation);
    }
    private void SendInputMove(float input)
    {
        photonView.RPC(nameof(GetInputMove), RpcTarget.AllViaServer, input, transform.position);
    }

    private void SendInputTurn(float input)
    {
        photonView.RPC(nameof(GetInputTurn), RpcTarget.AllViaServer, input, transform.rotation);
    }

    private void SendInputBarrelTurn(float input)
    {
        photonView.RPC(nameof(GetInputBarrelTurn), RpcTarget.AllViaServer, input,
            BurrelTransform.rotation);
    }

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

    private CancellationTokenSource _cts;

    private void Bind()
    {
        _nextInputMoveVertical.DistinctUntilChanged().Subscribe(SendInputMove).AddTo(_cts.Token);
        _nextInputMoveHorizontal.DistinctUntilChanged().Subscribe(SendInputTurn).AddTo(_cts.Token);
        _nextInputVertical.DistinctUntilChanged().Subscribe(SendInputBarrelTurn).AddTo(_cts.Token);
    }

    public void Active()
    {
        _cts = new CancellationTokenSource();
        Bind();
    }

    public void DeActive()
    {
        _cts?.Cancel();
    }
    
    #endregion
    

}