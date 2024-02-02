using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public static class PhotonPlayerPropertiesExtensions
{
    //public static event 
    private const string Ready = "Ready";
    private const string CurrentPlayerCount = "CurrentPlayerCount";
    private const string MessageKey = "Message";

    private static readonly Hashtable _propertyToSet = new Hashtable();

    public static void ReadyToGo()
    {
        int currentReadyFlag = (PhotonNetwork.LocalPlayer.CustomProperties[Ready] is int value) ? value : 0 ;
        int readyFlag = 1 << (PhotonNetwork.LocalPlayer.ActorNumber - 1);
        _propertyToSet[Ready] = currentReadyFlag + readyFlag ;
        PhotonNetwork.LocalPlayer.SetCustomProperties(_propertyToSet);
        _propertyToSet.Clear();
    }
    public static void ResetReady()
    {
        Debug.Log("roomPropertyをリセットしました");
        _propertyToSet[Ready] = 0 ;
        PhotonNetwork.LocalPlayer.SetCustomProperties(_propertyToSet);
        _propertyToSet.Clear();
    }

    public static bool IsReady(int maxPlayer)
    {
        int readyFlags = (PhotonNetwork.LocalPlayer.CustomProperties[Ready] is int value) ? value : 0 ;
        for (int i = 0; i < maxPlayer; i++)
        {
            Debug.Log($"判定中{readyFlags}");
            if ((readyFlags >> i & 1) == 0)
            {
                return false;
            } 
        }

        return true;
    }

}