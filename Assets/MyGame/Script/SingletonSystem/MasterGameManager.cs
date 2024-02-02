using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// ルームのオーナーが他参加者に通知を送るクラス。
/// </summary>
public class MasterGameManager : MonoBehaviourPunCallbacks
{
    public static MasterGameManager Instance;

    private static int _sumBreakCount;
    private static int _currentStage;
    private static int _readyFlags;
    private static int _currentPlayerCount;
    private static int _currentLife;
    [SerializeField] private int _maxMultiGamePlayers;
    [SerializeField] private int _timeOutMilliSecond;
    [SerializeField] private string _soloScene;
    [SerializeField] private string _multiScene;
    [SerializeField] private string _startStageScene;
    [SerializeField] private int _maxStageCount = 2;
    [SerializeField] private int _maxLife = 3;

    private string _titleScene;
    private int _currentEnemyCount;
    private int _maxEnemyCount;
    private int _currentMaxPlayers;
    
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
    /// <summary>
    /// OffLineModeでスタートする。
    /// </summary>
    private void Start()
    {
        JoinSoloGame();
    }

    public async void JoinMultiGame()
    {
        _titleScene = _multiScene;
        _currentMaxPlayers = _maxMultiGamePlayers;
        await NetworkManager.Instance.Connect("1.0" , _currentMaxPlayers);
        await CallLoadTitle();
        await UniTask.WaitUntil(() => PhotonNetwork.InRoom);
        Debug.Log(PhotonNetwork.IsMasterClient + "オーナです");
        if (PhotonNetwork.IsMasterClient)
        {
            _ = CallStartTitles();
        }
        
        //
        //NetWorkManager側から最大人数入室時にスタートコールされる。
    }
    [PunRPC]
    public async void JoinSoloGame()
    {
        _currentMaxPlayers = 1;
        _titleScene = _soloScene;
        await NetworkManager.Instance.Connect("1.0" , _currentMaxPlayers);
        await CallLoadTitle();
        _ = CallStartTitles();
    }

    public void LeaveRoom()
    {
        photonView.RPC(nameof(JoinSoloGame) , RpcTarget.AllViaServer);
    }
    
    public async UniTask CallLoadTitle()
    {
        photonView.RPC(nameof(LocalGameManager.Instance.LoadTitle), RpcTarget.Others, _titleScene);
        await LocalGameManager.Instance.LoadTitle(_titleScene);
        //await UniTask.WaitUntil(() => PhotonNetwork.CurrentRoom.IsOpen) ;
    }
    /// <summary>
    /// 準備完了を受け取る
    /// </summary>
    /// <param name="actorNumber"></param>
    [PunRPC]
    public void CompleteLocalActions(int actorNumber)
    {
        _readyFlags |= 1 << (actorNumber - 1);
    }
    /// <summary>
    /// 準備待機コール
    /// </summary>
    [PunRPC]
    private void CheckCompleteToSceneChange()
    {
        _readyFlags = 0;
        photonView.RPC(nameof(LocalGameManager.Instance.ReadyToSceneChange), RpcTarget.AllViaServer , SceneManager.GetActiveScene().name);
    }
    [PunRPC]
    private void CheckCompleteToSpawnPlayer()
    {
        _readyFlags = 0;
        photonView.RPC(nameof(LocalGameManager.Instance.ReadyToSpawnPlayer), RpcTarget.AllViaServer);
    }
    /// <summary>
    /// ルームの全てのプレイヤーが準備できているか判定。
    /// </summary>
    private bool IsAllPlayerReady()
    {
        for (var i = 0; i < _currentMaxPlayers; i++)
            if (((_readyFlags >> i) & 1) == 0)
                return false;
        return true;
    }

