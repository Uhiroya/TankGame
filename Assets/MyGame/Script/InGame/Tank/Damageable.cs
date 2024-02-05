using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Damageable : MonoBehaviour
{
    public event Action OnDead;
    public bool IsImmortal { get; set; } = false;
    int _currentHp;
    public Damageable Initialize(int maxHp)
    {
        _currentHp = maxHp;
        return this;
    }
    public void TakeDamage(int damage)
    {
        if (IsImmortal) return;
        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            OnDead?.Invoke();
        }
    }
    
}
