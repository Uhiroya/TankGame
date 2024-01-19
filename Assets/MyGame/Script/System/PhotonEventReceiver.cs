using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;    // IOnEventCallback を使うため
using ExitGames.Client.Photon;  // EventData を使うため
using Photon.Pun;   // PhotonNetwork を使うため
using System;

/// <summary>
/// イベントを受け取るコンポーネント（パターン A）
/// やっていること：
/// 1. MonoBehaviour の代わりに MonoBehaviourPunCallbacks クラスを継承する
/// 2. IOnEventCallback インターフェイスを継承し、IOnEventCallback.OnEvent(EventData) を実装する
/// 3. イベントが Raise されると OnEvent メソッドが呼ばれるので、呼ばれた時の処理を実装する
/// </summary>
public class PhotonEventReceiver : MonoBehaviourPunCallbacks, IOnEventCallback
{
    /// <summary>
    /// イベントが Raise されると呼ばれる
    /// </summary>
    /// <param name="e">イベントデータ</param>
    void IOnEventCallback.OnEvent(EventData eventData)
    {
        switch (eventData.Code)
        {
            case 1://GameStart
                GameManager.Instance.StartGame();
                break;
        }
    }
}