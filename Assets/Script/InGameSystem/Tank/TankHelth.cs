using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class TankHelth : MonoBehaviour
{
    [SerializeField] GameObject _destroyEffect;

    [SerializeField] bool _immortal = false;
    int _maxHelth;
    int _currentHelth;
    void Awake()
    {
        _maxHelth = GetComponent<ITankData>().GetTankData().TankHP;
        _currentHelth = _maxHelth;
    }
    public void TakeDamege(int Damege)
    {
        if (!_immortal)
        {
            _currentHelth -= Damege;
            if (_currentHelth <= 0)
            {
                Instantiate(_destroyEffect, transform.position , _destroyEffect.transform.rotation) ;
                AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.explotion);
                if (transform.tag == "Player")
                {
                    GameManager.Instance?.GameOver();
                    //ゲームオーバー処理
                }
                else if (transform.tag == "Enemy")
                {
                    GameManager.Instance?.DestroyEnemy();
                    Destroy(gameObject);
                    //何らかの処理
                }
            }
        }

    }


}
