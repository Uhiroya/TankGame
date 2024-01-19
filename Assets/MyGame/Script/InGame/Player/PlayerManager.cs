using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviour , IStart , IPause , ITankData 
{
    [SerializeField] private TankAction _action;
    [SerializeField] private TankMovement _tankMovement;
    [SerializeField] GameObject _destroyEffect;
    [SerializeField] bool _immortal = false;
    public TankData TankData;
    private TankModel _model;
    
    PlayerInputManager _playerInputManager;
    private PhotonView _photonView;
    void Awake()
    {
        _model = gameObject.AddComponent<TankModel>().Initialize(_destroyEffect, TankData.TankHP, _immortal);
        
        _playerInputManager = gameObject.AddComponent<PlayerInputManager>();
         RegisterEvent();
         _photonView = GetComponent<PhotonView>();
        _playerInputManager.enabled = false;
    }

    void RegisterEvent()
    {
        _model.OnDead += () => GameManager.Instance?.GameOver();
        _playerInputManager.OnFire += () => _action.ReadyToFire();
    }
    void OnEnable()
    {
        MyServiceLocator.IRegister(this as IPause);
        MyServiceLocator.IRegister(this as IStart);
    }

    void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IPause);
        MyServiceLocator.IUnRegister(this as IStart);
        
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
