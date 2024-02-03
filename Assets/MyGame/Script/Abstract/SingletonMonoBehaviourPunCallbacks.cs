using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
/// <summary>
/// ToDo なぜ使えないのか聞く
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMonoBehaviourPunCallbacks<T> : MonoBehaviourPunCallbacks 
    where T : Component
{
    public static T Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            OnAwake();
        }
        else
        {
            Destroy(this);
        }

    }

    protected virtual void OnAwake()
    {
        
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
