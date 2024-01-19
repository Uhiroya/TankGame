using System;

using UnityEngine;

using UniRx;
using UniRx.Triggers;


public class GameBGMPlayer : MonoBehaviour , IStart
{
    [SerializeField] private AudioManager.BGMSceneType _bgmType;

    void OnEnable()
    {
        MyServiceLocator.IRegister(this as IStart);
    }

    void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IStart);
    }
        
    public void Active()
    {
        AudioManager.Instance.PlayBGM(_bgmType);
    }
    public void DeActive()
    {
        AudioManager.Instance._audioBGMSource?.Stop();
    }
}
