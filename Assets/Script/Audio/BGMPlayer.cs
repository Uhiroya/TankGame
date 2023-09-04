using System;

using UnityEngine;
using UniRx;
using UniRx.Triggers;


public class BGMPlayer : MonoBehaviour
{
    //�t�B�[���h
    [SerializeField] private AudioManager.BGMSceneType _bgmType;
    //�V�[���J�n��
    private void Start()
    {
        //BGM���Đ�����
        AudioManager.Instance.PlayBGM(_bgmType);
    }
    private void OnDestroy()
    {
        AudioManager.Instance._audioBGMSource?.Stop();
    }
}
