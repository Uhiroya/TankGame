using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using UniRx.Triggers;

public class EnemyController : MonoBehaviour
{
    [SerializeField] SphereCollider _scanPlayerCollider;
    [SerializeField] SphereCollider _scanFieldCollider;
    [SerializeField] float _scanFieldRange = 3f;

    private enum EnemyState
    {
        Waiting = 0,
        Searching = 1,
        Detected = 2,
    }

    private EnemyState _state;
    TankEnemyParam _enemyParam;
    TankMovement _tankMovement;
    TankAction _tankAction;
    Vector3 _moveDir;
    CancellationTokenSource cts;
    bool _moveFlag = false;
    bool _isNearField = false;
    private Collider _playerCollider;
    void Awake()
    {
        _tankMovement = GetComponent<TankMovement>();
        _tankAction = GetComponent<TankAction>();
        _enemyParam = GetComponent<ITankData>().GetTankData().TankEnemyParam;
        _scanPlayerCollider.radius = _enemyParam._enemyScanRadius;
        _scanFieldCollider.radius = _scanFieldRange;

        _scanPlayerCollider
            .OnTriggerEnterAsObservable()
            .Where(x => x.CompareTag("Player"))
            .Subscribe(x =>
            {
                if (!_playerCollider)
                {
                    _state = EnemyState.Detected;
                    _playerCollider = x;
                }
            })
            .AddTo(this);
            
        _scanPlayerCollider
            .OnTriggerExitAsObservable()
            .Where(x => x.CompareTag("Player"))
            .Subscribe(x =>
            {
                var cols = Physics.OverlapSphere(transform.position, _enemyParam._enemyScanRadius);
                _playerCollider = cols.FirstOrDefault(x => x.CompareTag("Player"));
                if (_playerCollider)
                {
                    _state = EnemyState.Detected;
                }
                _state = EnemyState.Searching;
                
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
    void OnEnable()
    {
        cts?.Dispose();
        cts = new CancellationTokenSource();
        _moveFlag = true;
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

    private void FixedUpdate()
    {
        switch (_state)
        {
            case EnemyState.Waiting :
                break;
            case EnemyState.Searching :
                break;
            case EnemyState.Detected :
                DetectPlayer(_playerCollider);
                break;
        }
    }

    public async UniTask AutoMover()
    { 
        while (_moveFlag)
        {
            await Move().SuppressCancellationThrow();
            await UniTask.Delay((int)(_enemyParam._moveDelay * 1000));
        }
    }
    public async UniTask Move()
    {
        var randomQua = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360f), 0f);
        _moveDir = randomQua * transform.forward * _enemyParam._scanMoveRange;
        Debug.DrawRay(transform.position, _moveDir, Color.green , 5f);
        Physics.BoxCast(transform.position, Vector3.one * transform.lossyScale.x, _moveDir, out RaycastHit hit , Quaternion.identity , _enemyParam._scanMoveRange);
        try
        {
            if (hit.collider?.gameObject.tag != "Field" && hit.collider?.gameObject.tag != "Player")
            {
                var previousPosition = transform.position;
                while ((transform.position - previousPosition).magnitude < _moveDir.magnitude)
                {
                    if(Vector3.Angle(transform.forward, _moveDir) >= 1f)
                    {
                        if (IsInferiorAngle(transform.forward, _moveDir))
                        {
                            _tankMovement?.Turn(-1.0f);
                            _tankMovement.BarrelTurn(1.0f);
                        }
                        else
                        {
                            _tankMovement?.Turn(1.0f);
                            _tankMovement.BarrelTurn(-1.0f);
                        }
                    }
                    _tankMovement.Move(1.0f);
                    await UniTask.Yield(cancellationToken: cts.Token);
                    if (_isNearField)
                    {
                        _isNearField = false;
                        break;
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Stop");
        }
    }
    //Detectorにプレイヤーが振れている間呼ばれる。
    public void DetectPlayer(Collider player)
    {
        if (!_moveFlag) return;
        var playerDir = player.transform.position - transform.position;
        Physics.Raycast(transform.position ,playerDir , out RaycastHit hit, _enemyParam._enemyScanRadius);
        Debug.DrawRay(transform.position, playerDir, Color.red);
        if (hit.collider?.gameObject.tag == "Player")
        {
            DetectedPlayer(player);
        }
        else
        {
            ResetRotation();
        }
    }
    public void DetectedPlayer(Collider player)
    {
        //円でサーチ
        var bullelTransform = _tankMovement.BrrelTransform;
        var toPlayerVec = player.gameObject.transform.position - transform.position;
        //外積を利用して最速方向に回転する
        if (IsInferiorAngle(bullelTransform.forward, toPlayerVec))
        {
            //左回り
            _enemyParam._targetSpeed = -Mathf.Abs(_enemyParam._targetSpeed);
        }
        else
        {
            //右回り
            _enemyParam._targetSpeed = Mathf.Abs(_enemyParam._targetSpeed);
        }
        var angleToPlayer = Vector3.Angle(bullelTransform.forward, toPlayerVec);
        if (angleToPlayer < 5f)
        {
            _tankAction.ReadyToFire();
            if (angleToPlayer > 2f)
            {
                _tankMovement.BarrelTurn(_enemyParam._targetSpeed);
            }
        }
        else
        {
            _tankMovement.BarrelTurn(_enemyParam._targetSpeed);
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

    void ResetRotation()
    {
        if(IsInferiorAngle( _tankMovement.BrrelTransform.forward , transform.forward) )
        {
            if(Vector3.Angle(transform.forward, _tankMovement.BrrelTransform.forward) > 2f)
            {
                _tankMovement.BarrelTurn(-1.0f);
            }
        }
        else
        {
            if (Vector3.Angle(transform.forward, _tankMovement.BrrelTransform.forward) > 2f)
            {
                _tankMovement.BarrelTurn(1.0f);
            }
        }
    }
}
