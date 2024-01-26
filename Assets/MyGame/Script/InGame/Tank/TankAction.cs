using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using MyGame.Script.SingletonSystem;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class TankAction : MonoBehaviourPunCallbacks ,IPause
{
    public BulletType BulletType;
    [SerializeField] GameObject _nozzle;
    [SerializeField] Transform _burrelTransform;
    [SerializeField] Slider _slider;
    //[SerializeField] Image FIreTimeImage;
    public bool _isHitNozzle = false;
    float _fireCoolTime = 0f;
    private float _fireTimer;
    private float _pauseTimer;
    bool _isReloaded = true;
    bool _isPause = false;
    void Awake()
    {
        _fireCoolTime = GetComponent<ITankData>().GetTankData().FireCoolTime;
        _slider.maxValue = _fireCoolTime;
    }

    public override void OnEnable()
    {
        MyServiceLocator.IRegister(this as IPause);
    }

    public override void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IPause);
    }
    void Update()
    {
        if(!_isPause)
        {
            _fireTimer += Time.deltaTime;
        }
        
        UpdateReloadUI();
        if (_fireTimer > _fireCoolTime -0.5f)
        {
            if (!_isReloaded)
            {
                AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.reload);
                _isReloaded = true;
            }
        }
    }
    public void ReadyToFire()
    {
        if (_fireTimer > _fireCoolTime && !_isHitNozzle)
        {
            _isReloaded = false;
            _fireTimer = 0f;
            photonView.RPC(nameof(Fire) , RpcTarget.AllViaServer);
    }
    }
    [PunRPC]
    public void Fire()
    {
        _isReloaded = false;
        _fireTimer = 0f;
        if(PhotonNetwork.IsMasterClient)
            BulletManager.Instance.CallMadeBullet(BulletType, _nozzle.transform.position, _burrelTransform.rotation);

        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fire);
    }
    public void UpdateReloadUI()
    {
        _slider.value = _fireTimer;
        //FIreTimeImage.color = Color.Lerp(Color.red, Color.green, _fireCoolTime/ _fireTimer);
    }
    public void Pause()
    {
        _isPause = true;
        _pauseTimer = _fireTimer;
    }

    public void Resume()
    {
        _isPause = false;
        _fireTimer = _pauseTimer;
    }

    public void HitNozzleToField()
    {
        _isHitNozzle = true;
    }
    public void OutNozzleToField()
    {
        _isHitNozzle = false;
    }
}
