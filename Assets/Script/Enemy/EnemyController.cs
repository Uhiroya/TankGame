using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using TMPro;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float _targetSpeed = 1f;
    [SerializeField] float _enemyScanRadius = 20f;
    [SerializeField] SphereCollider _scanCollider;
    [SerializeField] float _scanMoveRange = 1.0f;
    [SerializeField] float _moveDelay = 1.0f;
    [SerializeField] bool _staticEnemy = false ;
    private TankMovement _tankMovement;
    private TankAction _tankAction;
    Vector3 _moveDir;
    CancellationTokenSource cts;
    bool _moveFlag = false;
    void Awake()
    {
        _tankMovement = GetComponent<TankMovement>();
        _tankAction = GetComponent<TankAction>();
        _scanCollider.radius = _enemyScanRadius;
    }
    void OnEnable()
    {
        cts?.Dispose();
        cts = new CancellationTokenSource();
        _moveFlag = !_staticEnemy;
        _ = AutoMover();
    }
    void OnDisable()
    {
        _moveFlag = false;
        transform.DOKill();
        cts?.Cancel();
    }
    void OnDestroy()
    {
        cts?.Dispose();
    }
    void Start()
    {

    }
    void Update()
    {
        
    }
    public async UniTask AutoMover()
    { 
        while (_moveFlag)
        {
            await Move().SuppressCancellationThrow();
            await UniTask.Delay((int)(_moveDelay * 1000));
        }
    }
    public async UniTask Move()
    {
        _moveDir = Quaternion.Euler(0f, UnityEngine.Random.Range(0,360f), 0f) * transform.forward;
        Ray ray = new Ray(transform.position, _moveDir * _scanMoveRange);
        Debug.DrawRay(transform.position, _moveDir * _scanMoveRange, Color.green , 5f);
        Physics.Raycast(ray, out RaycastHit hit, _scanMoveRange);
        try
        {
            if (hit.collider?.gameObject.tag != "Field" && hit.collider?.gameObject.tag != "Player")
            {

                while (Vector3.Angle(transform.forward, _moveDir) >= 1f)
                {
                    if(IsInferiorAngle(transform.forward , _moveDir))
                    {
                        _tankMovement?.Turn(-1.0f);
                    }
                    else
                    {
                        _tankMovement?.Turn(1.0f);
                    }
                    
                    await UniTask.Yield(cancellationToken: cts.Token);
                }
                await transform.DOMove(transform.position + _moveDir, 1f)
                       .ToUniTask(cancellationToken: cts.Token);
            }
        }
        catch
        {
            Debug.Log("キャンセル済み");
        }
    }

    public void DetectPlayer(Collider player)
    {
        Physics.Raycast(transform.position , player.transform.position - transform.position, out RaycastHit hit, _enemyScanRadius);
        Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.red);
        if (hit.collider?.gameObject.tag == "Player")
        {
            DetectedPlayer(player);
        }
        else
        {
            if(IsInferiorAngle( _tankMovement._barrelTranform.forward , transform.forward) )
            {
                if(Vector3.Angle(transform.forward, _tankMovement._barrelTranform.forward) > 2f)
                {
                    _tankMovement.BarrelTurn(-1.0f);
                }
            }
            else
            {
                if (Vector3.Angle(transform.forward, _tankMovement._barrelTranform.forward) > 2f)
                {
                    _tankMovement.BarrelTurn(1.0f);
                }
            }
        }
    }
     public void DetectedPlayer(Collider player)
    {
        //円でサーチ
        var bullelTransform = _tankMovement._barrelTranform;
        var toPlayerVec = player.gameObject.transform.position - transform.position;
        //外積を利用して最速方向に回転する
        if (IsInferiorAngle(bullelTransform.forward, toPlayerVec))
        {
            //左回り
            _targetSpeed = -Mathf.Abs(_targetSpeed);
        }
        else
        {
            //右回り
            _targetSpeed = Mathf.Abs(_targetSpeed);
        }
        var angleToPlayer = Vector3.Angle(bullelTransform.forward, toPlayerVec);
        if (angleToPlayer < 5f)
        {
            _tankAction.OnFire(true);
            if (angleToPlayer > 2f)
            {
                _tankMovement.BarrelTurn(_targetSpeed);
            }
        }
        else
        {
            _tankMovement.BarrelTurn(_targetSpeed);
            _tankAction.OnFire(false);
        }
    }
    //二つのベクトルのなす角が時計回りか反時計かを判定　trueが反時計
    public bool IsInferiorAngle(Vector3 vecA , Vector3 vecB)
    {
        if(Vector3.Cross(vecA , vecB).y < 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
