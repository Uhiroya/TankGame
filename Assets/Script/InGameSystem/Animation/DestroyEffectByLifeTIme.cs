using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEffectByLifeTIme : MonoBehaviour , IPause
{
    [SerializeField] float _lifeTime = 1.5f;
    float _timer = 0f ;
    void OnEnable()
    {
        MyServiceLocator.IRegister(this as IPause);
    }

    void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IPause);
    }

    void Update()
    {
        if (!PauseManager.IsPause)
        {
            _timer += Time.deltaTime;
            if (_timer > _lifeTime)
            {
                Destroy(gameObject );
            }
        }
    }
    public void Pause()
    {
        GetComponent<ParticleSystem>().Pause();
    }
    public void Resume()
    {
        GetComponent<ParticleSystem>().Play();
    }
}
