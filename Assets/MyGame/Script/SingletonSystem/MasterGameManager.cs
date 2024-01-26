using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterGameManager : MonoBehaviourPunCallbacks
{
    public static MasterGameManager Instance;

    private static int _sumBreakCount;
    private static int _currentStage;
    private static int _readyFlags;
    private static int _currentPlayerCount;
    private static int _currentLife;
    [SerializeField] private int _maxStageCount = 2;
    [SerializeField] private int _maxLife = 3;

    private int _currentEnemyCount;
    private int _maxEnemyCount;

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
        _readyFlags |= 1 << (actorNumber - 1);
    }

    [PunRPC]
    private void CheckReady()
    {
        _readyFlags = 0;
        photonView.RPC(nameof(LocalGameManager.Instance.Ready), RpcTarget.AllViaServer , SceneManager.GetActiveScene().name);
    }

    private bool IsAllPlayerReady()
    {
        for (var i = 0; i < NetworkManager.Instance.MaxPlayer; i++)
            if (((_readyFlags >> i) & 1) == 0)
                return false;
        return true;
    }

    private async UniTask SpawnPlayers()
    {
        CheckReady();
        photonView.RPC(nameof(NetworkManager.Instance.SpawnPlayer), RpcTarget.AllBufferedViaServer);
        await UniTask.WaitUntil(IsAllPlayerReady);
    }

    [PunRPC]
    public async UniTaskVoid CallStartTitles()
    {
        //ステージ入場時は加算しないため1から始めている
        _currentStage = 1;
        _currentLife = _maxLife;
        _currentEnemyCount = -1;
        _sumBreakCount = 0;

        await SpawnPlayers();
        photonView.RPC(nameof(LocalGameManager.Instance.StartTitle), RpcTarget.AllViaServer);
    }

    [PunRPC]
    private async UniTask CallStartGames()
    {
        _currentPlayerCount = PhotonNetwork.CurrentRoom.MaxPlayers;
        _currentEnemyCount = _maxEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        _sumBreakCount += _maxEnemyCount;

        await SpawnPlayers();
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
        _ = CallStartGames();
    }

    private async UniTaskVoid CallBackToTitle()
    {
        photonView.RPC(nameof(LocalGameManager.Instance.BackToTitle), RpcTarget.Others, _sumBreakCount);
        await LocalGameManager.Instance.BackToTitle(_sumBreakCount);
        _ = CallStartTitles();
    }
}