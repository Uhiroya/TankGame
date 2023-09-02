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
                SceneUIManager.Instance?.Pause();
            }
            else
            {
                SceneUIManager.Instance?.Resume();
            }
        }
    }

    public void Active()
    {
        _isActive = true; 
    }

    public void InActive()
    {
        _isActive = false;
    }
}
