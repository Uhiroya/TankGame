using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// シングルトン、ゲームマネージャー
    /// </summary>
    static GameManager instance;
    [SerializeField] float StartDelay = 3.0f;
    [SerializeField] int StageCount = 2; 
    [SerializeField] int LifeCount = 3;
    static int _currentStage = 1;
    static int _sumBreakCount = 0;
    static int _maxEnemyCount = -1;
    int _currentEnemyCount = 0;
    public static int CurrentLifeCount { get; private set; }
    public static bool IsNetworking = false;
    public static GameManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }
    [PunRPC]
    public async UniTask StartTitle()
    {
        CurrentLifeCount = LifeCount;
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fall);
        await Initialize();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Landing);
        ActivateObjects();
    }
    [PunRPC]
    public async UniTask StartGame()
    {
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.SceneChange);
        await Initialize();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Start);
        await SceneUIManager.Instance.ShowStartText();
        RoundStart();
    }
    public async UniTask Initialize()
    {
        _currentEnemyCount = -1;
        DeActivateObjects();
        await InitializeObjects();
    }
    async UniTask InitializeObjects()
    {
        var objs = MyServiceLocator.IResolve<IAnimAwake>().OfType<IAnimAwake>().ToList();
        List<UniTask> tasks = new List<UniTask>();
        foreach (var obj in objs)
        {
            tasks.Add(obj.AnimAwake(StartDelay));
        }
        await UniTask.WhenAll(tasks);
    }
    public void RoundStart()
    {
        _maxEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        _currentEnemyCount = _maxEnemyCount;
        _sumBreakCount += _maxEnemyCount;
        ActivateObjects();
    }
    public void DestroyEnemy()
    {
        _currentEnemyCount--;
        if (_currentEnemyCount == 0)
        {
            RoundClear();
        }
    }
    public async void RoundClear()
    {
        //クリア
        DeActivateObjects();
        _currentStage += 1;
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Sucseece);
        await SceneUIManager.Instance.ShowClearText();
        if(_currentStage <= StageCount)
        {
            await GoNextStage($"Stage {_currentStage}");
        }
        else
        {
            BackToTitle();
        }
        
    }

    public async UniTask GoNextStage(string nextStage)
    {
        _ = SceneUIManager.Instance.FadeIn();
        await SceneUIManager.Instance.FadeInStageUI(nextStage);
        await SceneManager.LoadSceneAsync(nextStage);
        _ = SceneUIManager.Instance.FadeOutStageUI();
        await SceneUIManager.Instance.FadeOut();
    }
    public async void GameOver()
    {
        _sumBreakCount -= _maxEnemyCount ;
        DeActivateObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fail);
        await SceneUIManager.Instance.ShowGameOverText();
        CurrentLifeCount--;
        if(CurrentLifeCount == 0)
        {
            BackToTitle();
        }
        else
        {
            await GoNextStage($"Stage {_currentStage}");
        }
    }
    public async void BackToTitle()
    {
        _currentStage = 1;
        await SceneUIManager.Instance.ShowUpResult(_sumBreakCount);
        _sumBreakCount = 0;
        SceneUIManager.Instance?.FadeIn();
    }
    public void ActivateObjects()
    {
        MyServiceLocator.IResolve<IStart>().OfType<IStart>().ToList().ForEach(x => x.Active());
    }
    public void DeActivateObjects()
    {
        MyServiceLocator.IResolve<IStart>().OfType<IStart>().ToList().ForEach(x => x.DeActive());
    }
    public void Debug()
    {
        CurrentLifeCount = 99;
    }
}
