using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IAwakeAnim 
{
    UniTask AnimAwake(float time);
}
