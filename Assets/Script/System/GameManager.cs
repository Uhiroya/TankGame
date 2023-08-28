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
    static GameManager instance;
    public static GameManager Instance => instance;
    static int _enemyCount;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    async void Start()
    {
        InActiveObjects();
        await UniTask.Delay(3000);
        RoundStart();
    }
    public void RoundStart()
    {
        _enemyCount = EnemyField.GetComponentsInChildren<EnemyTankManager>().Length;
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
    public static void RoundClear()
    {
        //クリア
        InActiveObjects();
        SceneManager.LoadScene("Title");
    }
    public static void GameOver()
    {
        //プレイヤーがやられた。
        SceneManager.LoadScene("Title");
    }
    public static void ActiveObjects()
    {
        MyServiceLocator.IResolve<IStart>().OfType<IStart>().ToList().ForEach(x => x.Active());
    }
    public static void InActiveObjects()
    {
        MyServiceLocator.IResolve<IStart>().OfType<IStart>().ToList().ForEach(x => x.InActive());
    }
}
