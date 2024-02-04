using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalGameManager : MonoBehaviourPunCallbacks
{
    public static LocalGameManager Instance;
    private IEnumerable<PlayerManager> _playerManagers;
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
        if (players.Length == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            _playerManagers = players.ToList().Select(x => x.GetComponent<PlayerManager>());
            return true;
        }

        return false;
    }

    [PunRPC]
    public async UniTask StartTitle(int startDelay)
    {
        foreach (var playerManager in _playerManagers)
            playerManager.ChangeImmortal(true);
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fall);
        DeActivateObjects();
        await AnimateObjects(startDelay);
        ActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Landing);
    }

    [PunRPC]
    public async UniTask StartGame(int startDelay)
    {
        //初期化・準備
        foreach (var playerManager in _playerManagers)
            playerManager.ChangeImmortal(false);
        DeActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fall);
        await AnimateObjects(startDelay);
        //ゲームスタート
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Start);
        await UIManager.Instance.ShowStartText();
        ActivateObjects();
    }

    private async UniTask AnimateObjects(float startDelay)
    {
        await MyServiceLocator.IResolve<IAwakeAnim>().OfType<IAwakeAnim>()
            .Select(x => x.AnimAwake(startDelay));
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
        await UIManager.Instance.ShowClearText();
    }

    [PunRPC]
    public async UniTask RoundFailed()
    {
        DeActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fail);
        await UIManager.Instance.ShowGameOverText();
    }

    [PunRPC]
    public async UniTask GoNextStage(string nextStage, int lifeCount)
    {
        DeActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.SceneChange);
        _ = UIManager.Instance.FadeIn();
        await UIManager.Instance.FadeInStageUI(nextStage, lifeCount);

        await SceneManager.LoadSceneAsync(nextStage);

        _ = UIManager.Instance.FadeOutStageUI();
        await UIManager.Instance.FadeOut();
    }



    [PunRPC]
    public async UniTask BackToTitle(string titleScene, int sumBreakCount)
    {
        await UIManager.Instance.ShowUpResult(sumBreakCount);
        await LoadTitle(titleScene);
    }
    [PunRPC]
    public async UniTask LoadTitle(string titleScene)
    {
        DeActivateObjects();
        await UIManager.Instance.FadeIn();
        await SceneManager.LoadSceneAsync(titleScene);
        _ = UIManager.Instance.FadeOut();
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