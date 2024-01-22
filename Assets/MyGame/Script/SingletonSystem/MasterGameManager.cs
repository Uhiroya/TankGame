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
    private static int _currentLife;
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
        _readyFlags += 1 << (actorNumber - 1);
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
        _ = CallStartTitles();
    }
    [PunRPC]
    public async UniTaskVoid CallStartTitles()
    {
        //ステージ入場時は加算しないため1から始めている
        _currentStage = 1;
        _currentLife = _maxLife;
        _currentEnemyCount = -1;
        _sumBreakCount = 0;
        CheckReady();
        photonView.RPC(nameof(NetworkManager.Instance.SpawnPlayer) , RpcTarget.AllViaServer);
        await UniTask.WaitUntil(IsAllPlayerReady);
        photonView.RPC(nameof(LocalGameManager.Instance.StartTitle) , RpcTarget.AllViaServer);
    }
    [PunRPC]
    public async UniTask CallStartGames()
    {
        _currentPlayerCount = PhotonNetwork.CurrentRoom.MaxPlayers;
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
            CallRoundFailed();
        }
    }
    [PunRPC]
    public void OnDestroyEnemy()
    {
        _currentEnemyCount--;
        if (_currentEnemyCount == 0)
        {
            CallRoundClear();
        }
    }
    
    public async UniTaskVoid CallRoundClear()
    {
        _currentStage += 1;
        photonView.RPC(nameof(LocalGameManager.Instance.RoundClear) , RpcTarget.Others);
        await LocalGameManager.Instance.RoundClear();
        if (_currentStage < _maxStageCount)
            _ = CallChangeStages($"Stage {_currentStage}");
        else
            _ = CallBackToTitle();
    }
    public async UniTaskVoid CallRoundFailed()
    {
        _sumBreakCount -= _maxEnemyCount;
        _currentLife--;
        photonView.RPC(nameof(LocalGameManager.Instance.RoundFailed) , RpcTarget.Others);
        await LocalGameManager.Instance.RoundFailed();
        if (_currentLife > 0)
            _ = CallChangeStages($"Stage {_currentStage}");
        else 
            _ = CallBackToTitle();
    }
    public async UniTaskVoid CallChangeStages(string nextScene)
    {
        photonView.RPC(nameof(LocalGameManager.Instance.GoNextStage), 
            RpcTarget.Others, nextScene , _currentLife);
        await LocalGameManager.Instance.GoNextStage(nextScene ,_currentLife);
        _ = CallStartGames();
    }

    public async UniTaskVoid CallBackToTitle()
    {
        photonView.RPC(nameof(LocalGameManager.Instance.BackToTitle), RpcTarget.Others , _sumBreakCount);
        await LocalGameManager.Instance.BackToTitle(_sumBreakCount);
        _ = CallStartTitles();
    }



}
