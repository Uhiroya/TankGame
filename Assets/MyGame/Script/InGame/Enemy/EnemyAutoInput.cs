using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAutoInput : MonoBehaviour
{
    [SerializeField] private TankInputSync _tankInputSync;
    [SerializeField] private SphereCollider _scanPlayerCollider;
    [SerializeField] private SphereCollider _scanFieldCollider;
    [SerializeField] private float _playerScanRadius = 20f;
    [SerializeField] private float _scanMoveRadius = 3f;
    
    private Collider _currentTargetPlayer;

    private bool _moveFlag;
    private readonly List<Collider> _playerColliderList = new();
    private float _currentScanMoveRadius;
    
    private EnemyState _state;
    private CancellationTokenSource _cts;

    private readonly ReactiveProperty<float> _barrelInput = new();
    private float _rotateBarrelByMove;
    private float _rotateBarrelBySearch;
    private bool _isNearField ;
    private void Awake()
    {
        _barrelInput
            .DistinctUntilChanged()
            .Subscribe( x => _tankInputSync.InputBarrelTurn(x));
        
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
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube (transform.position + _randomDirection * _currentScanMoveRadius ,Vector3.one * transform.lossyScale.x );
        Color c = new Color(0, 0, 0.7f, 0.2f); 
        UnityEditor.Handles.color = c;
        UnityEditor.Handles.DrawSolidDisc(transform.position , Vector3.up , _currentScanMoveRadius );
        //UnityEditor.Handles.DrawWireCube(transform.position - Vector3.back * _scanMoveRadius / 2 , new Vector3(transform.lossyScale.x ,transform.lossyScale.y , _scanMoveRadius ));
        c = new Color(1.0f, 1.0f, 0.0f, 0.05f);
        UnityEditor.Handles.color = c;
        UnityEditor.Handles.DrawSolidDisc(transform.position , Vector3.up , _playerScanRadius );
    }
#endif
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
    Vector3 _randomDirection;
    public async UniTask Move(CancellationToken cts)
    {
        RaycastHit[] hits = new RaycastHit[100];
        var previousPosition = transform.position;
        var boxCastScale = Vector3.one * transform.lossyScale.x / 2;
        
        
        _currentScanMoveRadius = _scanMoveRadius;
        var searchCount = 0;
        _randomDirection = Quaternion.Euler(0f, Random.Range(0, 360f), 0f) * transform.forward ;
        while(true)
        {
            var isMovable = true;
            
            var size = Physics.BoxCastNonAlloc(previousPosition, boxCastScale, _randomDirection, hits, Quaternion.identity, _currentScanMoveRadius);
            Debug.DrawRay(transform.position , _randomDirection * _currentScanMoveRadius , Color.magenta , 0.5f);
            if(hits.Length != 0)
            {
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
                    if (searchCount % 6 == 0)
                    {
                        _currentScanMoveRadius /= 2f;
                        _randomDirection = Quaternion.Euler(0f, Random.Range(0, 360f), 0f) * transform.forward;
                    }
                    //周りが囲まれた際、動きを止める
                    if (searchCount > 60)
                    {
                        _moveFlag = false;
                        return;
                    }
                    _randomDirection = Quaternion.Euler(0f, 60f, 0f) * _randomDirection;
                    await UniTask.Delay(100, cancellationToken: cts);
                    continue;
                }
            }

            break;
        }
        try
        {
            bool isTurn = true;
            if (IsInferiorAngle(transform.forward, _randomDirection))
            {
                _tankInputSync.InputTurn(-1.0f);
                _rotateBarrelByMove = 1.0f;
            }
            else
            {
                _tankInputSync.InputTurn(1.0f);
                _rotateBarrelByMove = -1.0f;
            }
            _tankInputSync.InputMove(1.0f);
            //ゴール地点まで動く
            while ((transform.position - previousPosition).magnitude < (_randomDirection * _currentScanMoveRadius).magnitude)
            {
                //Rayが飛ばされた向きまで履帯を回転させる
                if (isTurn && Vector3.Angle(transform.forward, _randomDirection) < 1f)
                {
                    _tankInputSync.InputTurn(0f);
                    _rotateBarrelByMove = 0f;
                    isTurn = false;
                }
                if (_isNearField)
                {
                    _isNearField = false;
                    break;
                }
                await UniTask.Yield(cts);
            }
        }
        catch
        {
            //Debug.Log("Stop");
        }
        finally
        {
            _tankInputSync.InputTurn(0f);
            _tankInputSync.InputMove(0f);
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
        var bullelTransform = _tankInputSync.BurrelTransform;
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
            _tankInputSync.InputFire();
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
        if (IsInferiorAngle(_tankInputSync.BurrelTransform.forward, transform.forward))
        {
            if (Vector3.Angle(transform.forward, _tankInputSync.BurrelTransform.forward) > 2f)
                _rotateBarrelBySearch =-1.0f;
            else
            {
                _rotateBarrelBySearch = 0f;
            }
        }
        else
        {
            if (Vector3.Angle(transform.forward, _tankInputSync.BurrelTransform.forward) > 2f)
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