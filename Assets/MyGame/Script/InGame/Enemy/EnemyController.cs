﻿using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private TankController _tankController;
    [SerializeField] private SphereCollider _scanPlayerCollider;
    [SerializeField] private SphereCollider _scanFieldCollider;
    [SerializeField] private float _playerScanRadius = 20f;
    [SerializeField] private float _scanMoveRadius = 3f;
    
    private Collider _currentTargetPlayer;
    private TankEnemyParam _enemyParam;

    private bool _moveFlag;
    private readonly List<Collider> _playerColliderList = new();
    private float _currentScanMoveRadius;
    
    private EnemyState _state;
    private CancellationTokenSource _cts;

    private float _rotateBarrelByMove;
    private float _rotateBarrelBySearch;
    private bool _isNearField = false;
    private void Awake()
    {
        _scanPlayerCollider.radius = _playerScanRadius;
        _scanFieldCollider.radius = _currentScanMoveRadius =  _scanMoveRadius;

        _scanPlayerCollider
            .OnTriggerEnterAsObservable()
            .Where(x => x.CompareTag("Player"))
            .Subscribe(x =>
            {
                _state = EnemyState.Detected;
                _playerColliderList.Add(x);
            })
            .AddTo(this);

        _scanPlayerCollider
            .OnTriggerExitAsObservable()
            .Where(x => x.CompareTag("Player"))
            .Subscribe(x =>
            {
                MissTarget();
                _playerColliderList.Remove(x);
                if (_playerColliderList.Count == 0) _state = EnemyState.Searching;
            })
            .AddTo(this);

        _scanFieldCollider
            .OnTriggerEnterAsObservable()
            .Where(x => x.CompareTag("Field"))
            .Subscribe(_ => _isNearField = true)
            .AddTo(this);

        _scanFieldCollider
            .OnTriggerExitAsObservable()
            .Where(x => x.CompareTag("Field"))
            .Subscribe(_ => _isNearField = false)
            .AddTo(this);
    }

    #region 自動移動

        public async UniTask AutoMover()
    {
        while (_moveFlag)
        {
            await Move().SuppressCancellationThrow();
            //await UniTask.Delay((int)(_moveDelay * 1000));
        }
    }
    public async UniTask Move()
    {
        var randomQua = Quaternion.Euler(0f, Random.Range(0, 360f), 0f);
        var moveDir = randomQua * transform.forward * _currentScanMoveRadius;
        var pos = transform.position;
        var boxCastScale = Vector3.one * transform.lossyScale.x;
        Debug.DrawRay(pos, moveDir, Color.green, 5f);
        Physics.BoxCast(pos, boxCastScale, moveDir, out var hit,  Quaternion.identity, _currentScanMoveRadius);
        try
        {
            if (hit.collider?.gameObject.tag != "Field" && hit.collider?.gameObject.tag != "Player")
            {
                _scanFieldCollider.radius = _currentScanMoveRadius = _scanMoveRadius;
                var previousPosition = transform.position;
                //ゴール地点まで動く
                while ((transform.position - previousPosition).magnitude < moveDir.magnitude)
                {
                    //Rayが飛ばされた向きまで履帯を回転させる
                    if (Vector3.Angle(transform.forward, moveDir) >= 1f)
                    {
                        if (IsInferiorAngle(transform.forward, moveDir))
                        {
                            _tankController.InputTurn(-1.0f);
                            _rotateBarrelByMove = 1.0f;
                        }
                        else
                        {
                            _tankController.InputTurn(1.0f);
                            _rotateBarrelByMove = -1.0f;
                        }
                    }
                    else
                    {
                        _tankController.InputTurn(0f);
                        _rotateBarrelByMove = 0f;
                    }

                    _tankController.InputMove(1.0f);
                    await UniTask.Yield(_cts.Token);
                    if (_isNearField)
                    {
                        _isNearField = false;
                        break;
                    }
                }
            }
            else
            {
                _currentScanMoveRadius--;
            }
        }
        catch
        {
            Debug.Log("Stop");
        }
        finally
        {
            _tankController.InputTurn(0f);
            _tankController.InputMove(0f);
        }
    }

    #endregion

    #region 索敵関係

    private void FixedUpdate()
    {
        if (!_moveFlag) return;
        switch (_state)
        {
            case EnemyState.Waiting:
                return;
            case EnemyState.Searching:
                ResetRotation();
                
                break;
            case EnemyState.Detected:
                //TODO プレイヤーのターゲットリストから現在のターゲットを一つに決めたい。
                if (!_currentTargetPlayer)
                {
                    FindTarget();
                }
                else
                {
                    ChaseTarget();
                }
                break;
        }
        _tankController.InputBarrelTurn(_rotateBarrelByMove + _rotateBarrelBySearch);
    }
    
    private void FindTarget()
    {
        foreach (var playerCollider in _playerColliderList)
        {
            if (!playerCollider) continue;
            var playerDir = playerCollider.transform.position - transform.position;
            Debug.DrawRay(transform.position, playerDir, Color.red);
            if (Physics.Raycast(transform.position, playerDir, out var hit, playerDir.magnitude))
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    _currentTargetPlayer = hit.collider;
                    break;
                }
        }
    }
    private void ChaseTarget()
    {
        var targetDir = _currentTargetPlayer.transform.position - transform.position;
        Debug.DrawRay(transform.position, targetDir, Color.red);
        if (Physics.Raycast(transform.position, targetDir, out var hit, targetDir.magnitude))
            if (hit.collider.gameObject.CompareTag("Player"))
                DetectedPlayer(_currentTargetPlayer);
            else
                MissTarget();
        else
            MissTarget();
    }


    #endregion

    private void OnEnable()
    {
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _moveFlag = true;
        _ = AutoMover();
    }

    private void OnDisable()
    {
        _moveFlag = false;
        transform.DOKill();
        _cts?.Cancel();
    }

    private void OnDestroy()
    {
        _cts?.Dispose();
    }

    private void MissTarget()
    {
        _currentTargetPlayer = null;
    }



    public void DetectedPlayer(Collider player)
    {
        //円でサーチ
        var bullelTransform = _tankController.BurrelTransform;
        var toPlayerVec = player.gameObject.transform.position - transform.position;
        //外積を利用して最速方向に回転する
        if (IsInferiorAngle(bullelTransform.forward, toPlayerVec))
            //左回り
            _rotateBarrelBySearch = -1.0f;
        else
            //右回り
            _rotateBarrelBySearch = 1.0f;
        var angleToPlayer = Vector3.Angle(bullelTransform.forward, toPlayerVec);
        if (angleToPlayer < 5f)
        {
            _tankController.InputFire();
            if (angleToPlayer < 2f) _rotateBarrelBySearch = 0f;
        }
    }

    //二つのベクトルのなす角が時計回りか反時計かを判定　trueが反時計
    public bool IsInferiorAngle(Vector3 vecA, Vector3 vecB)
    {
        if (Vector3.Cross(vecA, vecB).y < 0)
            return true;
        return false;
    }
    /// <summary>
    /// 砲身を初期位置に戻す
    /// </summary>
    private void ResetRotation()
    {
        if (IsInferiorAngle(_tankController.BurrelTransform.forward, transform.forward))
        {
            if (Vector3.Angle(transform.forward, _tankController.BurrelTransform.forward) > 2f)
                _rotateBarrelBySearch =-1.0f;
            else
            {
                _rotateBarrelBySearch = 0f;
            }
        }
        else
        {
            if (Vector3.Angle(transform.forward, _tankController.BurrelTransform.forward) > 2f)
                _rotateBarrelBySearch = 1.0f;
            else
            {
                _rotateBarrelBySearch = 0f;
            }
        }
    }

    private enum EnemyState
    {
        Waiting = 0,
        Searching = 1,
        Detected = 2
    }
}