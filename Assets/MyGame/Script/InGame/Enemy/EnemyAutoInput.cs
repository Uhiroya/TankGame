﻿using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class EnemyAutoInput : MonoBehaviour
{
    [SerializeField] private InputReceiver _inputReceiver;
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

    private readonly ReactiveProperty<float> _barrelInput = new();
    private float _rotateBarrelByMove;
    private float _rotateBarrelBySearch;
    private bool _isNearField = false;
    private void Awake()
    {
        _barrelInput
            .DistinctUntilChanged()
            .Subscribe( x => _inputReceiver.InputBarrelTurn(x));
        
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
    private readonly TimeoutController _timeoutController = new TimeoutController();
    public async UniTask AutoMover(CancellationToken cts)
    {
        while (_moveFlag)
        {
            var timeoutController = _timeoutController.Timeout(3000);
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts ,timeoutController);
            await Move(linkedCts.Token).SuppressCancellationThrow();
            _timeoutController.Reset();
        }
    }
    public async UniTask Move(CancellationToken cts)
    {
        RaycastHit[] hits = new RaycastHit[100];
        var previousPosition = transform.position;
        var boxCastScale = Vector3.one * transform.lossyScale.x / 2;
        Vector3 randomDirection;
        
        _scanFieldCollider.radius = _currentScanMoveRadius = _scanMoveRadius;
        var searchCount = 0;
        while(true)
        {
            var isMovable = true;
            randomDirection = Quaternion.Euler(0f, Random.Range(0, 360f), 0f) * transform.forward ;
            var size = Physics.BoxCastNonAlloc(previousPosition, boxCastScale, randomDirection, hits, Quaternion.identity, _currentScanMoveRadius);
            if(hits.Length != 0)
            {
                Debug.DrawRay(previousPosition, randomDirection * _currentScanMoveRadius, Color.green, 5f);
                
                for (int i = 0; i < size; i++)
                {
                    var hit = hits[i];
                    if (hit.collider.gameObject.CompareTag("Field") || hit.collider.gameObject.CompareTag("Player"))
                    {
                        isMovable = false;
                        break;
                    }
                }

                if (!isMovable)
                {
                    searchCount++;
                    //Debug.Log(searchCount); 
                    if (searchCount % 5 == 0)
                    {
                        _currentScanMoveRadius /= 2f;
                        _scanFieldCollider.radius =　_currentScanMoveRadius;
                    }

                    continue;
                }
            }

            break;
        }
        try
        {
            bool isTurn = true;
            if (IsInferiorAngle(transform.forward, randomDirection))
            {
                _inputReceiver.InputTurn(-1.0f);
                _rotateBarrelByMove = 1.0f;
            }
            else
            {
                _inputReceiver.InputTurn(1.0f);
                _rotateBarrelByMove = -1.0f;
            }
            _inputReceiver.InputMove(1.0f);
            //ゴール地点まで動く
            while ((transform.position - previousPosition).magnitude < (randomDirection * _currentScanMoveRadius).magnitude)
            {
                //Rayが飛ばされた向きまで履帯を回転させる
                if (isTurn && Vector3.Angle(transform.forward, randomDirection) < 1f)
                {
                    _inputReceiver.InputTurn(0f);
                    _rotateBarrelByMove = 0f;
                    isTurn = false;
                }
                await UniTask.Yield(cts);
                if (_isNearField)
                {
                    _isNearField = false;
                    break;
                }
            }
        }
        catch
        {
            //Debug.Log("Stop");
        }
        finally
        {
            _inputReceiver.InputTurn(0f);
            _inputReceiver.InputMove(0f);
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

        _barrelInput.Value = _rotateBarrelByMove + _rotateBarrelBySearch;
        
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
    public void DetectedPlayer(Collider player)
    {
        //円でサーチ
        var bullelTransform = _inputReceiver.BurrelTransform;
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
            _inputReceiver.InputFire();
            if (angleToPlayer < 2f) _rotateBarrelBySearch = 0f;
        }
    }

    #endregion

    private void OnEnable()
    {
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _moveFlag = true;
        _ = AutoMover(_cts.Token);
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
        if (IsInferiorAngle(_inputReceiver.BurrelTransform.forward, transform.forward))
        {
            if (Vector3.Angle(transform.forward, _inputReceiver.BurrelTransform.forward) > 2f)
                _rotateBarrelBySearch =-1.0f;
            else
            {
                _rotateBarrelBySearch = 0f;
            }
        }
        else
        {
            if (Vector3.Angle(transform.forward, _inputReceiver.BurrelTransform.forward) > 2f)
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