    private async UniTask SpawnPlayers(CancellationToken token)
    {
        do
        {
            CheckCompleteToSceneChange();
            int updateTime = 200;
            if(!PhotonNetwork.OfflineMode)
                UIManager.Instance.UpdateWaitingUI(_timeOutMilliSecond , updateTime);
            await UniTask.Delay(updateTime , DelayType.DeltaTime , PlayerLoopTiming.Update , token );
        } while (!IsAllPlayerReady());
        UIManager.Instance.StopWaitingUI();
        CheckCompleteToSpawnPlayer();
        photonView.RPC(nameof(NetworkManager.Instance.SpawnPlayer), RpcTarget.AllViaServer);
        await UniTask.WaitUntil(IsAllPlayerReady, PlayerLoopTiming.FixedUpdate , token);
    }

    private readonly TimeoutController _timeoutController = new TimeoutController();
    [PunRPC]
    public async UniTaskVoid CallStartTitles()
    {
        //ステージ入場時は加算しないため1から始めている
        _currentStage = 1;
        _currentLife = _maxLife;
        _currentEnemyCount = -1;
        _sumBreakCount = 0;
        
        var timeoutController = _timeoutController.Timeout(_timeOutMilliSecond);
        
        try
        {
            await SpawnPlayers(timeoutController);
        }
        catch
        {
            Debug.Log("TimeOut！！");
            photonView.RPC(nameof(JoinSoloGame), RpcTarget.All);
        }
        finally
        {
            Debug.Log("Reset！！");
            UIManager.Instance.StopWaitingUI();
            _timeoutController.Reset();
        }
        photonView.RPC(nameof(LocalGameManager.Instance.StartTitle), RpcTarget.AllViaServer);
    }
    


    [PunRPC]
    private async UniTask CallStartRound()
    {
        _currentPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        _currentEnemyCount = _maxEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        _sumBreakCount += _maxEnemyCount;
        var timeoutController = _timeoutController.Timeout(_timeOutMilliSecond);
        try
        {
            await SpawnPlayers(timeoutController);
        }
        catch
        {
            photonView.RPC(nameof(JoinSoloGame), RpcTarget.All);
            
        }
        finally
        {
            _timeoutController.Reset();
        }
        photonView.RPC(nameof(LocalGameManager.Instance.StartGame), RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void OnPlayerDead()
    {
        _currentPlayerCount--;
        if (_currentPlayerCount == 0) _ = CallRoundFailed();
    }

    [PunRPC]
    public void OnDestroyEnemy()
    {
        _currentEnemyCount--;
        Debug.Log("_currentEnemyCount : " + _currentEnemyCount);
        if (_currentEnemyCount == 0) _ = CallRoundClear();
    }

    private async UniTaskVoid CallRoundClear()
    {
        _currentStage += 1;
        photonView.RPC(nameof(LocalGameManager.Instance.RoundClear), RpcTarget.Others);
        await LocalGameManager.Instance.RoundClear();
        _ = _currentStage <= _maxStageCount ? CallChangeStages($"Stage {_currentStage}") : CallBackToTitle();
    }

    private async UniTaskVoid CallRoundFailed()
    {
        _sumBreakCount -= _maxEnemyCount;
        _currentLife--;
        photonView.RPC(nameof(LocalGameManager.Instance.RoundFailed), RpcTarget.Others);
        await LocalGameManager.Instance.RoundFailed();
        _ = _currentLife > 0 ? CallChangeStages($"Stage {_currentStage}") : CallBackToTitle();
    }

    public async UniTaskVoid CallChangeStages(string nextScene)
    {
        photonView.RPC(nameof(LocalGameManager.Instance.GoNextStage),
            RpcTarget.Others, nextScene, _currentLife);
        await LocalGameManager.Instance.GoNextStage(nextScene, _currentLife);
        _ = CallStartRound();
    }

    private async UniTaskVoid CallBackToTitle()
    {
        photonView.RPC(nameof(LocalGameManager.Instance.BackToTitle), RpcTarget.Others, _titleScene, _sumBreakCount);
        await LocalGameManager.Instance.BackToTitle(_titleScene ,_sumBreakCount);
        _ = CallStartTitles();
    }
    public void CallGameStart()
    {
        _ = CallChangeStages(_startStageScene);
    }
}