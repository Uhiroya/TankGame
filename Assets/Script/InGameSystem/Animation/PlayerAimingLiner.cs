using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimingLiner : MonoBehaviour
{
    [SerializeField] float _cursorRange = 20f;
    private TankMovement _tankMovement;
    private LineRenderer _lineRenderer;
    void Start()
    {
        _tankMovement = GetComponentInParent<TankMovement>();
        _lineRenderer = GetComponent<LineRenderer>();
    }
    void Update()
    {
        _lineRenderer.SetPosition(0, transform.position);
        var dir = _tankMovement.BrrelTransform.forward;
        Ray ray = new Ray(transform.position, dir);
        Debug.DrawRay(transform.position, dir * _cursorRange, Color.green , 1f);
        if(Physics.Raycast(ray,  out RaycastHit hit , _cursorRange) && hit.transform?.gameObject.tag == "Field")
        {
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(1, hit.point) ;
            Vector3 RefDir = Vector3.Reflect(_lineRenderer.GetPosition(1) - _lineRenderer.GetPosition(0), hit.normal).normalized;
            var dis = (_lineRenderer.GetPosition(1) - _lineRenderer.GetPosition(0)).magnitude;
            _lineRenderer.SetPosition(2, hit.point + RefDir * 2f);
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }
}
