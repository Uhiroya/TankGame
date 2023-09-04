using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTankTrali : MonoBehaviour
{
    [SerializeField] GameObject _leftBackWheel;
    [SerializeField] GameObject _rightBackWheel;
    [SerializeField] GameObject _trailObject;
    [SerializeField] float _makeTrailDistance;
    private Vector3 _prevPos;
    private bool _animflag = false;
    private void FixedUpdate()
    {
        if (_animflag && new Vector2(transform.position.x - _prevPos.x, transform.position.z - _prevPos.z).magnitude > _makeTrailDistance)
        {
            _prevPos = transform.position;
            var obj =Instantiate(_trailObject, _trailObject.transform.position + _leftBackWheel.transform.position, Quaternion.Euler(_trailObject.transform.localEulerAngles.x, transform.localEulerAngles.y, 0));
            obj.gameObject.hideFlags = HideFlags.HideInHierarchy;
            var obj2 =Instantiate(_trailObject, _trailObject.transform.position + _rightBackWheel.transform.position, Quaternion.Euler(_trailObject.transform.localEulerAngles.x, transform.localEulerAngles.y, 0));
            obj2.gameObject.hideFlags = HideFlags.HideInHierarchy;

        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag =="Ground")
        {
            _animflag = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "Ground")
        {
            _animflag = false;
        }
    }
}
