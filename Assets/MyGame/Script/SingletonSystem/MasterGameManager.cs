using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;
using UnityEngine.Serialization;

public class MasterGameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private int _maxStageCount = 2;
    [SerializeField] private int _maxLife = 3;
    private static int _sumBreakCount;
    private static int _currentStage ;
    private static int _readyFlags = 0;
    public static MasterGameManager Instance;
    private int _currentEnemyCount;
    private int _maxEnemyCount;
    private static int _currentPlayerCount ;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            var photon = gameObject.AddComponent<PhotonView>();
            photon.ViewID = 1;
        }
        else
        {
            Destroy(this);
        }
    }
    
    [PunRPC]
    public void GetReady(int actorNumber)
    {
        _readyFlags += 1 << (actorNumber- 1);
    }

    [PunRPC]
    public void CheckReady()
    {
        _readyFlags = 0;
        photonView.RPC(nameof(LocalGameManager.Instance.Ready) , RpcTarget.All);
    }
    public bool IsAllPlayerReady()
    {
        for (int i = 0; i < NetworkManager.Instance.MaxPlayer; i++)
        {
            if ((_readyFlags >> i & 1) == 0)
            {
                return false;
            }
        }
        return true;
    }
    [PunRPC]
    public void InitializeGame()
    {
        
        _ = StartTitles();
    }
    [PunRPC]
    public async UniTaskVoid StartTitles()
    {
        _currentStage = 1;
        _currentPlayerCount = 0;
        _currentEnemyCount = -1;
        _sumBreakCount = 0;
        CheckReady();
        photonView.RPC(nameof(NetworkManager.Instance.SpawnPlayer) , RpcTarget.AllViaServer);
        await UniTask.WaitUntil(IsAllPlayerReady);
        photonView.RPC(nameof(LocalGameManager.Instance.StartTitle) , RpcTarget.AllViaServer);
    }
    [PunRPC]
    public async UniTask StartGames()
    {
        _currentEnemyCount = _maxEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        _sumBreakCount += _maxEnemyCount;
        CheckReady();
        photonView.RPC(nameof(NetworkManager.Instance.SpawnPlayer) , RpcTarget.AllViaServer);
        await UniTask.WaitUntil(IsAllPlayerReady);
        photonView.RPC(nameof(LocalGameManager.Instance.StartGame) , RpcTarget.AllViaServer);
        
    }
    [PunRPC]
    public void OnPlayerDead()
    {
        _currentPlayerCount--;
        if (_currentPlayerCount == 0)
        {
            GameOver();
        }
    }
    [PunRPC]
    public void OnDestroyEnemy()
    {
        _currentEnemyCount--;
        if (_currentEnemyCount == 0)
        {
            RoundClear();
        }
    }
    
    public async void RoundClear()
    {
        _currentStage += 1;
        photonView.RPC(nameof(LocalGameManager.Instance.RoundClear) , RpcTarget.AllViaServer);
        
        if (_currentStage < _maxStageCount)
            ChangeStages($"Stage {_currentStage}");
        else
            BackToTitles();
    }

    public void ChangeStages(string nextScene)
    {
        photonView.RPC(nameof(LocalGameManager.Instance.GoNextStage), RpcTarget.AllViaServer,nextScene);
    }

    public void BackToTitles()
    {
        photonView.RPC(nameof(LocalGameManager.Instance.BackToTitle), RpcTarget.AllViaServer , _sumBreakCount);
    }

    public async void GameOver()
    {
        _sumBreakCount -= _maxEnemyCount;
        BackToTitles();
        
    }

}
