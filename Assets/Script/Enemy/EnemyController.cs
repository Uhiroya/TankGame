using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float _targetSpeed = 1f;
    [SerializeField] float _detectSpeed = 3f;
    [SerializeField] float _enemyScanRadius = 20f;
    [SerializeField] SphereCollider _scanCollider;
    [SerializeField] float _scanMoveRange = 1.0f;
    [SerializeField] float _moveDelay = 1.0f;
    private TankMovement _tankMovement;
    private TankAction _tankAction;
    Vector3 _moveDir;
    Vector3 _detectDir;
    CancellationTokenSource cts;
    bool _moveFlag = false;
    Ray _detectorRay = new();
    Ray _targetRay = new();
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
        _moveFlag = true;
        _ = AutoMover();
    }

    void OnDisable()
    {
        _moveFlag = false;
        transform.DOKill();
        cts?.Cancel();
    }
    private void OnDestroy()
    {
        cts?.Dispose();
    }
    void Start()
    {
        _detectDir = transform.forward;
        _moveDir = transform.forward;

        _detectorRay.origin = transform.position;
        _detectorRay.direction = _detectDir;
        var bullelTransform = _tankMovement._barrelTranform;
        _targetRay.origin = bullelTransform.position;
        _targetRay.direction = bullelTransform.forward;
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
        //print("呼ばれた");
        Debug.DrawRay(transform.position, _moveDir * _scanMoveRange, Color.green , 5f);
        var a = Physics.Raycast(ray, out RaycastHit hit, _scanMoveRange);
        try
        {
            print(a);
            print(hit.collider?.gameObject.tag);
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
        Physics.Raycast(_detectorRay, out RaycastHit hit, _enemyScanRadius);
        Debug.DrawRay(transform.position, _detectDir * _enemyScanRadius, Color.red);
        if (hit.collider?.gameObject.tag == "Player")
        {
            DetectedPlayer(player);
        }
        else
        {
            _detectDir = Quaternion.Euler(0f, _targetSpeed * _detectSpeed, 0f) * _detectDir;
            _detectorRay.origin = transform.position;
            _detectorRay.direction = _detectDir;
        }

    }
     public void DetectedPlayer(Collider player)
    {
        //円でサーチ
        var bullelTransform = _tankMovement._barrelTranform;

        Debug.DrawRay(bullelTransform.position , bullelTransform.forward * _enemyScanRadius, Color.blue);

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
        if(Vector3.Angle(bullelTransform.forward , toPlayerVec) < 5f)
        {
            
            if (Vector3.Angle(bullelTransform.forward, toPlayerVec) < 2f)
            {
                _tankAction.OnFire(true);
            }
            else
            {
                _tankMovement.BarrelTurn(_targetSpeed);
            }
        }
        else
        {
            _tankMovement.BarrelTurn(_targetSpeed);
            _tankAction.OnFire(false);
        }

        //if (Physics.Raycast(_targetRay, out RaycastHit hit2, _enemyScanRadius))
        //{
        //    if (hit2.collider.gameObject.tag == "Player")
        //    {
        //        _targetRay.origin = bullelTransform.position;
        //        _targetRay.direction = hit2.collider.gameObject.transform.position;
                
        //    }
        //    else
        //    {
        //        _tankAction.OnFire(false);
        //        _tankMovement.BarrelTurn(_targetSpeed);
        //        _targetRay.origin = bullelTransform.position;
        //        _targetRay.direction = bullelTransform.forward;
        //    }
        //}
        //else
        //{
        //    _tankMovement.BarrelTurn(_targetSpeed);
        //    _targetRay.origin = bullelTransform.position;
        //    _targetRay.direction = bullelTransform.forward;
        //}
    }
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
