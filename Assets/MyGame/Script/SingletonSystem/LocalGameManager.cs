using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalGameManager : MonoBehaviourPunCallbacks
{
    public static LocalGameManager Instance;
    
    [SerializeField] private float _startDelay = 3.0f;
    private IEnumerable<PlayerManager> _playerManagers;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    public async void ReadyToSceneChange(string sceneName)
    {
        await UniTask.WaitUntil(() => sceneName == SceneManager.GetActiveScene().name);
        photonView.RPC(nameof(MasterGameManager.Instance.CompleteLocalActions), RpcTarget.MasterClient,
            PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    public async void ReadyToSpawnPlayer()
    {
        await UniTask.WaitUntil(IsLoadedObjects);
        photonView.RPC(nameof(MasterGameManager.Instance.CompleteLocalActions), RpcTarget.MasterClient,
            PhotonNetwork.LocalPlayer.ActorNumber);
    }

    /// <summary>
    ///     初回起動時または、シーンチェンジ後に
    ///     Punで生成されているPlayerが共有された後OnEnabledが呼ばれた際にロード済みにして、
    ///     最大数になったら準備完了を伝える。
    /// </summary>
    private bool IsLoadedObjects()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            _playerManagers = players.ToList().Select(x => x.GetComponent<PlayerManager>());
            return true;
        }

        return false;
    }

    [PunRPC]
    public async UniTask StartTitle()
    {
        foreach (var playerManager in _playerManagers)
            playerManager.ChangeImmortal(true);
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fall);
        DeActivateObjects();
        await AnimateObjects();
        ActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Landing);
    }

    [PunRPC]
    public async UniTask StartGame()
    {
        //初期化・準備
        foreach (var playerManager in _playerManagers)
            playerManager.ChangeImmortal(false);
        DeActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fall);
        await AnimateObjects();
        //ゲームスタート
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Start);
        await SceneUIManager.Instance.ShowStartText();
        ActivateObjects();
    }

    private async UniTask AnimateObjects()
    {
        await MyServiceLocator.IResolve<IAwakeAnim>().OfType<IAwakeAnim>()
            .Select(x => x.AnimAwake(_startDelay));
    }

    public void OnPlayerDead()
    {
        photonView.RPC(nameof(MasterGameManager.Instance.OnPlayerDead), RpcTarget.MasterClient);
    }
    
    [PunRPC]
    public async UniTask RoundClear()
    {
        DeActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Sucseece);
        await SceneUIManager.Instance.ShowClearText();
    }

    [PunRPC]
    public async UniTask RoundFailed()
    {
        DeActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fail);
        await SceneUIManager.Instance.ShowGameOverText();
    }

    [PunRPC]
    public async UniTask GoNextStage(string nextStage, int lifeCount)
    {
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.SceneChange);
        _ = SceneUIManager.Instance.FadeIn();
        await SceneUIManager.Instance.FadeInStageUI(nextStage, lifeCount);

        await SceneManager.LoadSceneAsync(nextStage);

        _ = SceneUIManager.Instance.FadeOutStageUI();
        await SceneUIManager.Instance.FadeOut();
    }



    [PunRPC]
    public async UniTask BackToTitle(string titleScene, int sumBreakCount)
    {
        await SceneUIManager.Instance.ShowUpResult(sumBreakCount);
        await SceneUIManager.Instance.FadeIn();
        await SceneManager.LoadSceneAsync(titleScene);
        _ = SceneUIManager.Instance.FadeOut();
    }

    private void ActivateObjects()
    {
        MyServiceLocator.IResolve<IActivatable>().OfType<IActivatable>().ToList().ForEach(x => x.Active());
    }

    private void DeActivateObjects()
    {
        MyServiceLocator.IResolve<IActivatable>().OfType<IActivatable>().ToList().ForEach(x => x.DeActive());
    }
}