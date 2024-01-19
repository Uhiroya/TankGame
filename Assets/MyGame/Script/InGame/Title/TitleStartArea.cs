using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TitleStartArea : MonoBehaviour
{
    [SerializeField] Slider _startSlider;
    [SerializeField] string _nextScene;
    bool _done = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.StartArea);
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            _startSlider.value += Time.deltaTime;
            if( !_done && _startSlider.value >= _startSlider.maxValue)
            {
                GameManager.Instance.GoNextStage( _nextScene );
                _done = true;
                //SceneManager.LoadScene( _nextScene );
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            AudioManager.Instance._audioSESource.Stop();
            _startSlider.value = 0f;
        }
    }
}
