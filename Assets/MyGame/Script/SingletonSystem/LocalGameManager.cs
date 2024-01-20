using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalGameManager : MonoBehaviourPunCallbacks
{
    public static bool IsReady = false;
    private int _loadedPlayer = 0;
    public static LocalGameManager Instance;

    [SerializeField] private string _titleScene;
    [SerializeField] private float StartDelay = 3.0f;

    
    public static int CurrentLifeCount { get; private set; }

    [PunRPC]
    public void Ready()
    {
        IsReady = true;
        _loadedPlayer = 0;
    }

    [PunRPC]
    public void Wait()
    {
        IsReady = false;
    }

    
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
    public async UniTask StartTitle()
    {
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
        DeActivateObjects();
        await AnimateObjects();
        //ゲームスタート
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Start);
        await SceneUIManager.Instance.ShowStartText();
        ActivateObjects();
    }

    private async UniTask AnimateObjects()
    {
        await MyServiceLocator.IResolve<IAnimAwake>().OfType<IAnimAwake>()
                .Select(x => x.AnimAwake(StartDelay));
    }


    
    public void OnPlayerLoaded()
    {
        _loadedPlayer++;
        if (_loadedPlayer == NetworkManager.Instance.MaxPlayer)
        {
            photonView.RPC(nameof(MasterGameManager.Instance.CheckReady) , RpcTarget.MasterClient , PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    public void OnPlayerDead()
    {
        photonView.RPC(nameof(MasterGameManager.Instance.OnPlayerDead),RpcTarget.MasterClient);
    }

    [PunRPC]
    public async void RoundClear()
    {
        DeActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Sucseece);
        await SceneUIManager.Instance.ShowClearText();
    }
    [PunRPC]
    public async void GoNextStage(string nextStage)
    {
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.SceneChange);
        _ = SceneUIManager.Instance.FadeIn();
        await SceneUIManager.Instance.FadeInStageUI(nextStage);
        
        await SceneManager.LoadSceneAsync(nextStage);
        
        _ = SceneUIManager.Instance.FadeOutStageUI();
        await SceneUIManager.Instance.FadeOut();
        
        if (PhotonNetwork.IsMasterClient)
        {
            MasterGameManager.Instance.StartGames();
        }
    }
    [PunRPC]
    public async void GameOver()
    {
        DeActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fail);
        await SceneUIManager.Instance.ShowGameOverText();
    }
    [PunRPC]
    public async void BackToTitle(int sumBreakCount)
    {
        await SceneUIManager.Instance.ShowUpResult(sumBreakCount);
        await SceneUIManager.Instance.FadeIn();
        await SceneManager.LoadSceneAsync(_titleScene);
        _ = SceneUIManager.Instance.FadeOut();
        if (PhotonNetwork.IsMasterClient)
        {
            MasterGameManager.Instance.StartTitles();
        }
    }

    public void ActivateObjects()
    {
        MyServiceLocator.IResolve<IStart>().OfType<IStart>().ToList().ForEach(x => x.Active());
    }

    public void DeActivateObjects()
    {
        MyServiceLocator.IResolve<IStart>().OfType<IStart>().ToList().ForEach(x => x.DeActive());
    }
}