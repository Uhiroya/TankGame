using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    private TankMovement _tankMovement;
    private TankAction _tankAction;

    //input
    private float _inputMoveVertical;
    private float _inputMoveHorizontal;
    //BarrelRotation
    private float _inputVertical;
    private float _inputHorizontal;

    private bool _active = false;
    void Awake()
    {
        _tankMovement = GetComponent<TankMovement>();
        _tankAction = GetComponent<TankAction>();
    }

    void OnEnable()
    {
        _active = true;
    }

    void OnDisable()
    {
        _active = false;
    }

    void Start()
    {
        
    }
    private void FixedUpdate()
    {
        _tankMovement.Move(_inputMoveVertical);
        _tankMovement.Turn(_inputMoveHorizontal);
        _tankMovement.BarrelTurn(_inputHorizontal);
    }
    void Update()
    {
        if( _active)
        {
            _inputMoveVertical = Input.GetAxis("Vertical");
            _inputMoveHorizontal = Input.GetAxis("Horizontal");

            _inputVertical = Input.GetAxis("Vertical2");
            _inputHorizontal = Input.GetAxis("Horizontal2");
            if (Input.GetButtonDown("Fire1"))
            {
                _tankAction.Fire();
            }
        }
    }
}
