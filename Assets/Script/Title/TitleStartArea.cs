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
    void Awake()
    {
        
    }

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            _startSlider.value += Time.deltaTime;
            if( _startSlider.value >= _startSlider.maxValue)
            {
                SceneManager.LoadScene( _nextScene );
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            _startSlider.value = 0f;
        }
    }
}
