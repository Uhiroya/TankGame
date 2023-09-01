using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PauseManager 
{
    public static bool IsPause = false;
    public static void Pause()
    {
        if(!IsPause)
        {
            MyServiceLocator.IResolve<IPause>().OfType<IPause>().ToList().ForEach(x => x.Pause());
            IsPause = true;
        }
        else
        {
            MyServiceLocator.IResolve<IPause>().OfType<IPause>().ToList().ForEach(x => x.Resume());
            IsPause = false;
        }
    }
}
