using Cysharp.Threading.Tasks;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class TankMovement : MonoBehaviourPunCallbacks, IAnimAwake
{
    [FormerlySerializedAs("startPositionY")] [SerializeField]
    private float _startPositionY = 50f;

    public Transform BrrelTransform;
    private Rigidbody _rb;
    private TankMoveParam _tankMoveParam;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _tankMoveParam = GetComponent<ITankData>().GetTankData().TankMoveParam;
        transform.position += Vector3.up * _startPositionY;
    }

    private void OnEnable()
    {
        MyServiceLocator.IRegister(this as IAnimAwake);
    }

    private void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IAnimAwake);
    }

    public async UniTask AnimAwake(float time)
    {
        print("Start");
        var pos = transform.position;
        _ = transform.DOMove(new Vector3(pos.x, 0, pos.z), time);
        await transform.DOLocalRotate(new Vector3(0, 360, 0), time, RotateMode.FastBeyond360);
    }

    public void Move(float inputVertical)
    {
        photonView.RPC(nameof(PunMove), RpcTarget.All, inputVertical);
    }

    [PunRPC]
    public void PunMove(float inputVertical)
    {
        var movement = transform.forward * (inputVertical * _tankMoveParam._moveSpeed);
        _rb.velocity = movement;
    }

    public void Turn(float inputHorizontal)
    {
        photonView.RPC(nameof(PunTurn), RpcTarget.All, inputHorizontal);
    }

    [PunRPC]
    public void PunTurn(float inputHorizontal)
    {
        var turn = inputHorizontal * _tankMoveParam._turnMoveSpeed;
        var turnRotation = Quaternion.Euler(0f, turn, 0f);
        transform.rotation *= turnRotation;
    }

    public void BarrelTurn(float inputVertical)
    {
        photonView.RPC(nameof(PunBarrelTurn), RpcTarget.All, inputVertical);
    }

    [PunRPC]
    public void PunBarrelTurn(float inputVertical)
    {
        var turn = inputVertical * _tankMoveParam._turnBarrelSpeed;
        var turnRotation = Quaternion.Euler(0f, turn, 0f);
        BrrelTransform.rotation *= turnRotation;
    }
}