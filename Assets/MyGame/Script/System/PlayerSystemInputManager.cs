using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSystemInputManager : MonoBehaviour , IStart
{
    private static PlayerSystemInputManager instance = null;
    bool _isActive = false;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }
    void OnEnable()
    {
        MyServiceLocator.IRegister(this as IStart);
    }

    void OnDisable()
    {
        MyServiceLocator.IUnRegister(this as IStart);
    }
    void Update()
    {
        if (Input.GetButtonDown("Cancel") && _isActive)
        {
            PauseManager.Pause();
            if(PauseManager.IsPause)
            {
                AudioManager.Instance._audioBGMSource.volume *= 0.25f ;
                SceneUIManager.Instance?.Pause();
            }
            else
            {
                AudioManager.Instance._audioBGMSource.volume *= 4f;
                SceneUIManager.Instance?.Resume();
            }
        }
    }
    public void Active()
    {
        _isActive = true; 
    }

    public void DeActive()
    {
        _isActive = false;
    }
}
