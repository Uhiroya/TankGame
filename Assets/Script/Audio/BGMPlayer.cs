using System;

using UnityEngine;
using UniRx;
using UniRx.Triggers;


public class BGMPlayer : MonoBehaviour
{
    //フィールド
    [SerializeField] private AudioManager.BGMSceneType _bgmType;
    //シーン開始時
    private void Start()
    {
        //BGMを再生する
        AudioManager.Instance.PlayBGM(_bgmType);
    }
    private void OnDestroy()
    {
        AudioManager.Instance._audioBGMSource?.Stop();
    }
}
