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
    [SerializeField] SphereCollider _scanPlayerCollider;
    [SerializeField] SphereCollider _scanFieldCollider;
    [SerializeField] float _scanFieldRange = 3f;
    TankEnemyParam _enemyParam;
    TankMovement _tankMovement;
    TankAction _tankAction;
    Vector3 _moveDir;
    CancellationTokenSource cts;
    bool _moveFlag = false;
    bool _isNearField = false;
    void Awake()
    {
        _tankMovement = GetComponent<TankMovement>();
        _tankAction = GetComponent<TankAction>();
        _enemyParam = GetComponent<ITankData>().GetTankData().TankEnemyParam;
        _scanPlayerCollider.radius = _enemyParam._enemyScanRadius;
        _scanFieldCollider.radius = _scanFieldRange;
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
    //void OnDrawGizmos()
    //{
    //    var scale = transform.lossyScale.x;
    //    var isHit = Physics.BoxCast(transform.position, Vector3.one * scale, _moveDir, out RaycastHit hit, Quaternion.identity, _enemyParam._scanMoveRange);

    //    if (isHit)
    //    {
    //        Gizmos.DrawRay(transform.position, transform.forward * hit.distance);
    //        Gizmos.DrawWireCube(transform.position + transform.forward * hit.distance, Vector3.one * scale * 2);
    //    }
    //}
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
        Ray ray = new Ray(transform.position, _moveDir );
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
                        }
                        else
                        {
                            _tankMovement?.Turn(1.0f);
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
                //while (Vector3.Angle(transform.forward, _moveDir) >= 1f)
                //{
                //    if(IsInferiorAngle(transform.forward , _moveDir))
                //    {
                //        _tankMovement?.Turn(-1.0f);
                //    }
                //    else
                //    {
                //        _tankMovement?.Turn(1.0f);
                //    }

                //    await UniTask.Yield(cancellationToken: cts.Token);
                //}
                //var previousPosition = transform.position;
                //while ((transform.position - previousPosition).magnitude  <  _moveDir.magnitude )
                //{
                //    _tankMovement.Move(1.0f);
                //    await UniTask.Yield(cancellationToken: cts.Token);
                //}
            }
        }
        catch
        {
            Debug.Log("Stop");
        }
    }
    public void IsHitField(Collider field)
    {
        _isNearField = true;
    }
    public void IsOutField(Collider field)
    {
        _isNearField = false;
    }
    public void DetectPlayer(Collider player)
    {
        if (!_moveFlag) return;
        Physics.Raycast(transform.position , player.transform.position - transform.position, out RaycastHit hit, _enemyParam._enemyScanRadius);
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
            _tankAction.OnFire(true);
            if (angleToPlayer > 2f)
            {
                _tankMovement.BarrelTurn(_enemyParam._targetSpeed);
            }
        }
        else
        {
            _tankMovement.BarrelTurn(_enemyParam._targetSpeed);
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
