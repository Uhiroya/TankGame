using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject EnemyField = null;
    [SerializeField] float StartDelay = 3.0f;
    static GameManager instance;
    public static GameManager Instance => instance;
    public static bool ActivFlag = false;
    static int _enemyCount = -1;
    [SerializeField] static int PlayerCount = 3;
    public static int NowPlayerCount;
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
        _enemyCount = -1;
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
        EnemyField = GameObject.FindWithTag("EnemyField");
        if(EnemyField != null)
        {
            _enemyCount = EnemyField.GetComponentsInChildren<EnemyTankManager>().Length;
        }
        ActiveObjects();
    }
    public void DestroyEnemy()
    {
        _enemyCount--;
        if (_enemyCount == 0)
        {
            RoundClear();
        }
    }
    public async void RoundClear()
    {
        //クリア
        InActiveObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Sucseece);
        await SceneUIManager.Instance.ClearUI();
        SceneUIManager.Instance?.FadeAndNextStage("Stage 1");
    }

    public async void GameOver()
    {
        //プレイヤーがやられた。
        InActiveObjects();
        AudioManager.Instance.PlaySE(AudioManager.TankGameSoundType.Fail);
        await SceneUIManager.Instance.GameOverUI();
        NowPlayerCount--;
        if(NowPlayerCount == 0)
        {
            SceneUIManager.Instance?.FadeAndNextScene("Title");
        }
        else
        {
            SceneUIManager.Instance?.FadeAndNextStage("Stage 1");
        }
        
    }
    public void ActiveObjects()
    {
        MyServiceLocator.IResolve<IStart>().OfType<IStart>().ToList().ForEach(x => x.Active());
    }
    public void InActiveObjects()
    {
        MyServiceLocator.IResolve<IStart>().OfType<IStart>().ToList().ForEach(x => x.InActive());
    }
}
