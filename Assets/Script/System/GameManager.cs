using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using System.Linq;
public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject EnemyField = null;
    static GameManager instance;
    public static GameManager Instance => instance;
    static int _enemyCount;
    List<IStart> _takeActives = new();
    List<IPause> _takePauses = new();
    EnemyTankManager[] _enemies;
    PlayerTankManager _player ;
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    public void RoundStart()
    {
        //_player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerTankManager>();
        //_enemies = EnemyField.GetComponentsInChildren<EnemyTankManager>();
        _enemyCount = EnemyField.GetComponentsInChildren<EnemyTankManager>().Length;

        var objs = FindObjectsOfType<GameObject>();
        foreach (var obj in objs)
        {
            IStart start = obj.GetComponent<IStart>();
            start?.Active();
        }


    }
    public static void DestroyEnemy()
    {
        _enemyCount--;
        if(_enemyCount == 0)
        {
            RoundClear();
        }
    }
    public static void RoundClear()
    {
        //クリア
    }
    public static void GameOver()
    {
        //プレイヤーがやられた。
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
