using Photon.Pun;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviourPunCallbacks , IActivatable , IPause , ITankData 
{
    [SerializeField] private TankAction _action;
    [SerializeField] private TankMovement _tankMovement;
    [SerializeField] GameObject _destroyEffect;
    [SerializeField] private GameObject _minePlayerText;
    public TankData TankData;
    private TankModel _model;
    
    PlayerInputManager _playerInputManager;
    private PhotonView _photonView;
    void Awake()
    {
        _model = gameObject.AddComponent<TankModel>().Initialize(TankData.TankHP);
        _playerInputManager = gameObject.AddComponent<PlayerInputManager>();
         _photonView = GetComponent<PhotonView>();
         if(_photonView.IsMine) _minePlayerText.SetActive(true);
        _playerInputManager.enabled = false;
    }

    void RegisterEvent()
    {
        _model.OnDead += CallDestroy;
        _playerInputManager.OnFire += ReadyToFire;
    }
    void UnRegisterEvent()
    {
        _model.OnDead -= CallDestroy;
        _playerInputManager.OnFire -= ReadyToFire;
    }

    void CallDestroy()
    {
        photonView.RPC(nameof(TryDestroy), RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void ChangeImmortal(bool flag)
    {
        _model.IsImmortal = flag;
    }
    [PunRPC]
    void TryDestroy()
    {
        if (_photonView.IsMine)
        {
            LocalGameManager.Instance.OnPlayerDead();
            PhotonNetwork.Destroy(gameObject);
        }
        if (this)
        {
            Instantiate(_destroyEffect, transform.position , _destroyEffect.transform.rotation) ;
        }
    }
    void ReadyToFire() => _action.ReadyToFire();
    public override void OnEnable()
    {
        print("準備かんりょう");
        RegisterEvent();
        MyServiceLocator.IRegister(this as IPause);
        MyServiceLocator.IRegister(this as IActivatable);
    }
    public override void OnDisable()
    {
        UnRegisterEvent();
        MyServiceLocator.IUnRegister(this as IPause);
        MyServiceLocator.IUnRegister(this as IActivatable);
    }
    public TankData GetTankData() => TankData;
    public void Active()
    {
        if (_photonView.IsMine)
        {
            _playerInputManager.enabled = true;
        }
        
    }
    public void DeActive()
    {
        _playerInputManager.enabled = false;
    }
    public void Pause()
    {
        _playerInputManager.StopTankAudio();
        _playerInputManager.enabled = false;
    }

    public void Resume()
    {
        _playerInputManager.enabled = true;
    }
}
