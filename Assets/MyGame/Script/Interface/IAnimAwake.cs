﻿using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IAnimAwake 
{
    UniTask AnimAwake(float time);
}
