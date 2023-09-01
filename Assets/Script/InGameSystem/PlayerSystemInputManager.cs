using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSystemInputManager : MonoBehaviour
{
    private static PlayerSystemInputManager instance = null;
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
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
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
}
