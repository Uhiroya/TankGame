using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystemInputManager : MonoBehaviour
{
    bool _isPause = false;
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            var objs = MyServiceLocator.IResolve<IPause>();
            if (objs != null)
            {
                if (!_isPause)
                {
                    print("cancel");
                    foreach (IPause obj in objs)
                    {
                        obj.Pause();
                    }
                    _isPause = true;
                }
                else
                {
                    foreach (IPause obj in objs)
                    {
                        obj.Resume();
                    }
                    _isPause = false;
                }
            }
        }
    }
}
