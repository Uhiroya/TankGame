using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class TitleStartArea : MonoBehaviour , IActivatable , IPause
{
    [SerializeField] private StartAreaSetting _areaSetting;
    [SerializeField] private Collider _startCollider;
    [SerializeField] private Slider _startSlider;

    public enum StartAreaSetting
    {
        StartGame,
        MultiMode,
        LeaveRoom,
    }
    private static bool _done;
    void OnEnable()
    {
        MyServiceLocator.IRegister(this as IPause);
        MyServiceLocator.IRegister(this as IActivatable);
    }
    void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IPause);
        MyServiceLocator.IUnRegister(this as IActivatable);
    }
    public void Active()
    {
        _done = false;
        _startCollider.enabled = true;
    }
    public void DeActive()
    {
        _startCollider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.StartArea);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            AudioManager.Instance._audioSESource.Stop();
            _startSlider.value = 0f;

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_done) return;
            _startSlider.value += Time.deltaTime;
            if (_startSlider.value >= _startSlider.maxValue)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    switch (_areaSetting)
                    {
                        case StartAreaSetting.StartGame:
                            MasterGameManager.Instance.CallGameStart();
                            break;
                        case StartAreaSetting.MultiMode:
                            MasterGameManager.Instance.JoinMultiGame();
                            break;
                        case StartAreaSetting.LeaveRoom:
                            MasterGameManager.Instance.LeaveRoom();
                            break;
                    }
                }
                _done = true;
                //SceneManager.LoadScene( _nextScene );
            }
        }
    } 
    
    public void Pause()
    {
        _startCollider.enabled = false;
    }

    public void Resume()
    {
        _startCollider.enabled = true;
    }
}