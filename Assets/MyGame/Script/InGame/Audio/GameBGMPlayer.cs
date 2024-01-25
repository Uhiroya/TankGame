using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


public class GameBGMPlayer : MonoBehaviour, IStart
{
    [SerializeField] private AudioManager.BGMSceneType _bgmType;

    private void OnEnable()
    {
        MyServiceLocator.IRegister(this as IStart);
    }

    private void OnDisable()
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