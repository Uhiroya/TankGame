using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class TankModel : MonoBehaviour
{
    GameObject _destroyEffect;
    public event Action OnDead;
    private bool _immortal;
    int _maxHP;
    int _currentHP;
    public TankModel Initialize(GameObject destroyEffect , int maxHP ,bool immortal = false)
    {
        _destroyEffect = destroyEffect;
        _maxHP = maxHP;
        _currentHP = maxHP;
        _immortal = immortal;
        return this;
    }
    public void TakeDamage(int damage)
    {
        if (_immortal) return;
        _currentHP -= damage;
        if (_currentHP <= 0)
        {
            Instantiate(_destroyEffect, transform.position , _destroyEffect.transform.rotation) ;
            AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.explotion);
            OnDead?.Invoke();
        }
    }

}
