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
    public bool IsImmortal { get; set; } = false;
    int _maxHP;
    int _currentHP;
    public TankModel Initialize(int maxHP)
    {
        _maxHP = maxHP;
        _currentHP = maxHP;
        return this;
    }
    public void TakeDamage(int damage)
    {
        if (IsImmortal) return;
        _currentHP -= damage;
        if (_currentHP <= 0)
        {
            AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.explotion);
            OnDead?.Invoke();
        }
    }
    
}
