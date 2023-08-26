using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PauseManager 
{
    public static bool IsPause = false;
    internal static List<IPause> PauseScripts = new List<IPause>();
    public static void Pause()
    {
        if(!IsPause)
        {
            foreach(var pauseScript in PauseScripts)
            {
                pauseScript?.Pause();
            }
            IsPause = true;
        }
        else
        {
            foreach (var pauseScript in PauseScripts)
            {
                pauseScript?.Resume();
            }
            IsPause = false;
        }
    }
}
