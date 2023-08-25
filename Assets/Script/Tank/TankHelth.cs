using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TankHelth : MonoBehaviour
{
    [SerializeField] Slider _slider;
    [SerializeField] Image _HPImage;
    [SerializeField] int _maxHelth = 100;
    int _currentHelth;

    void OnEnable()
    {
        _currentHelth = _maxHelth;
        _slider.maxValue = _maxHelth;
        UpdateHelthUI();
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

    public void TakeDamege(int Damege)
    {
        _currentHelth -= Damege;
        UpdateHelthUI();
        if (_currentHelth <= 0)
        {
            if(transform.tag == "Player")
            {
                Destroy(gameObject);
                //ゲームオーバー処理
            }
            else if (transform.tag == "Enemy")
            {
                Destroy(gameObject);
                //何らかの処理
            }
        }
        
    }

    public void UpdateHelthUI()
    {
        _slider.value = _currentHelth;
        _HPImage.color = Color.Lerp( Color.red, Color.green, (float)_currentHelth / _maxHelth);
    }
}
