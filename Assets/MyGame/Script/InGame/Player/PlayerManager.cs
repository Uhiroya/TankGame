using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks, IActivatable, IPause
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private GameObject _destroyEffect;
    [SerializeField] private GameObject _meText;
    [SerializeField] private TankController _tankController;

    private void Awake()
    {
       
        if (photonView.IsMine) _meText.SetActive(true);
        _playerInput.enabled = false;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        _tankController.DeadEvent += CallDestroy;
        MyServiceLocator.IRegister(this as IPause);
        MyServiceLocator.IRegister(this as IActivatable);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        _tankController.DeadEvent -= CallDestroy;
        MyServiceLocator.IUnRegister(this as IPause);
        MyServiceLocator.IUnRegister(this as IActivatable);
    }

    public void Active()
    {
        if (photonView.IsMine) _playerInput.enabled = true;
    }

    public void DeActive()
    {
        _playerInput.StopTankAudio();
        _playerInput.enabled = false;
    }

    public void Pause()
    {
        _playerInput.StopTankAudio();
        _playerInput.enabled = false;
    }

    public void Resume()
    {
        _playerInput.enabled = true;
    }
    
    private void CallDestroy()
    {
        photonView.RPC(nameof(TryDestroy), RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void ChangeImmortal(bool flag)
    {
        _tankController.ChangeImmortal(flag);
    }

    [PunRPC]
    private void TryDestroy()
    {
        if (this)
        {
            AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.explotion);
            Instantiate(_destroyEffect, transform.position, _destroyEffect.transform.rotation);
        }
        if (photonView.IsMine)
        {
            LocalGameManager.Instance.OnPlayerDead();
            PhotonNetwork.Destroy(gameObject);
        }
    }
}