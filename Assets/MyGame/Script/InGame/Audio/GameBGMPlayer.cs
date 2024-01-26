using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


public class GameBGMPlayer : MonoBehaviour, IActivatable
{
    [SerializeField] private AudioManager.BGMSceneType _bgmType;

    private void OnEnable()
    {
        MyServiceLocator.IRegister(this as IActivatable);
    }

    private void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IActivatable);
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