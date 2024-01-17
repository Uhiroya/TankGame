using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// シングルトン、ゲームマネージャー
    /// </summary>
    static GameManager instance;
    [SerializeField] float StartDelay = 3.0f;
    [SerializeField] int StageCount = 2;
    [SerializeField] int PlayerCount = 3;
    static int _nowStage = 1;
    static int _breakEnemyCount = 0;
    static int _enemyCount = -1;
    int _nowEnemyCount = 0;
    public static int NowPlayerCount { get; private set; }
    public static GameManager Instance => instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public async UniTask TitleInitialize()
    {
        NowPlayerCount = PlayerCount;
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fall);
        await Initialize();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Landing);
        ActiveObjects();
    }
    public async UniTask GameInitialize()
    {
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.SceneChange);
        await Initialize();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Start);
        await SceneUIManager.Instance.StartUI();
        RoundStart();
    }
    public async UniTask Initialize()
    {
        _nowEnemyCount = -1;
        InActiveObjects();
        //await UniTask.Delay((int)StartDelay * 1000);
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
        _enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        _nowEnemyCount = _enemyCount;
        print($"EnemyCount : {_nowEnemyCount}");
        _breakEnemyCount += _enemyCount;
        ActiveObjects();
    }
    public void DestroyEnemy()
    {
        _nowEnemyCount--;
        if (_nowEnemyCount == 0)
        {
            RoundClear();
        }
    }
    public async void RoundClear()
    {
        //クリア
        InActiveObjects();
        _nowStage += 1;
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Sucseece);
        await SceneUIManager.Instance.ClearUI();
        if(_nowStage <= StageCount)
        {
            SceneUIManager.Instance?.FadeAndNextStage($"Stage {_nowStage}");
        }
        else
        {
            BackToTitle();
        }
        
    }
    public async void GameOver()
    {
        //プレイヤーがやられた。
        _breakEnemyCount -= _enemyCount ;
        InActiveObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fail);
        await SceneUIManager.Instance.GameOverUI();
        NowPlayerCount--;
        if(NowPlayerCount == 0)
        {
            BackToTitle();
        }
        else
        {
            SceneUIManager.Instance?.FadeAndNextStage($"Stage {_nowStage}");
        }
    }
    public async void BackToTitle()
    {
        _nowStage = 1;
        await SceneUIManager.Instance.ShowUpResult(_breakEnemyCount);
        _breakEnemyCount = 0;
        SceneUIManager.Instance?.FadeAndNextScene();
    }
    public void ActiveObjects()
    {
        MyServiceLocator.IResolve<IStart>().OfType<IStart>().ToList().ForEach(x => x.Active());
    }
    public void InActiveObjects()
    {
        MyServiceLocator.IResolve<IStart>().OfType<IStart>().ToList().ForEach(x => x.InActive());
    }
    public void Debug()
    {
        NowPlayerCount = 99;
    }
}
