using System.Collections;
using System.Collections.Generic;
using MyGame.Script.SingletonSystem;
using Photon.Pun;
using UnityEngine;
/// <summary>
/// Masterのみで動かす
/// </summary>
public class BulletsFireSync : MonoBehaviourPunCallbacks , IActivatable
{
    public static BulletsFireSync Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

    }
    private static int _bulletID = 0;
    
    [PunRPC]
    public void CallReleaseBullet(BulletType bulletType ,int bulletID)
    {
         photonView.RPC(nameof(BulletsManager.Instance.ReleaseBullet), RpcTarget.AllViaServer, bulletType, bulletID);
    }
    
    [PunRPC]
    public void CallMadeBullet(BulletType bulletType , Vector3 position , Quaternion rotation )
    {
        _bulletID += 1;
        //Debug.Log("CallMadeBullet" + _bulletID);
        photonView.RPC(nameof(BulletsManager.Instance.MadeBullet), RpcTarget.AllViaServer , bulletType,position, rotation , _bulletID);
    }

    public void Active()
    {
        _bulletID = 0;
    }

    public void DeActive()
    {
        _bulletID = 0;
    }
}
