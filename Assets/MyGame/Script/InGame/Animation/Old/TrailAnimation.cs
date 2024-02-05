using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TrailAnimation : MonoBehaviour
{
    [SerializeField] float lifeTime = 2.5f;
    float _timer = 0f ;
    void Start()
    {
        Destroy(gameObject,lifeTime);
    }
    private void Update()
    {
        _timer += Time.deltaTime;
        Color color = GetComponent<SpriteRenderer>().color;
        color.a = (lifeTime - _timer) / lifeTime ;
        GetComponent<SpriteRenderer>().color = color ;
    }
}
