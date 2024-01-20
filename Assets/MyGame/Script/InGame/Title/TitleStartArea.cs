using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class TitleStartArea : MonoBehaviour , IStart , IPause
{
    [SerializeField] private Collider _startCollider;
    [SerializeField] private Slider _startSlider;
    [SerializeField] private string _nextScene;
    private bool _done;
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
    public void Active()
    {
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
            _startSlider.value += Time.deltaTime;
            if (!_done && _startSlider.value >= _startSlider.maxValue)
            {
                if(PhotonNetwork.IsMasterClient)
                    MasterGameManager.Instance.ChangeStages(_nextScene);
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